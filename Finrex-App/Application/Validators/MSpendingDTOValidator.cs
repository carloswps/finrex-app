using Finrex_App.Application.DTOs;
using Finrex_App.Infra.Data;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class MSpendingDTOValidator : AbstractValidator<MSpendingDtO>
{
    public MSpendingDTOValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddDays(30)))
            .GreaterThan(DateOnly.MinValue)
            .NotEmpty().WithMessage("Por favor digite uma data valida");

        RuleFor(x => x.Entertainment)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Entertainment.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Groceries)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Groceries.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Rent)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Rent.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Transportation)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Transportation.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Utilities)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Utilities.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");
    }
}