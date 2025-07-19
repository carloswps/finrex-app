using Finrex_App.Application.DTOs;
using Finrex_App.Infra.Data;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class MIncomeDTOValidator : AbstractValidator<MIncomeDto>
{
    private readonly AppDbContext _dbContext;

    public MIncomeDTOValidator( AppDbContext dbContext )
    {
        _dbContext = dbContext;
        RuleFor( x => x.Date )
            .LessThanOrEqualTo( DateOnly.FromDateTime( DateTime.Today ) )
            .NotEmpty().WithMessage( "Por favor digite uma data valida" );
    }
}