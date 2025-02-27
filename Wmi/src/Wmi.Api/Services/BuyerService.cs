using AutoMapper;
using FluentValidation;
using Wmi.Api.Data;
using Wmi.Api.Models;
using Wmi.Api.Models.Dto;

namespace Wmi.Api.Services;

public class BuyerService(IDataRepository dataRepository, IValidator<Buyer> validator, IMapper mapper):IBuyerService
{
    public async Task<Result<List<Buyer>>> GetBuyersAsync(int page = 1, int pageSize = 10)
    {
        var result = await dataRepository.GetBuyersAsync(page: page, pageSize: pageSize);
        return Result<List<Buyer>>.Ok(result.ToList());
    }

    public async Task<Result<Buyer>> CreateBuyerAsync(CreateBuyerDto buyerDto)
    {
        
        var buyerExistsByEmail = await dataRepository.ExistsBuyerByEmailAsync(buyerDto.Email);
        if (buyerExistsByEmail)
        {
            return Result<Buyer>.Fail("Email already exists");
        }
        
        var newId = Guid.NewGuid().ToString("N").ToLower();

        var draftBuyer = mapper.Map<Buyer>(buyerDto);
        draftBuyer.Id = newId;
        
        var validationResult = await validator.ValidateAsync(draftBuyer);
        if (!validationResult.IsValid)
        {
            return Result<Buyer>.Fail(string.Join(", ", validationResult.Errors.Select(x=>x.ErrorMessage)));
        }
        
        var newBuyerSuccess = await dataRepository.InsertBuyerAsync(draftBuyer);
        if (!newBuyerSuccess)
        {
            return Result<Buyer>.Fail("failed to create buyer");
        }
        
        return Result<Buyer>.Ok(draftBuyer);
    }

    public async Task<Result<bool>> ExistsBuyerAsync(string id)
    {
        var buyerExists = await dataRepository.ExistsBuyerAsync(id);
        return Result<bool>.Ok(buyerExists);
    }

    public async Task<Result<Buyer>> UpdateBuyerAsync(string id, UpdateBuyerDto updateBuyerDto)
    {
        var draftBuyer = mapper.Map<Buyer>(updateBuyerDto);
        draftBuyer.Id = id;
        
        var validationResult = await validator.ValidateAsync(draftBuyer);
        if (!validationResult.IsValid)
        {
            return Result<Buyer>.Fail(string.Join(", ", validationResult.Errors.Select(x=>x.ErrorMessage)));
        }
        
        var updatedResult = await dataRepository.UpdateBuyerAsync(draftBuyer);
        if (!updatedResult)
        {
            return Result<Buyer>.Fail("failed to update buyer");
        }
        return Result<Buyer>.Ok(draftBuyer);
    }

    public async Task<Result<bool>> DeleteBuyerAsync(string id)
    {
        var buyerExists = await dataRepository.ExistsBuyerAsync(id);
        if (!buyerExists)
        {
            return Result<bool>.Fail("Buyer not found");
        }
        
        var buyerInUse = await dataRepository.ExistsProductWithBuyerIdAsync(id);
        if (buyerInUse)
        {
            return Result<bool>.Fail("Buyer is in-use. Cannot be deleted");
        }
        
        var deleteResult = await dataRepository.DeleteBuyerAsync(id);
        if (!deleteResult)
        {
            return Result<bool>.Fail("failed to delete buyer");
        }
        return Result<bool>.Ok(true);
    }
}