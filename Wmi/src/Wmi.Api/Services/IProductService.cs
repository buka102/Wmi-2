using Wmi.Api.Models;

namespace Wmi.Api.Services;

public interface IProductService
{
    Task<Result<List<Product>>> GetProductsAsync(string? titleContains = null, string? titleStartsWith = null,
        int page = 1, int pageSize = 10, bool includeBuyer = false);

    Task<Result<Product>> CreateProductAsync(string sku, string title, string? description, string buyerId,
        bool active);
    
    Task<Result<Product>> ChangeActiveStatusAsync(string sku, bool active);
    Task<Result<Product>> ChangeBuyerAsync(string sku, string newBuyerId);
}