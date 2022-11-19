using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Cqrs.Products.Command;

public class AddProductCommandValidation : AbstractValidator<AddProductCommand>
{
    public AddProductCommandValidation()
    {
        RuleFor(a => a.Name).NotEmpty().WithMessage("Name is required.");
    }
}

