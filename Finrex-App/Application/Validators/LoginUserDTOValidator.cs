using Finrex_App.Application.DTOs;
using FluentValidation;

namespace Finrex_App.Application.Validators;

public class LoginUserDTOValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDTOValidator()
    {
        RuleFor(x => x.email)
            .NotEmpty()
            .WithMessage("{PropertyName} por favor informe o seu email.")
            .EmailAddress()
            .WithMessage("{PropertyName} por favor use ume email valido");

        RuleFor(x => x.password)
            .NotEmpty().WithMessage("Por favor informe a sua senha.")
            .Length(6, 100).WithMessage("A senha deve ter entre 6 e 100 caracteres.");
    }
}