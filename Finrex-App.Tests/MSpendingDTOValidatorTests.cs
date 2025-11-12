using Finrex_App.Application.DTOs;
using Finrex_App.Application.Validators;
using Finrex_App.Infra.Data;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Finrex_App.Tests;

public class MSpendingDtoValidatorTests
{
    private readonly MSpendingDTOValidator _validator;

    public MSpendingDtoValidatorTests()
    {
        var options = new DbContextOptions<AppDbContext>();
        var dbContextMock = new Mock<AppDbContext>( options );

        _validator = new MSpendingDTOValidator( dbContextMock.Object );
    }

    [Fact]
    public void Date_ShouldHaveError_WhenDateIsInTheFuture()
    {
        var model = new MSpendingDtO
        {
            Date = DateOnly.FromDateTime( DateTime.Today.AddYears( 1 ) )
        };

        var result = _validator.TestValidate( model );

        result.ShouldHaveValidationErrorFor( x => x.Date );
    }

    [Fact]
    public void Date_WithNegativeValues_ShouldHaveError()
    {
        var model = new MSpendingDtO
        {
            Date = DateOnly.FromDateTime( DateTime.Today.AddDays( 1 ) )
        };

        var result = _validator.TestValidate( model );

        result.ShouldHaveValidationErrorFor( x => x.Date );
    }

    [Fact]
    public void Must_Accept_Decimal_Values_Or_Null()
    {
        var model = new MSpendingDtO
        {
            Date = DateOnly.FromDayNumber( DateTime.Today.Day ),
            Entertainment = 200.00m,
            Groceries = null,
            Rent = 50,
            Transportation = null,
            Utilities = null
        };
    }
}