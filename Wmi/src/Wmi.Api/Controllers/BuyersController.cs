using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Wmi.Api.Models;
using Wmi.Api.Services;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class BuyersController(IBuyerService buyerService) : ControllerBase
{
    public class CreateBuyerRequest
    {

        [Required] public string Name { get; init; } = default!;
        [Required] public string Email  { get; init; } = default!;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<Buyer>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateBuyerAsync([FromBody] CreateBuyerRequest buyer)
    {
        var newBuyerResult = await buyerService.CreateBuyerAsync(buyer.Name, buyer.Email);

        if (newBuyerResult.Success)
        {
            return Created($"api/buyers/{newBuyerResult.Value!.Id}", newBuyerResult.Value);
        }
        return BadRequest(newBuyerResult);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBuyerAsync(string id)
    {
        /*
    var product = await _productService.GetProductByIdAsync(id);
    return product is not null ? Ok(product) : NotFound();
    */
        return Ok();
    }

    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        /*
    var updated = await _productService.UpdateProductAsync(id, updatedProduct);
    return updated ? NoContent() : NotFound();
    */
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        /*
    var deleted = await _productService.DeleteProductAsync(id);
    return deleted ? NoContent() : NotFound();
    */
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Buyer>>), 200)]
    public async Task<IActionResult> GetAllBuyers([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var buyerListResult = await buyerService.GetBuyersAsync(page:page?? 1, pageSize:pageSize ?? 100);

        if (buyerListResult.Success)
        {
            return Ok(buyerListResult);
        }
        return BadRequest(buyerListResult);
    }
}