using Finrex_App.Core.DTOs;
using Finrex_App.Infra.Data;
using Finrex_App.Services.Interface;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Application.Validators;

public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
{
    private readonly AppDbContext _dbContext;
    
    public RegisterDTOValidator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        RuleFor( x => x.Nome )
            .NotEmpty().WithMessage( "Por favor informe o seu nome." )
            .Length( 2, 100 ).WithMessage( "O nome deve ter entre 2 e 100 caracteres." );
        RuleFor( x => x.Email )
            .NotEmpty().WithMessage( "Por favor informe o seu email." )
            .EmailAddress().WithMessage( "Por favor informe um e-mail válido")
            .MustAsync( BeUniqueEmail ).WithMessage( "Este e-mail já esta cadastrado." );
        RuleFor( x => x.Senha )
            .NotEmpty().WithMessage( "Por favor informe a sua senha." )
            .Length( 6, 100 ).WithMessage( "A senha deve ter entre 6 e 100 caracteres." );
        RuleFor( x => x.ConfirmarSenha )
            .NotEmpty().WithMessage( "Por favor confirme a sua senha." );
    }

    private async Task<bool> BeUniqueEmail( string email, CancellationToken cancellationToken )
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken: cancellationToken );
        return !userExists;
    }
}