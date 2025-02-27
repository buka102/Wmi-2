using Wmi.Api.Models;
using Wmi.Api.Models.Dto;

namespace Wmi.Api.Services;

public interface IBuyerService
{
    Task<Result<List<Buyer>>> GetBuyersAsync(int page = 1, int pageSize = 10);
    Task<Result<Buyer>> CreateBuyerAsync(CreateBuyerDto createBuyerDto);
    Task<Result<bool>> ExistsBuyerAsync(string id);
    Task<Result<Buyer>> UpdateBuyerAsync(string id, UpdateBuyerDto draftBuyer);
    Task<Result<bool>> DeleteBuyerAsync(string id);
}