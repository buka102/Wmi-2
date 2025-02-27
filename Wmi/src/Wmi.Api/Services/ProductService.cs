using AutoMapper;
using FluentValidation;
using Wmi.Api.Data;
using Wmi.Api.Models;
using Wmi.Api.Models.Dto;

namespace Wmi.Api.Services;

public class ProductService(
    IDataRepository dataRepository,
    IBuyerService buyerService,
    IValidator<Product> validator,
    INotify notify,
    IMapper mapper) : IProductService
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

    public async Task<Result<Product>> CreateProductAsync(CreateProductDto productDto)
    {
        var productExistsBySku = await dataRepository.ExistsProductBySkuAsync(productDto.Sku);
        if (productExistsBySku)
        {
            return Result<Product>.Fail("Sku already exists");
        }

        // Moved to Validator
        // var buyerExists = await buyerService.ExistsBuyerAsync(productDto.BuyerId);
        // if (!buyerExists.Success || !buyerExists.Value)
        // {
        //     return Result<Product>.Fail("buyerId is invalid");
        // }

        var draftProduct = mapper.Map<Product>(productDto);

        var validationResult = await validator.ValidateAsync(draftProduct);
        if (!validationResult.IsValid)
        {
            return Result<Product>.Fail(string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        var newProductSuccess = await dataRepository.InsertProductAsync(draftProduct);
        if (!newProductSuccess)
        {
            return Result<Product>.Fail("failed to create product");
        }

        notify.Notify(draftProduct.BuyerId, $"new product (sku: '{draftProduct.Sku})' is created");
        return Result<Product>.Ok(draftProduct);
    }

    public async Task<Result<Product>> UpdateProductAsync(string sku, UpdateProductDto productDto)
    {
        var existingProductBySku = await dataRepository.GetProductBySkuAsync(sku);
        if (existingProductBySku == null)
        {
            return Result<Product>.Fail("Sku does not exists");
        }

        var productChangedToDeactivated = false;
        var productAssignedBuyerChanged = false;
        var previousProductBuyerId = string.Empty;
        
        var draftProduct = mapper.Map<Product>(productDto);
        draftProduct.Sku = existingProductBySku.Sku;

        var validationResult = await validator.ValidateAsync(draftProduct);
        if (!validationResult.IsValid)
        {
            return Result<Product>.Fail(string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        if (existingProductBySku.BuyerId != draftProduct.BuyerId)
        {
            productAssignedBuyerChanged = true;
            previousProductBuyerId = existingProductBySku.BuyerId;
        }

        if (existingProductBySku.Active != draftProduct.Active && !draftProduct.Active)
        {
            productChangedToDeactivated = true;
        }

        var updateResult = await dataRepository.UpdateProductAsync(draftProduct);
        if (!updateResult)
        {
            return Result<Product>.Fail("Product updated failed");
        }

        if (productAssignedBuyerChanged)
        {
            NotifyProductChangedBuyer(draftProduct.Sku, previousProductBuyerId, draftProduct.BuyerId);
        }

        if (productChangedToDeactivated)
        {
            //Here we notify only latest buyer (if buyer was changed, here we can provide previousProductBuyerId)
            NotifyProductDeactivated(draftProduct.Sku, draftProduct.BuyerId);
        }
        
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
                NotifyProductDeactivated(productBySku.Sku, productBySku.BuyerId);
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
                NotifyProductChangedBuyer(productBySku.Sku, previousBuyerId, newBuyerId);
            }
        }

        return Result<Product>.Ok(productBySku);
    }

    private void NotifyProductDeactivated(string sku, string buyerId)
    {
        notify.Notify(buyerId, $"product (sku: '{sku})' has been deactivated");
    }

    private void NotifyProductChangedBuyer(string sku, string previousBuyerId, string newBuyerId)
    {
        notify.Notify(previousBuyerId, $"product (sku: '{sku}') has been unassigned from you");
        notify.Notify(newBuyerId, $"product (sku: '{sku}') has been assigned to you");
    }
}