using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/finance-factors" )]
[Authorize]
public class FinanceFactorController : ControllerBase
{
    private readonly IFinanceFactorsService _financeFactorsService;

    private readonly ILogger<FinanceFactorController> _logger;

    public FinanceFactorController(
        IFinanceFactorsService financeFactorsService, ILogger<FinanceFactorController> logger )
    {
        _financeFactorsService = financeFactorsService;
        _logger = logger;
    }

    private string? GetUserId()
    {
        return User.FindFirst( "userId" )?.Value
               ?? User.FindFirst( ClaimTypes.NameIdentifier )?.Value
               ?? User.FindFirst( "sub" )?.Value
               ?? User.FindFirst( "nameid" )?.Value;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpsertFinanceFactors( [FromBody] FinanceFactorDto input )
    {
        if ( input == null )
        {
            return BadRequest( new { message = "Corpo da requisição inválido ou ausente." } );
        }

        if ( !ModelState.IsValid )
        {
            return BadRequest( new { message = "Dados inválidos.", errors = ModelState } );
        }

        var userIdString = GetUserId();
        if ( string.IsNullOrEmpty( userIdString ) || !int.TryParse( userIdString, out var userId ) )
        {
            return Unauthorized( new { message = "O usuário não possui as credenciais necessárias." } );
        }

        var referenceMonth = input.ReferenceDate ?? new DateOnly( DateTime.Today.Year, DateTime.Today.Month, 1 );

        try
        {
            await _financeFactorsService.UpsertFinanceFactorsAsync( userId, referenceMonth, input );

            return Ok( new
            {
                message = "Fatores financeiros registrados com sucesso.",
                Dados = input
            } );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao atualizar fatores financeiros para o usuário {UserId}.", userId );

            return StatusCode( StatusCodes.Status500InternalServerError,
                new { message = "Ocorreu um erro inesperado ao processar sua solicitação." } );
        }
    }
}