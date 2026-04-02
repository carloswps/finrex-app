using Finrex_App.Application.DTOs;
using Finrex_App.Infra.Data;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class MIncomeDTOValidator : AbstractValidator<MIncomeDto>
{
    public MIncomeDTOValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.MainIncome)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MainIncome.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Freelance)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Freelance.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Other)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Other.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Benefits)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Benefits.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.BusinessProfit)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BusinessProfit.HasValue)
            .WithMessage("{PropertyName} deve ser maior ou igual a {ComparisonValue}");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddDays(30)))
            .GreaterThan(DateOnly.MinValue)
            .NotEmpty().WithMessage("Por favor digite uma data valida");
    }
}