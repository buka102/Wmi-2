using Wmi.Api.Models;
using Wmi.Api.Models.Dto;

namespace Wmi.Api.Services;

public interface IProductService
{
    Task<Result<List<Product>>> GetProductsAsync(string? titleContains = null, string? titleStartsWith = null,
        int page = 1, int pageSize = 10, bool includeBuyer = false);

    Task<Result<Product>> CreateProductAsync(CreateProductDto productDto);
    
    Task<Result<Product>> ChangeActiveStatusAsync(string sku, bool active);
    Task<Result<Product>> ChangeBuyerAsync(string sku, string newBuyerId);
}