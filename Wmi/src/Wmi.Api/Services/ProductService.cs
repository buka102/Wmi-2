using FluentValidation;
using Wmi.Api.Data;
using Wmi.Api.Models;

namespace Wmi.Api.Services;

public class ProductService(IDataRepository dataRepository, IBuyerService buyerService, IValidator<Product> validator, INotify notify): IProductService
{
    public async Task<Result<List<Product>>> GetProductsAsync(string? titleContains = null,
        string? titleStartsWith = null, int page = 1, int pageSize = 10, bool includeBuyer = false)
    {

        IEnumerable<Product> products;
        if (includeBuyer)
        {
            products = await dataRepository.GetProductsWithBuyersAsync(titleContains, titleStartsWith, page, pageSize);
        }
        else
        {
            products = await dataRepository.GetProductsAsync(titleContains, titleStartsWith, page, pageSize);
        }

        return Result<List<Product>>.Ok(products.ToList());
    }

    public async Task<Result<Product>> CreateProductAsync(string sku, string title, string? description, string buyerId,
        bool active)
    {

        var productExistsBySku = await dataRepository.ExistsProductBySkuAsync(sku);
        if (productExistsBySku)
        {
            return Result<Product>.Fail("Sku already exists");
        }

        var buyerExists = await buyerService.ExistsBuyerAsync(buyerId);
        if (!buyerExists.Success || !buyerExists.Value)
        {
            return Result<Product>.Fail("buyerId is invalid");
        }

        var draftProduct = new Product
        {
            SKU = sku,
            Title = title,
            Description = description,
            BuyerId = buyerId,
            Active = active
        };

        var validationResult = await validator.ValidateAsync(draftProduct);
        if (!validationResult.IsValid)
        {
            return Result<Product>.Fail(string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)));
        }


        var newProductSuccess = await dataRepository.InsertProductAsync(new()
            { SKU = sku, Title = title, BuyerId = buyerId, Active = active });
        if (!newProductSuccess)
        {
            return Result<Product>.Fail("failed to create product");
        }

        notify.Notify(buyerId, $"new product (sku: '{draftProduct.SKU})' is created");
        return Result<Product>.Ok(draftProduct);
    }

    public async Task<Result<Product>> ChangeActiveStatusAsync(string sku, bool active)
    {
        var productBySku = await dataRepository.GetProductBySkuAsync(sku);
        if (productBySku == null)
        {
            return Result<Product>.Fail("Sku is out of range");
        }

        if (productBySku.Active != active)
        {
            productBySku.Active = active;
            var updateProductResult = await dataRepository.UpdateProductAsync(productBySku);
            if (updateProductResult && !productBySku.Active)
            {
                notify.Notify(productBySku.BuyerId, $"product (sku: '{productBySku.SKU})' has been deactivated");
            }
        }
        return Result<Product>.Ok(productBySku);
    }

    public async Task<Result<Product>> ChangeBuyerAsync(string sku, string newBuyerId)
    {
        
        var buyerExists = await buyerService.ExistsBuyerAsync(newBuyerId);
        if (!buyerExists.Success || !buyerExists.Value)
        {
            return Result<Product>.Fail("buyerId is invalid");
        }
        
        var productBySku = await dataRepository.GetProductBySkuAsync(sku);
        if (productBySku == null)
        {
            return Result<Product>.Fail("Sku is out of range");
        }

        if (productBySku.BuyerId != newBuyerId)
        {
            var previousBuyerId = productBySku.BuyerId;
            productBySku.BuyerId = newBuyerId;
            var updateProductResult = await dataRepository.UpdateProductAsync(productBySku);
            if (updateProductResult)
            {
                notify.Notify(previousBuyerId, $"product (sku: '{productBySku.SKU}') has been unassigned from you");
                notify.Notify(productBySku.BuyerId, $"product (sku: '{productBySku.SKU}') has been assigned to you");
            }
        }
        return Result<Product>.Ok(productBySku);
    }
}