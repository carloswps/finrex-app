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

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/transactions")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly MIncomeDTOValidator _dtoMiValidator;
    private readonly MSpendingDTOValidator _dtoMsValidator;
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        IFinancialTransactionService financialTransactionService,
        ILogger<TransactionController> logger, MIncomeDTOValidator dtoValidatorMi,
        MSpendingDTOValidator dtoMsValidator)
    {
        _financialTransactionService = financialTransactionService;
        _logger = logger;
        _dtoMsValidator = dtoMsValidator;
        _dtoMiValidator = dtoValidatorMi;
    }

    [HttpPost("incomes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterIncome([FromBody] MIncomeDto mIncomeDto)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized("O usuário não possui as credências necessárias");
        await _dtoMiValidator.ValidateAndThrowAsync(mIncomeDto);
        var result = await _financialTransactionService.RegisterMIncomeAsync(mIncomeDto, userId.Value);
        if (!result) return BadRequest("Não foi possivel realizar o cadastro");
        return CreatedAtAction(nameof(RegisterIncome), new { Sucesso = true, Dados = mIncomeDto });
    }


    [HttpPost("spendings")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterSpending([FromBody] MSpendingDtO mSpendingDto)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized("O usuário não possui as credências necessárias");
        var result = await _financialTransactionService.RegisterMSpendingAsync(mSpendingDto, userId.Value);
        if (!result) return BadRequest("Não foi possivel realizar o cadastro");
        return CreatedAtAction(nameof(RegisterSpending), new { Sucesso = true, Dados = mSpendingDto });
    }
}