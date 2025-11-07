using Finrex_App.Application.DTOs;
using Finrex_App.Application.Validators;
using Finrex_App.Infra.Data;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Finrex_App.Tests;

public class MIncomeDtoValidatorTests
{
    private readonly MIncomeDTOValidator _validator;

    public MIncomeDtoValidatorTests()
    {
        var options = new DbContextOptions<AppDbContext>();
        var dbContextMock = new Mock<AppDbContext>( options );

        _validator = new MIncomeDTOValidator( dbContextMock.Object );
    }

    [Fact]
    public void Date_ShouldHaveError_WhenDateIsInTheFuture()
    {
        var model = new MIncomeDto
        {
            Date = DateOnly.FromDateTime( DateTime.Today.AddYears( 1 ) )
        };

        var result = _validator.TestValidate( model );

        result.ShouldHaveValidationErrorFor( x => x.Date );
    }
}