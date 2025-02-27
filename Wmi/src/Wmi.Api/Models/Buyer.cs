using FluentValidation;

namespace Wmi.Api.Models;

    public class Buyer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class BuyerValidator : AbstractValidator<Buyer>
    {
        public BuyerValidator()
        {
            RuleFor(x => x.Id).NotNull().Length(32);
            RuleFor(x => x.Name).Length(1, 100);
            RuleFor(x => x.Email).EmailAddress().Length(2, 100);
        }
    }


