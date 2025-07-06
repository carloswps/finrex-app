using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FinancialTransactionController : ControllerBase
{
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly ILoginUserServices _loginUserServices;

    public FinancialTransactionController(
        IFinancialTransactionService financialTransactionService, ILoginUserServices loginUserServices )
    {
        _financialTransactionService = financialTransactionService;
        _loginUserServices = loginUserServices;
    }
    
    [HttpPost("income")]
    public async Task<IActionResult> RegisterMIncomeAsync( MIncomeDto mIncomeDto )
    {
         var usuarioId = User.FindFirst( ClaimTypes.NameIdentifier )?.Value;
         Console.WriteLine($"Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}"))}");
        if ( string.IsNullOrEmpty( usuarioId ) )
        {
            return Unauthorized("Id do usuario não encontrado no token");
        }
        
        var result = await _financialTransactionService.RegisterMIncomeAsync( mIncomeDto, Convert.ToInt32( usuarioId ) );
        if ( !result )
        {
            return BadRequest("Não foi possivel realizar o cadastro");
        }
        return Ok();
    }
    
}