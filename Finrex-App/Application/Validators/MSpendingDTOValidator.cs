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
       RuleFor( x => x.Mes )
           .NotEmpty().WithMessage( "Por favor digite um mês" )
           .InclusiveBetween( 1, 12 ).WithMessage( "Por favor digite um mes valido" );
    }
}