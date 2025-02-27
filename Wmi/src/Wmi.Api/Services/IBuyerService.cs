using Wmi.Api.Models;

namespace Wmi.Api.Services;

public interface IBuyerService
{
    Task<Result<List<Buyer>>> GetBuyersAsync(int page = 1, int pageSize = 10);
    Task<Result<Buyer>> CreateBuyerAsync(string name, string email);
    Task<Result<bool>> ExistsBuyerAsync(string id);
    Task<Result<Buyer>> UpdateBuyerAsync(Buyer draftBuyer);
    Task<Result<bool>> DeleteBuyerAsync(string id);
}