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


    public async Task<MoneySavedResult> GetCurrentMonthSpendingsAsync(
        int userId, DateOnly firstMonth, DateOnly lastMonth )
    {
        var firstMonthStart = firstMonth;
        var firstMonthEnd = firstMonth.AddMonths( 1 ).AddDays( -1 );

        var lastMonthStart = lastMonth;
        var lastMonthEnd = lastMonth.AddMonths( 1 ).AddDays( -1 );

        var firstMonthSpending = await _context.MSpending
            .Where( s => s.UsuarioId == userId && s.Date >= firstMonthStart && s.Date <= firstMonthEnd )
            .SumAsync( s => s.Transportation + s.Groceries + s.Entertainment + s.Rent + s.Utilities );

        var lastMonthSpending = await _context.MSpending
            .Where( s => s.UsuarioId == userId && s.Date >= lastMonthStart && s.Date <= lastMonthEnd )
            .SumAsync( s => s.Transportation + s.Groceries + s.Entertainment + s.Rent + s.Utilities );

        var differenceInReais = firstMonthSpending - lastMonthSpending;

        decimal differenceInPorcentage = 0;
        if ( differenceInPorcentage != 0 )
        {
            differenceInPorcentage = ( differenceInReais / firstMonthSpending ) * 100;
        }

        return new MoneySavedResult
        {
            FirstMonth = firstMonth,
            LastMonth = lastMonth,
            FirstMonthSaveSpending = firstMonthSpending,
            LastMonthSaveSpending = lastMonthSpending,
            DifferenceBetweenMonths = differenceInReais,
            DifferenceInPorcentage = differenceInPorcentage
        };
    }

    public async Task<SummaryResponse> GetSummaryAsync( DateTime? startDate, DateTime? endDate, int userId )
    {
        var incomeQuery = _context.MIncome.AsQueryable();
        var spendingQuery = _context.MSpending.AsQueryable();

        incomeQuery = incomeQuery.Where( i => i.UsuarioId == userId );
        spendingQuery = spendingQuery.Where( s => s.UsuarioId == userId );

        if ( startDate.HasValue )
        {
            var startDateOnly = DateOnly.FromDateTime( startDate.Value );
            incomeQuery = incomeQuery.Where( i => i.Date >= startDateOnly );
            spendingQuery = spendingQuery.Where( s => s.Date >= startDateOnly );
        }

        if ( endDate.HasValue )
        {
            var endOfMonth = new DateOnly( endDate.Value.Year, endDate.Value.Month, 1 ).AddMonths( 1 );
            incomeQuery = incomeQuery.Where( i => i.Date < endOfMonth );
            spendingQuery = spendingQuery.Where( s => s.Date < endOfMonth );
        }

        var period = startDate.HasValue && endDate.HasValue
            ? $"{startDate.Value:yyyy-MM} à {endDate.Value:yyyy-MM}"
            : "Todo periodo";

        var income = await incomeQuery
            .GroupBy( i => 1 )
            .Select( g => new IncomeSummaryDto
            {
                Period = period,
                MainIncome = g.Sum( i => i.MainIncome ),
                Freelance = g.Sum( i => i.Freelance ),
                Benefits = g.Sum( i => i.Benefits ),
                BusinessProfit = g.Sum( i => i.BusinessProfit ),
                Other = g.Sum( i => i.Other )
            } ).ToListAsync();

        var spending = await spendingQuery
            .GroupBy( s => 1 )
            .Select( g => new SpendingSummaryDto
            {
                Period = period,
                Transportation = g.Sum( s => s.Transportation ),
                Groceries = g.Sum( s => s.Groceries ),
                Entertainment = g.Sum( s => s.Entertainment ),
                Rent = g.Sum( s => s.Rent ),
                Utilities = g.Sum( s => s.Utilities )
            } ).ToListAsync();

        var response = new SummaryResponse { Income = income, Spending = spending };

        return response;
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