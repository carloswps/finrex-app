using Finrex_App.Application.DTOs;
using Finrex_App.Infra.Data;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class MSpendingDTOValidator : AbstractValidator<MSpendingDtO>
{
    private readonly AppDbContext _dbContext;

    public MSpendingDTOValidator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddDays(30)))
            .NotEmpty().WithMessage("Por favor digite uma data valida");
    }
}