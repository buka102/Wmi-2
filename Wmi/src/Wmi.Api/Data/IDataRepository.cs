using Wmi.Api.Models;

namespace Wmi.Api.Data;

public interface IDataRepository
{

    Task<IEnumerable<Product>> GetProductsAsync(string? titleContains = null, string? titleStartsWith = null, int page = 1, int pageSize = 10);
    Task<IEnumerable<Product>> GetProductsWithBuyersAsync(string? titleContains = null, string? titleStartsWith = null, int page = 1, int pageSize = 10);
    Task<Product?> GetProductBySkuAsync(string sku);
    Task<bool> ExistsProductBySkuAsync(string sku);
    Task<bool> ExistsProductWithBuyerIdAsync(string buyer);
    Task<bool> InsertProductAsync(Product draftProduct);
    Task<bool> UpdateProductAsync(Product draftProduct);
    Task<bool> DeleteProductAsync(string sku);
    
    Task<IEnumerable<Buyer>> GetBuyersAsync(int page = 1, int pageSize = 10);
    Task<Buyer?> GetBuyerByIdAsync(string id);
    Task<bool> InsertBuyerAsync(Buyer draftBuyer);
    Task<bool> UpdateBuyerAsync(Buyer draftBuyer);
    Task<bool> DeleteBuyerAsync(string id);
    Task<bool> ExistsBuyerAsync(string id);
    Task<bool> ExistsBuyerByEmailAsync(string email);
    

}