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
        var dbContextMock = new Mock<AppDbContext>(options);

        _validator = new MIncomeDTOValidator(dbContextMock.Object);
    }

    [Fact]
    public void Date_ShouldHaveError_WhenDateIsInTheFuture()
    {
        var model = new MIncomeDto
        {
            Date = DateOnly.FromDateTime(DateTime.Today.AddYears(1))
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Date_WithNegativeValues_ShouldHaveError()
    {
        var model = new MIncomeDto
        {
            Date = DateOnly.MinValue
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Must_Accept_Decimal_Values_Or_Null()
    {
        var model = new MIncomeDto
        {
            Date = DateOnly.FromDayNumber(DateTime.Today.Day),
            Benefits = 2000.00m,
            BusinessProfit = 555.00m,
            Freelance = 454.00m,
            Other = null
        };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}