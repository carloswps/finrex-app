using Finrex_App.Application.DTOs;

namespace Finrex_App.Application.Services.Interface;

public interface IFinancialTransactionService
{
    Task<bool> RegisterMIncomeAsync( MIncomeDto mIncomeDto, int userId );

    Task<bool> RegisterMSpendingAsync( MSpendingDtO mSpendingDto, int userId );

    Task<MoneySavedResult> GetCurrentMonthSpendingsAsync( int userId, DateOnly firstMonth, DateOnly lastMonth );

    Task<SummaryResponse> GetSummaryAsync( DateTime? startDate, DateTime? endDate, int userId );

    Task<SavingsGrowthResult> GetSavingsGrowthAsync( int userId, DateOnly firstMonth, DateOnly lastMonth );

    Task<NetProfitResult> GetNetProfitGrowthAsync( int userId, DateOnly firstMonth, DateOnly lastMonth );
}