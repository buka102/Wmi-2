using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace Wmi.Api.Models.Dto;
[AutoMap(typeof(Buyer), ReverseMap = true)]
public class CreateBuyerDto
{
    public required string Name { get; init; }
    public required string Email  { get; init; }
}