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
        if ( firstMonthSpending != 0 )
        {
            differenceInPorcentage = ( differenceInReais / firstMonthSpending ) * 100;
        }


        var formattedValue = Math.Round( differenceInPorcentage, 2 );

        return new MoneySavedResult
        {
            FirstMonth = firstMonth,
            LastMonth = lastMonth,
            FirstMonthSaveSpending = firstMonthSpending,
            LastMonthSaveSpending = lastMonthSpending,
            DifferenceBetweenMonths = differenceInReais,
            DifferenceInPorcentage = formattedValue
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

    private async Task<decimal> CalculateNetBalanceForMonth( int userId, DateOnly month )
    {
        var monthStart = month;
        var monthEnd = month.AddMonths( 1 ).AddDays( -1 );

        var totalIncome = await _context.MIncome
            .Where( i => i.UsuarioId == userId && i.Date >= monthStart && i.Date <= monthEnd )
            .SumAsync( i => i.MainIncome + i.Freelance + i.Benefits + i.BusinessProfit + i.Other );

        var totalSpending = await _context.MSpending
            .Where( s => s.UsuarioId == userId && s.Date >= monthStart && s.Date <= monthEnd )
            .SumAsync( s => s.Groceries + s.Rent + s.Transportation + s.Utilities + s.Entertainment );

        return totalIncome - totalSpending;
    }


    public async Task<SavingsGrowthResult> GetSavingsGrowthAsync(
        int userId, DateOnly firstMonth, DateOnly lastMonth )
    {
        var firstMonthNetBalance = await CalculateNetBalanceForMonth( userId, firstMonth );
        var lastMonthNetBalance = await CalculateNetBalanceForMonth( userId, lastMonth );

        var growthInReais = lastMonthNetBalance - firstMonthNetBalance;

        decimal growthInPorcentage = 0;
        if ( firstMonthNetBalance != 0 )
        {
            growthInPorcentage = ( growthInReais / firstMonthNetBalance ) * 100;
        }

        var formattedValue = Math.Round( growthInPorcentage, 2 );

        return new SavingsGrowthResult
        {
            FirstMonth = firstMonth,
            LastMonth = lastMonth,
            FirstMonthNetBalance = firstMonthNetBalance,
            LastMonthNetBalance = lastMonthNetBalance,
            GrowInReais = growthInReais,
            GrowInPorcentage = formattedValue
        };
    }

    public Task<bool> NetProfit()
    {
        throw new NotImplementedException();
    }
}