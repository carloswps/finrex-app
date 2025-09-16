using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Domain.Entities;
using Finrex_App.Infra.Data;
using MapsterMapper;

namespace Finrex_App.Application.Services;

public class FinancialTransactionService : IFinancialTransactionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FinancialTransactionService> _logger;
    private readonly IMapper _mapper;

    public FinancialTransactionService(
        AppDbContext context, ILogger<FinancialTransactionService> logger, IMapper mapper )
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<bool> RegisterMIncomeAsync( MIncomeDto mIncomeDto, int userId )
    {
        try
        {
            var mIncome = _mapper.Map<MonthlyIncome>( mIncomeDto );
            mIncome.UsuarioId = userId;

            await _context.MIncome.AddAsync( mIncome );
            await _context.SaveChangesAsync();
            return true;
        } catch ( Exception ex )
        {
            _logger.LogError( ex, "Erro ao cadastrar Valores" );
            throw;
        }
    }

    public async Task<bool> RegisterMSpendingAsync( MSpendingDtO mSpendingDto, int userId )
    {
        try
        {
            var mSpending = _mapper.Map<MonthlySpending>( mSpendingDto );
            mSpending.UsuarioId = userId;

            await _context.MSpending.AddAsync( mSpending );
            await _context.SaveChangesAsync();
            return true;
        } catch ( Exception ex )
        {
            _logger.LogError( ex, "Erro ao cadastrar Valores" );
            throw;
        }
    }
}