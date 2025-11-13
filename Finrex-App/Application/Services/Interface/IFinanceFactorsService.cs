using Finrex_App.Application.DTOs;

namespace Finrex_App.Application.Services.Interface;

public interface IFinanceFactorsService
{
    public Task UpsertFinanceFactorsAsync( int userId, DateOnly month, FinanceFactorDto input );
}