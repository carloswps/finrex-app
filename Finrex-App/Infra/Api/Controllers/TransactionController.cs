using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Finrex_App.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/transactions" )]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly MIncomeDTOValidator _dtoMiValidator;
    private readonly MSpendingDTOValidator _dtoMsValidator;
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly ILogger<TransactionController> _logger;
    private readonly AppDbContext _dbContext;

    public TransactionController(
        IFinancialTransactionService financialTransactionService,
        ILogger<TransactionController> logger, MIncomeDTOValidator dtoValidatorMi,
        MSpendingDTOValidator dtoMsValidator, AppDbContext dbContext )
    {
        _financialTransactionService = financialTransactionService;
        _logger = logger;
        _dtoMsValidator = dtoMsValidator;
        _dbContext = dbContext;
        _dtoMiValidator = dtoValidatorMi;
    }

    private int? GetCurrentUserId()
    {
        var userIdString = User.FindFirst( "userId" )?.Value
                           ?? User.FindFirst( ClaimTypes.NameIdentifier )?.Value;
        if ( int.TryParse( userIdString, out var userId ) )
        {
            return userId;
        }

        return null;
    }

    [HttpPost( "incomes" )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterIncome( [FromBody] MIncomeDto mIncomeDto )
    {
        if ( mIncomeDto == null )
        {
            return BadRequest( new
                { Sucesso = false, Mensagem = "O corpo da requisição está vazio ou em um formato inválido." } );
        }

        try
        {
            var userId = GetCurrentUserId();
            if ( userId == null )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var validationResult = await _dtoMiValidator.ValidateAsync( mIncomeDto );
            if ( !validationResult.IsValid )
            {
                return BadRequest( new
                {
                    Sucesso = false,
                    Erros = validationResult.Errors.Select( e => e.ErrorMessage )
                } );
            }

            var result = await _financialTransactionService.RegisterMIncomeAsync( mIncomeDto, userId.Value );
            if ( !result )
            {
                return BadRequest( "Não foi possivel realizar o cadastro" );
            }

            return CreatedAtAction( nameof( RegisterIncome ), new { Sucesso = true, Dados = mIncomeDto } );
        } catch ( Exception e )
        {
            _logger.LogError( e.Message, "Erro ao realizar cadastro" );
            throw;
        }
    }


    [HttpPost( "spendings" )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterSpending( [FromBody] MSpendingDtO mSpendingDto )
    {
        if ( mSpendingDto == null )
        {
            return BadRequest( new
                { Sucesso = false, Mensagem = "O corpo da requisição está vazio ou em um formato inválido." } );
        }

        try
        {
            var userId = GetCurrentUserId();
            if ( userId == null )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var validationResult = await _dtoMsValidator.ValidateAsync( mSpendingDto );
            if ( !validationResult.IsValid )
            {
                return BadRequest( new
                {
                    Sucesso = false,
                    Erros = validationResult.Errors.Select( e => e.ErrorMessage )
                } );
            }

            var result = await _financialTransactionService.RegisterMSpendingAsync( mSpendingDto, userId.Value );
            if ( !result )
            {
                return BadRequest( "Não foi possivel realizar o cadastro" );
            }

            return CreatedAtAction( nameof( RegisterSpending ), new { Sucesso = true, Dados = mSpendingDto } );
        } catch ( Exception e )
        {
            _logger.LogError( e.Message, "Erro ao realizar cadastro" );
            throw;
        }
    }
}