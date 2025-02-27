using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Wmi.Api.Models;
using Wmi.Api.Services;

[ApiController]
[Route("api/v2/[controller]")]
// [Authorize]
public class ProductsController(IProductService productService) : ControllerBase
{

    public class NewProductPayload
    {
        [Required] public string Sku => null!;

        [Required]
        public string Title => null!;

        public string Description => null!;

        [Required]
        public string BuyerId => null!;

        public bool Active { get; }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<Product>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateProductAsync([FromBody] NewProductPayload product)
    {
        var newProductResult = await productService.CreateProductAsync(product.Sku, product.Title, product.Description,
            product.BuyerId, product.Active);

        if (newProductResult.Success)
        {
            return Created($"api/v2/Products/{newProductResult.Value!.SKU}", newProductResult.Value);
        }

        return BadRequest(newProductResult);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProduct(int id)
    {
        /*
        var product = await _productService.GetProductByIdAsync(id);
        return product is not null ? Ok(product) : NotFound();
        */
        return Ok();
    }

    [HttpPatch("{sku}/active")]
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

    [HttpPatch("{sku}/buyer")]
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
    public async Task<IActionResult> GetAllProducts([FromQuery] bool? expand, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var productListResult = await productService.GetProductsAsync(page:page??1, pageSize:pageSize??10, includeBuyer:expand ?? false);

        if (productListResult.Success)
        {
            return Ok(productListResult);
        }
        return BadRequest(productListResult);
    }
}