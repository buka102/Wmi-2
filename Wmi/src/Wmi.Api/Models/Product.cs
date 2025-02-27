using FluentValidation;
using Wmi.Api.Data;

namespace Wmi.Api.Models
{
    public class Product
    {
        public required string SKU { get; set; }
        
        public required string Title { get; init; }

        public string? Description { get; set; }
        
        public required string BuyerId { get; set; }

        public bool Active { get; set; }
        
        // Virtual navigation property (not mapped to DB, populated as needed)
        public virtual Buyer? Buyer { get; set; }
    }
    
    public class ProductValidator : AbstractValidator<Product> 
    {
        public ProductValidator(IDataRepository dataRepository) 
        {
            RuleFor(x => x.SKU).NotEmpty().WithMessage("SKU cannot be empty.")
                .Matches(@"^[a-zA-Z0-9_-]{1,50}$").WithMessage("SKU must contain only letters, numbers, hyphens, and underscores, and be 1 to 50 characters long.");
            RuleFor(x => x.Title).Length(1, 200);
            RuleFor(x => x.Description).Length(0,10000);
            RuleFor(x => x.BuyerId)
                .NotNull().Length(32).WithMessage("Buyer id cannot be empty.")
                .MustAsync(async (buyerId, _) => await dataRepository.ExistsBuyerAsync(buyerId))
                .WithMessage("buyerId is invalid");
        }
    }
}