using Finrex_App.Core.DTOs;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class LoginUserDTOValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDTOValidator()
    {
        RuleFor( x => x.Email )
            .NotEmpty().WithMessage( "Por favor informe o seu email." )
            .EmailAddress().WithMessage( "Por favor informe um e-mail válido" );
        RuleFor( x => x.Senha )
            .NotEmpty().WithMessage( "Por favor informe a sua senha." )
            .Length( 6, 100 ).WithMessage( "A senha deve ter entre 6 e 100 caracteres." );
    }
}