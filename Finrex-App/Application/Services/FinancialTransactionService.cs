using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Domain.Entities;
using Finrex_App.Infra.Data;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IEnumerable<MoneySavedResult>> GetCurrentMonthSpendingsAsync( int userId )
    {
        var today = DateTime.Today;

        var startOfMonth = new DateOnly( today.Year, today.Month, 1 );
        var startOfMonthDateTime = startOfMonth.ToDateTime( TimeOnly.MinValue );
        var startOfNextMotnh = startOfMonthDateTime.AddMonths( 1 );

        var endOfMonthDateTime = startOfNextMotnh.AddDays( -1 );
        var enOfMonth = DateOnly.FromDateTime( endOfMonthDateTime );

        var spendings = await _context.MSpending
            .Where( s => s.UsuarioId == userId && s.Date >= startOfMonth && s.Date <= enOfMonth )
            .GroupBy( s => true )
            .Select( s => new MoneySavedResult
            {
                Month = startOfMonth,
                Transportation = s.Sum( s => s.Transportation ),
                Groceries = s.Sum( s => s.Groceries ),
                Entertainment = s.Sum( s => s.Entertainment ),
                Rent = s.Sum( s => s.Rent ),
                Utilities = s.Sum( s => s.Utilities )
            } ).ToListAsync();

        return spendings.Adapt<IEnumerable<MoneySavedResult>>();
    }


    public Task<bool> SavingsGrowth( MIncomeDto mIncomeDto, MSpendingDtO mSpendingDtO, int userId )
    {
        throw new NotImplementedException();
    }

    public Task<bool> NetProfit()
    {
        throw new NotImplementedException();
    }
}