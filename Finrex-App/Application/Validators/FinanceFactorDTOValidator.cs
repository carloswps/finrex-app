using Finrex_App.Application.DTOs;
using Finrex_App.Infra.Data;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class FinanceFactorDTOValidator : AbstractValidator<FinanceFactorDto>
{
    public FinanceFactorDTOValidator()
    {
        RuleFor(x => x.Taxes)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .When(x => x.Taxes.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Interest)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .When(x => x.Interest.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.OtherDeductions)
            .GreaterThanOrEqualTo(0)
            .When(x => x.OtherDeductions.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.ReferenceDate)
            .GreaterThan(DateOnly.MinValue)
            .When(x => x.ReferenceDate.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");
    }
}