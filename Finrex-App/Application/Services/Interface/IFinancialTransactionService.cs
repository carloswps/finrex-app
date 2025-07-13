using Finrex_App.Application.DTOs;

namespace Finrex_App.Application.Services.Interface;

public interface IFinancialTransactionService
{
    Task<bool> RegisterMIncomeAsync( MIncomeDto mIncomeDto, int userId );
}