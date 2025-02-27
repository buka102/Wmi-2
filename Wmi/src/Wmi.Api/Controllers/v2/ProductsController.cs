using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Wmi.Api.Models;
using Wmi.Api.Models.Dto;
using Wmi.Api.Services;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

[ApiController]
[Route("api/v2/[controller]")]
// [Authorize]
public class ProductsController(IProductService productService) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType(typeof(Result<Product>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductDto createProductDto)
    {
        var newProductResult = await productService.CreateProductAsync(createProductDto);

        if (newProductResult.Success)
        {
            return Created($"api/v2/Products/{newProductResult.Value!.Sku}", newProductResult.Value);
        }

        return BadRequest(newProductResult);
    }

    [HttpPut("{sku:length(1,50)}")]
    [ProducesResponseType(typeof(Result<Product>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateProductAsync([FromRoute]string sku, [FromBody] UpdateProductDto updateProductDto)
    {
        var updatedProductResult = await productService.UpdateProductAsync(sku, updateProductDto);

        if (updatedProductResult.Success)
        {
            return Ok(updatedProductResult);
        }

        return BadRequest(updatedProductResult);
    }

    [HttpPatch("{sku:length(1,50)}/active")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProductActiveStatus([FromRoute]string sku, [FromBody] bool active)
    {
        var updatedProductResult = await productService.ChangeActiveStatusAsync(sku, active);

        if (updatedProductResult.Success)
        {
            return Ok(updatedProductResult);
        }
        return BadRequest(updatedProductResult);
    }

    [HttpPatch("{sku:length(1,50)}/buyer")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProductActiveStatus([FromRoute]string sku, [FromBody] string newBuyerId)
    {
        var updatedProductResult = await productService.ChangeBuyerAsync(sku, newBuyerId);

        if (updatedProductResult.Success)
        {
            return Ok(updatedProductResult);
        }
        return BadRequest(updatedProductResult);
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(Result<List<Product>>), 200)]
    public async Task<IActionResult> GetAllProducts([FromQuery] bool? expand, [FromQuery] string? titleContains,
        [FromQuery] string? titleStartsWith, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var productListResult = await productService.GetProductsAsync(titleContains, titleStartsWith, page: page ?? 1,
            pageSize: pageSize ?? 10, includeBuyer: expand ?? false);

        if (productListResult.Success)
        {
            return Ok(productListResult);
        }

        return BadRequest(productListResult);
    }
}