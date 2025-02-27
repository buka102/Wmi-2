using System.ComponentModel.DataAnnotations;

namespace Wmi.Api.Models.Dto;

public class CreateBuyerDto
{
    [Required] public string Name { get; init; } = default!;
    [Required] public string Email  { get; init; } = default!;
}