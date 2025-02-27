using System.ComponentModel.DataAnnotations;

namespace Wmi.Api.Models.Dto;

public class CreateProductDto
{
    [Required] public string Sku => null!;

    [Required]
    public string Title => null!;

    public string Description => null!;

    [Required]
    public string BuyerId => null!;

    public bool Active { get; }
}