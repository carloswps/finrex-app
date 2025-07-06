using Finrex_App.Domain.Entities;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class UserEntitieValidator : AbstractValidator<User>
{
    public UserEntitieValidator()
    {
        RuleFor( x => x.Nome )
            .NotEmpty().WithMessage( "Por favor informe o seu nome." )
            .Length( 2, 100 ).WithMessage( "O nome deve ter entre 2 e 100 caracteres." );
        RuleFor( x => x.Email )
            .NotEmpty().WithMessage( "Por favor informe o seu email." )
            .EmailAddress().WithMessage( "Por favor digite um e-mail valido" );
        RuleFor( x => x.Senha )
            .NotEmpty().WithMessage( "Por favor informe a sua senha." )
            .Length( 6, 100 ).WithMessage( "A senha deve ter entre 6 e 100 caracteres." );
    }
}