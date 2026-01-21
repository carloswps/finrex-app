using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/finance-factors")]
[Authorize]
public class FinanceFactorController(IFinanceFactorsService factorsService, ILogger<FinanceFactorController> logger)
    : ControllerBase
{
    private readonly IFinanceFactorsService _financeFactorsService = factorsService;
    private readonly ILogger<FinanceFactorController> _logger = logger;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpsertFinanceFactors([FromBody] FinanceFactorDto input)
    {
        if (!ModelState.IsValid)
        {
            var response = ApiResponse<object>.CreateFailure("Dados inválidos.");
            return BadRequest(response);
        }

        var userIdString = User.GetUserId();
        if (userIdString == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var referenceMonth = input.ReferenceDate ?? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        await _financeFactorsService.UpsertFinanceFactorsAsync(userIdString.Value, referenceMonth, input);

        var successResponse = ApiResponse<string>.CreateSuccess("Dados atualizados com sucesso");
        return Ok(successResponse);
    }
}