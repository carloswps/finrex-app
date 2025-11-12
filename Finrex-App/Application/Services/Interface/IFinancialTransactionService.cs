using Finrex_App.Application.DTOs;

namespace Finrex_App.Application.Services.Interface;

public interface IFinancialTransactionService
{
    Task<bool> RegisterMIncomeAsync( MIncomeDto mIncomeDto, int userId );

    Task<bool> RegisterMSpendingAsync( MSpendingDtO mSpendingDto, int userId );

    Task<MoneySavedResult> GetCurrentMonthSpendingsAsync( int userId, DateOnly firstMonth, DateOnly lastMonth );

    Task<SummaryResponse> GetSummaryAsync( DateTime? startDate, DateTime? endDate, int userId );

    Task<bool> SavingsGrowth( MIncomeDto mIncomeDto, MSpendingDtO mSpendingDtO, int userId );

    Task<bool> NetProfit();
}