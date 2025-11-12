using System;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Validators;
using Finrex_App.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Tests;

public class RegisterDtoValidatorTests
{
    private readonly RegisterDTOValidator _validator;

    public RegisterDtoValidatorTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase( Guid.NewGuid().ToString() )
            .Options;
        var context = new AppDbContext( options );
        _validator = new RegisterDTOValidator( context );
    }

    [Fact]
    public async Task Must_Give_Error_When_Email_Is_Invalid()
    {
        var model = new RegisterDTO
        {
            email = "invalid-email",
            password = "senhaForte123"
        };

        var result = await _validator.ValidateAsync( model );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, err => err.PropertyName == "email" );
    }

    [Fact]
    public async Task Must_Give_Error_When_Email_Is_Empty()
    {
        var model = new RegisterDTO
        {
            email = "",
            password = "senhaForte123"
        };

        var result = await _validator.ValidateAsync( model );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, err => err.PropertyName == "email" );
    }

    [Fact]
    public async Task Must_Give_Error_When_Password_Is_Empty()
    {
        var model = new RegisterDTO
        {
            email = "usuario@exemplo.com",
            password = ""
        };

        var result = await _validator.ValidateAsync( model );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, err => err.PropertyName == "password" );
    }

    [Fact]
    public async Task Must_Give_Error_When_Password_Is_Too_Short()
    {
        var model = new RegisterDTO
        {
            email = "usuario@exemplo.com",
            password = "123"
        };

        var result = await _validator.ValidateAsync( model );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, err => err.PropertyName == "password" );
    }

    [Fact]
    public async Task Must_Pass_Validation_With_Valid_Input()
    {
        var model = new RegisterDTO
        {
            email = "usuario@exemplo.com",
            password = "SenhaSegura123!"
        };

        var result = await _validator.ValidateAsync( model );

        Assert.True( result.IsValid );
        Assert.Empty( result.Errors );
    }
}