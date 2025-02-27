using System.Text;
using Dapper;
using Wmi.Api.Models;

namespace Wmi.Api.Data;

public class DataRepository(IDbConnectionFactory connectionFactory, IConfiguration configuration)
    : IDataRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private readonly uint _maxRowsReturn = configuration.GetValue<uint>("maxGetRecords", 1000); // Ensure a positive default

    public async Task<IEnumerable<Product>> GetProductsAsync(string? titleContains = null, string? titleStartsWith = null, int page = 1, int pageSize = 10)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        // Build the WHERE clause dynamically
        var sqlBuilder = new StringBuilder(
            @"SELECT p.sku, p.title, p.description, p.active, p.buyer_id AS BuyerId
              FROM products p WHERE 1=1");

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(titleContains))
        {
            sqlBuilder.Append(" AND p.title ILIKE @TitleContains");
            parameters.Add("TitleContains", $"%{titleContains}%");
        }

        if (!string.IsNullOrEmpty(titleStartsWith))
        {
            sqlBuilder.Append(" AND p.title ILIKE @TitleStartsWith");
            parameters.Add("TitleStartsWith", $"{titleStartsWith}%");
        }

        // Add pagination with maxRowsReturn safety
        page = Math.Max(1, page); // Ensure page is at least 1
        pageSize = Math.Max(1, Math.Min(pageSize, (int)_maxRowsReturn)); // Cap pageSize at maxRowsReturn
        int offset = (page - 1) * pageSize;

        sqlBuilder.Append(" ORDER BY p.sku LIMIT @Limit OFFSET @Offset");
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var products = await connection.QueryAsync<Product>(sqlBuilder.ToString(), parameters);
        return products;
    }
    
    public async Task<IEnumerable<Product>> GetProductsWithBuyersAsync(string? titleContains = null, string? titleStartsWith = null, int page = 1, int pageSize = 10)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        // Build the WHERE clause dynamically
        var sqlBuilder = new StringBuilder(
            @"SELECT p.sku, p.title, p.description, p.buyer_id AS BuyerId, p.active,
                     b.id, b.name, b.email
              FROM products p
              LEFT JOIN buyers b ON p.buyer_id = b.id WHERE 1=1");

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(titleContains))
        {
            sqlBuilder.Append(" AND p.title ILIKE @TitleContains");
            parameters.Add("TitleContains", $"%{titleContains}%");
        }

        if (!string.IsNullOrEmpty(titleStartsWith))
        {
            sqlBuilder.Append(" AND p.title ILIKE @TitleStartsWith");
            parameters.Add("TitleStartsWith", $"{titleStartsWith}%");
        }

        // Add pagination with maxRowsReturn safety
        page = Math.Max(1, page); // Ensure page is at least 1
        pageSize = Math.Max(1, Math.Min(pageSize, (int)_maxRowsReturn)); // Cap pageSize at maxRowsReturn
        int offset = (page - 1) * pageSize;

        sqlBuilder.Append(" ORDER BY p.sku LIMIT @Limit OFFSET @Offset");
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var products = await connection.QueryAsync<Product, Buyer, Product>(
            sqlBuilder.ToString(),
            (product, buyer) =>
            {
                product.Buyer = buyer;
                return product;
            },
            parameters,
            splitOn: "id");

        return products;
    }

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        
        var product = await connection.QueryFirstOrDefaultAsync<Product>(
            @"SELECT p.sku, p.title, p.description, p.buyer_id AS BuyerId, p.active
              FROM products p
              WHERE p.sku = @SKU",
            new { SKU = sku });

        return product;
    }

    public async Task<bool> ExistsProductBySkuAsync(string sku)
    {
        if (string.IsNullOrEmpty(sku))
            return false; // Early return for invalid input

        using var connection = await _connectionFactory.CreateConnectionAsync();

        var exists = await connection.ExecuteScalarAsync<bool>(
            @"SELECT EXISTS (
                  SELECT 1 
                  FROM products 
                  WHERE sku = @Sku
              )",
            new { Sku = sku });

        return exists;
    }

    public async Task<bool> ExistsProductWithBuyerIdAsync(string buyerId)
    {
        if (string.IsNullOrEmpty(buyerId))
            return false; // Early return for invalid input

        using var connection = await _connectionFactory.CreateConnectionAsync();

        var exists = await connection.ExecuteScalarAsync<bool>(
            @"SELECT EXISTS (
                  SELECT 1 
                  FROM products 
                  WHERE buyer_id = @BuyerId
              )",
            new { @BuyerId = buyerId});

        return exists;
    }

    public async Task<bool> InsertProductAsync(Product product)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var rowsAffected = await connection.ExecuteAsync(
            @"INSERT INTO products (sku, title, description, buyer_id, active)
              VALUES (@SKU, @Title, @Description, @BuyerId, @Active)",
            new
            {
                product.SKU,
                product.Title,
                product.Description,
                product.BuyerId,
                product.Active
            });

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateProductAsync(Product draftProduct)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var rowsAffected = await connection.ExecuteAsync(
            @"UPDATE products 
              SET title = @Title,
                  description = @Description,
                  buyer_id = @BuyerId,
                  active = @Active
              WHERE sku = @SKU",
            new
            {
                draftProduct.SKU,
                draftProduct.Title,
                draftProduct.Description,
                draftProduct.BuyerId,
                draftProduct.Active
            });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteProductAsync(string sku)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var rowsAffected = await connection.ExecuteAsync(
            "DELETE FROM products WHERE sku = @Sku",
            new { Sku = sku });

        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Buyer>> GetBuyersAsync(int page = 1, int pageSize = 10)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        // Build the WHERE clause dynamically
        var sqlBuilder = new StringBuilder(
            @"SELECT b.id, b.name, b.email
              FROM buyers b WHERE 1=1");

        var parameters = new DynamicParameters();

        // Add pagination with maxRowsReturn safety
        page = Math.Max(1, page); // Ensure page is at least 1
        pageSize = Math.Max(1, Math.Min(pageSize, (int)_maxRowsReturn)); // Cap pageSize at maxRowsReturn
        int offset = (page - 1) * pageSize;

        sqlBuilder.Append(" ORDER BY b.id LIMIT @Limit OFFSET @Offset");
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var buyers = await connection.QueryAsync<Buyer>(sqlBuilder.ToString(), parameters);
        return buyers;
    }

    public async Task<Buyer?> GetBuyerByIdAsync(string id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var buyer = await connection.QueryFirstOrDefaultAsync<Buyer>(
            @"SELECT id, name, email
              FROM buyers
              WHERE id = @Id",
            new { Id = id });

        return buyer;
    }

    public async Task<bool> InsertBuyerAsync(Buyer draftBuyer)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var rowsAffected = await connection.ExecuteAsync(
            @"INSERT INTO buyers (id, name, email)
              VALUES (@Id, @Name, @Email)",
            new
            {
                draftBuyer.Id,
                draftBuyer.Name,
                Email = draftBuyer.Email.ToLower()
            });

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateBuyerAsync(Buyer draftBuyer)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var rowsAffected = await connection.ExecuteAsync(
            @"UPDATE buyers 
              SET name = @Name,
                  email = @Email
              WHERE id = @Id", 
            new
            {
                draftBuyer.Id,
                draftBuyer.Name,
                Email = draftBuyer.Email.ToLower()
            });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteBuyerAsync(string id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var rowsAffected = await connection.ExecuteAsync(
            "DELETE FROM buyers WHERE id = @Id",
            new { Id = id });

        return rowsAffected > 0;
    }

    public async Task<bool> ExistsBuyerAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false; // Early return for invalid input

        using var connection = await _connectionFactory.CreateConnectionAsync();

        var exists = await connection.ExecuteScalarAsync<bool>(
            @"SELECT EXISTS (
                  SELECT 1 
                  FROM buyers 
                  WHERE id = @Id
              )",
            new { Id = id });

        return exists;
    }

    public async Task<bool> ExistsBuyerByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false; // Early return for invalid input

        using var connection = await _connectionFactory.CreateConnectionAsync();

        var exists = await connection.ExecuteScalarAsync<bool>(
            @"SELECT EXISTS (
                  SELECT 1 
                  FROM buyers 
                  WHERE LOWER(email) = LOWER(@Email)
              )",
            new { Email = email});

        return exists;
    }
}