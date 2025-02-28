using AutoMapper;
using FluentValidation;
using Wmi.Api.Data;
using Wmi.Api.Models;
using Wmi.Api.Models.Dto;

namespace Wmi.Api.Services;

public class BuyerService(
    ILogger<BuyerService> logger,
    IDataRepository dataRepository, IValidator<Buyer> validator, IMapper mapper):IBuyerService
{
    public async Task<Result<List<Buyer>>> GetBuyersAsync(int page = 1, int pageSize = 10)
    {
        logger.LogDebug("GetBuyers was called");
        var result = await dataRepository.GetBuyersAsync(page: page, pageSize: pageSize);
        return Result<List<Buyer>>.Ok(result.ToList());
    }

    public async Task<Result<Buyer>> CreateBuyerAsync(CreateBuyerDto buyerDto)
    {
        logger.LogDebug("CreateBuyerAsync was called");
        var buyerExistsByEmail = await dataRepository.ExistsBuyerByEmailAsync(buyerDto.Email);
        if (buyerExistsByEmail)
        {
            logger.LogDebug("Buyer email already exists");
            return Result<Buyer>.Fail("Email already exists");
        }
        
        var newId = Guid.NewGuid().ToString("N").ToLower();

        var draftBuyer = mapper.Map<Buyer>(buyerDto);
        draftBuyer.Id = newId;
        
        var validationResult = await validator.ValidateAsync(draftBuyer);
        if (!validationResult.IsValid)
        {
            logger.LogDebug("CreateBuyerAsync validation failed");
            return Result<Buyer>.Fail(string.Join(", ", validationResult.Errors.Select(x=>x.ErrorMessage)));
        }
        
        var newBuyerSuccess = await dataRepository.InsertBuyerAsync(draftBuyer);
        if (!newBuyerSuccess)
        {
            logger.LogDebug("CreateBuyerAsync failed with inserting new buyer");
            return Result<Buyer>.Fail("failed to create buyer");
        }
        logger.LogDebug("CreateBuyerAsync success");
        return Result<Buyer>.Ok(draftBuyer);
    }

    public async Task<Result<bool>> ExistsBuyerAsync(string id)
    {
        logger.LogDebug("ExistsBuyerAsync was called");
        var buyerExists = await dataRepository.ExistsBuyerAsync(id);
        return Result<bool>.Ok(buyerExists);
    }

    public async Task<Result<Buyer>> UpdateBuyerAsync(string id, UpdateBuyerDto updateBuyerDto)
    {
        logger.LogDebug("UpdateBuyerAsync was called");
        var draftBuyer = mapper.Map<Buyer>(updateBuyerDto);
        draftBuyer.Id = id;
        
        var validationResult = await validator.ValidateAsync(draftBuyer);
        if (!validationResult.IsValid)
        {
            logger.LogDebug("UpdateBuyerAsync validation failed");
            return Result<Buyer>.Fail(string.Join(", ", validationResult.Errors.Select(x=>x.ErrorMessage)));
        }
        
        var updatedResult = await dataRepository.UpdateBuyerAsync(draftBuyer);
        if (!updatedResult)
        {
            logger.LogDebug("UpdateBuyerAsync failed with db update");
            return Result<Buyer>.Fail("failed to update buyer");
        }
        logger.LogDebug("UpdateBuyerAsync success");
        return Result<Buyer>.Ok(draftBuyer);
    }

    public async Task<Result<bool>> DeleteBuyerAsync(string id)
    {
        logger.LogDebug("DeleteBuyerAsync was called");
        var buyerExists = await dataRepository.ExistsBuyerAsync(id);
        if (!buyerExists)
        {
            logger.LogDebug("Buyer does not exists");
            return Result<bool>.Fail("Buyer not found");
        }
        
        var buyerInUse = await dataRepository.ExistsProductWithBuyerIdAsync(id);
        if (buyerInUse)
        { 
            logger.LogDebug("Buyer is in use");
            return Result<bool>.Fail("Buyer is in-use. Cannot be deleted");
        }
        
        var deleteResult = await dataRepository.DeleteBuyerAsync(id);
        if (!deleteResult)
        {
            logger.LogDebug("DeleteBuyerAsync failed in data layer");
            return Result<bool>.Fail("failed to delete buyer");
        }
        logger.LogDebug("DeleteBuyerAsync success");
        return Result<bool>.Ok(true);
    }
}