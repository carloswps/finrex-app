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

    [HttpGet("incomes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIncomes()
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var result = await financialTransactionService.GetIncomeAsync(userId.Value);
        var successResponse = ApiResponse<List<MIncomeResponseDto>>.CreateSuccess(result);
        return Ok(successResponse);
    }

    [HttpGet("incomes/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIncomesById(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var result = await financialTransactionService.GetIncomeByIdAsync(id, userId.Value);
        if (result == null) return NotFound(ApiResponse<string>.CreateFailure("Nenhum resultado encontrado"));

        var successResponse = ApiResponse<MIncomeResponseDto>.CreateSuccess(result);
        return Ok(successResponse);
    }

    [HttpPut("incomes/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateIncome(int id, [FromBody] MIncomeDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        await dtoValidatorMi.ValidateAndThrowAsync(dto);
        var result = await financialTransactionService.UpdateIncomeAsync(id, dto, userId.Value);
        if (!result)
            return NotFound(ApiResponse<string>.CreateFailure("Registro não encontrado"));

        var successResponse = ApiResponse<object>.CreateSuccess(new { Sucesso = true, Dados = dto });
        return Ok(successResponse);
    }

    [HttpDelete("incomes/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteIncome(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var result = await financialTransactionService.DeleteIncomeAsync(id, userId.Value);
        if (!result)
        {
            var response = ApiResponse<string>.CreateFailure("Registro não encontrado");
            return NotFound(response);
        }

        var successResponse = ApiResponse<string>.CreateSuccess("Removido com sucesso");
        return Ok(successResponse);
    }

    [HttpGet("spendings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSpendings()
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var result = await financialTransactionService.GetSpendingAsync(userId.Value);
        var successResponse = ApiResponse<List<MSpendingResponseDto>>.CreateSuccess(result);
        return Ok(successResponse);
    }

    [HttpGet("spendings/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSpendingById(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var result = await financialTransactionService.GetSpendingByIdAsync(id, userId.Value);
        if (result == null) return NotFound(ApiResponse<string>.CreateFailure("Nenhum resultado encontrado"));

        var successResponse = ApiResponse<MSpendingResponseDto>.CreateSuccess(result);
        return Ok(successResponse);
    }

    [HttpPut("spendings/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateSpending(int id, [FromBody] MSpendingDtO dto)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        await _dtoMsValidator.ValidateAndThrowAsync(dto);
        var result = await financialTransactionService.UpdateSpendingAsync(id, dto, userId.Value);
        if (!result)
            return NotFound(ApiResponse<string>.CreateFailure("Registro não encontrado"));

        var successResponse = ApiResponse<object>.CreateSuccess(new { Sucesso = true, Dados = dto });
        return Ok(successResponse);
    }

    [HttpDelete("spendings/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteSpending(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var result = await financialTransactionService.DeleteSpendingAsync(id, userId.Value);
        if (!result)
        {
            var response = ApiResponse<string>.CreateFailure("Registro não encontrado");
            return NotFound(response);
        }

        var successResponse = ApiResponse<string>.CreateSuccess("Removido com sucesso");
        return Ok(successResponse);
    }
}