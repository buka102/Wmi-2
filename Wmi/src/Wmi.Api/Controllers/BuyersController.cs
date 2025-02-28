using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Wmi.Api.Models;
using Wmi.Api.Models.Dto;
using Wmi.Api.Services;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class BuyersController(IBuyerService buyerService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Result<Buyer>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateBuyerAsync([FromBody] CreateBuyerDto buyerDto)
    {
        var newBuyerResult = await buyerService.CreateBuyerAsync(buyerDto);

        if (newBuyerResult.Success)
        {
            return Created($"api/v2/Buyers/{newBuyerResult.Value!.Id}", newBuyerResult.Value);
        }

        return NotFound(newBuyerResult);
    }

    [HttpPut("{id:length(32)}")]
    [ProducesResponseType(typeof(Result<Buyer>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateBuyerAsync([FromRoute] string id, [FromBody] UpdateBuyerDto updateBuyerDto)
    {
        var updatedBuyerResult = await buyerService.UpdateBuyerAsync(id, updateBuyerDto);

        if (updatedBuyerResult.Success)
        {
            return Ok(updatedBuyerResult);
        }

        return NotFound(updatedBuyerResult);
    }

    [HttpDelete("{id:length(32)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBuyerAsync([FromRoute] string id)
    {
        var deleteResult = await buyerService.DeleteBuyerAsync(id);

        if (deleteResult.Success)
        {
            return NoContent();
        }

        return NotFound(deleteResult);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Buyer>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBuyers([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var buyerListResult = await buyerService.GetBuyersAsync(page: page ?? 1, pageSize: pageSize ?? 100);

        if (buyerListResult.Success)
        {
            return Ok(buyerListResult);
        }

        return NotFound(buyerListResult);
    }
}
