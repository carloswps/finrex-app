using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Finrex_App.Extensions;
using Finrex_App.Infra.Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Infra.Api.Controllers;

/// <inheritdoc />
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/transactions")]
[Authorize]
public class TransactionController(
    IFinancialTransactionService financialTransactionService,
    ILogger<TransactionController> logger,
    MIncomeDTOValidator dtoValidatorMi,
    MSpendingDTOValidator dtoMsValidator)
    : ControllerBase
{
    private readonly MSpendingDTOValidator _dtoMsValidator = dtoMsValidator;
    private readonly ILogger<TransactionController> _logger = logger;

    [HttpPost("incomes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterIncome([FromBody] MIncomeDto mIncomeDto)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        await dtoValidatorMi.ValidateAndThrowAsync(mIncomeDto);
        var result = await financialTransactionService.RegisterMIncomeAsync(mIncomeDto, userId.Value);

        if (!result)
        {
            var response = ApiResponse<string>.CreateFailure("Não foi possivel realizar o cadastro");
            return BadRequest(response);
        }

        var successResponse = ApiResponse<object>.CreateSuccess(new
            { Sucesso = true, Dados = mIncomeDto });
        return Ok(successResponse);
    }


    [HttpPost("spendings")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterSpending([FromBody] MSpendingDtO mSpendingDto)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        await dtoMsValidator.ValidateAndThrowAsync(mSpendingDto);
        var result = await financialTransactionService.RegisterMSpendingAsync(mSpendingDto, userId.Value);

        if (!result)
        {
            var response = ApiResponse<string>.CreateFailure("Não foi possivel realizar o cadastro");
            return BadRequest(response);
        }

        var successResponse = ApiResponse<object>.CreateSuccess(new { Sucesso = true, Dados = mSpendingDto });
        return Ok(successResponse);
    }
}