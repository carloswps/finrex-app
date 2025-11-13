using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Domain.Entities;
using Finrex_App.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Application.Services;

public class FinanceFactorsService : IFinanceFactorsService
{
    private readonly AppDbContext _dbContext;

    public FinanceFactorsService( AppDbContext dbContext )
    {
        _dbContext = dbContext;
    }

    public async Task UpsertFinanceFactorsAsync( int userId, DateOnly month, FinanceFactorDto input )
    {
        var existingFactor = await _dbContext.MFinanceFactorsEnumerable
            .FirstOrDefaultAsync( f => f.UsuarioId == userId && f.Date == month );

        if ( existingFactor == null )
        {
            existingFactor = new MFinanceFactors
            {
                UsuarioId = userId,
                Date = month,
                Taxes = input.Taxes ?? 0,
                OtherDeductions = input.OtherDeductions ?? 0,
                Interest = input.Interest ?? 0
            };
            _dbContext.MFinanceFactorsEnumerable.Add( existingFactor );
        } else
        {
            existingFactor.Taxes = input.Taxes ?? existingFactor.Taxes;
            existingFactor.Interest = input.Interest ?? existingFactor.Interest;
            existingFactor.OtherDeductions = input.OtherDeductions ?? existingFactor.OtherDeductions;
        }

        await _dbContext.SaveChangesAsync();
    }
}