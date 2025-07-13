using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Domain.Entities;
using Finrex_App.Infra.Data;

namespace Finrex_App.Application.Services;

public class FinancialTransactionService : IFinancialTransactionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FinancialTransactionService> _logger;

    public FinancialTransactionService( AppDbContext context, ILogger<FinancialTransactionService> logger )
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<bool> RegisterMIncomeAsync( MIncomeDto mIncomeDto, int userId )
    {
        try
        {
            var mIncome = new MonthlyIncome
            {
                Mes = mIncomeDto.Mes,
                UsuarioId = userId,
               MainIncome = mIncomeDto.MainIncome ?? 0,
               Freelance = mIncomeDto.Freelance ?? 0,
               Benefits = mIncomeDto.Benefits ?? 0,
               BussinesProfit = mIncomeDto.BussinesProfit ?? 0,
               Other = mIncomeDto.Other ?? 0
            };
            await _context.MIncome.AddAsync( mIncome );
            await _context.SaveChangesAsync();
            return true;

        } catch ( Exception ex )
        {
            _logger.LogError( ex, "Erro ao cadastrar Valores" );
            throw;
        }
    }

    
}