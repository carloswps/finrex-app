using Finrex_App.Application.DTOs;
using Finrex_App.Infra.Data;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class MSpendingDTOValidator : AbstractValidator<MSpendingDtO>
{
    private readonly AppDbContext _dbContext;

    public MSpendingDTOValidator( AppDbContext dbContext )
    {
        _dbContext = dbContext;
        RuleFor( x => x.Date )
            .NotEmpty().WithMessage( "Por favor digite uma data valida" )
            .LessThanOrEqualTo( DateTime.Today )
            .GreaterThanOrEqualTo( DateTime.Today.AddMonths( -1 ) )
            .WithMessage( "A data não pode ser maior que o dia atual" );
    }
}