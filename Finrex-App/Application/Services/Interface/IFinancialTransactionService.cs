using Finrex_App.Application.DTOs;

namespace Finrex_App.Application.Services.Interface;

public interface IFinancialTransactionService
{
    Task<bool> RegisterMIncomeAsync(MIncomeDto mIncomeDto, int userId);

    Task<bool> RegisterMSpendingAsync(MSpendingDtO mSpendingDto, int userId);

    Task<SpendingVariationResult> GetCurrentMonthSpendingsAsync(int userId, DateOnly firstMonth, DateOnly lastMonth);

    Task<SummaryResponse> GetSummaryAsync(DateTime? startDate, DateTime? endDate, int userId);

    Task<SavingsGrowthResult> GetSavingsGrowthAsync(int userId, DateOnly firstMonth, DateOnly lastMonth);

    Task<NetProfitResult> GetNetProfitGrowthAsync(int userId, DateOnly firstMonth, DateOnly lastMonth);

    Task<SpendingComparison> GetSpendingComparisonAsync(int userId, DateOnly firstMonth, DateOnly lastMonth);

    Task<List<TopEarningMonth>> GetTopEarningMonthAsync(int userId);

    Task<List<TopSavingsMonth>> GetTopSavingsMonthAsync(int userId);

    Task<SpendingSummaryDto?> GetCurrentMonthSpendingSummaryAsync(int userId);

    Task<List<MIncomeResponseDto>> GetIncomeAsync(int userId);
    Task<MIncomeResponseDto> GetIncomeByIdAsync(int id, int userId);
    Task<bool> UpdateIncomeAsync(int id, MIncomeDto dto, int userId);
    Task<bool> DeleteIncomeAsync(int id, int userId);

    Task<List<MSpendingResponseDto>> GetSpendingAsync(int userId);
    Task<MSpendingResponseDto> GetSpendingByIdAsync(int id, int userId);
    Task<bool> UpdateSpendingAsync(int id, MSpendingDtO dtO, int userId);
    Task<bool> DeleteSpendingAsync(int id, int userId);
}