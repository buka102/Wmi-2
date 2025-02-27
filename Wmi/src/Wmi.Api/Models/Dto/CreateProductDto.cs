using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace Wmi.Api.Models.Dto;
[AutoMap(typeof(Product), ReverseMap = true)]
public class CreateProductDto
{
    public required string Sku { get; init; }
    
    public required string Title { get; init; }

    public string? Description { get; init; }
    
    public required string BuyerId { get; init; }

    public bool Active { get; init; }
}