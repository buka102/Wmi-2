using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Wmi.Api.Models
{
    public class Product
    {
        [Required]
        public string SKU { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string BuyerId { get; set; }

        public bool Active { get; set; }
        
        // Virtual navigation property (not mapped to DB, populated as needed)
        public virtual Buyer? Buyer { get; set; }
    }
    
    public class ProductValidator : AbstractValidator<Product> 
    {
        public ProductValidator() 
        {
            RuleFor(x => x.SKU).NotNull().Length(1,50);
            RuleFor(x => x.Title).Length(1, 200);
            RuleFor(x => x.Description).Length(0,10000);
            RuleFor(x => x.BuyerId).NotNull().Length(32);
        }
    }
}