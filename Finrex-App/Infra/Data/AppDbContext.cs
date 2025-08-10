using Finrex_App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Infra.Data;

/// <summary>
/// Represents the database context for the application, providing access to the database
/// with entities such as Users, Monthly Income, and Monthly Spending.
/// </summary>
/// <remarks>
/// Configures database mappings and relationships for the application's entities.
/// Uses Entity Framework Core for ORM functionality and ensures proper
/// constraints, relationships, and property configurations for the entities specified.
/// </remarks>
public class AppDbContext : DbContext
{
    public AppDbContext( DbContextOptions<AppDbContext> options ) : base( options )
    {
        Users = Set<User>();
        MIncome = Set<MonthlyIncome>();
        MSpending = Set<MonthlySpending>();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<MonthlyIncome> MIncome { get; set; }
    public DbSet<MonthlySpending> MSpending { get; set; }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        base.OnModelCreating( modelBuilder );

        modelBuilder.Entity<User>( entity =>
        {
            entity.HasKey( e => e.Id );
            entity.Property( e => e.Email ).IsRequired().HasMaxLength( 100 );
            entity.HasIndex( e => e.Email ).IsUnique();
            entity.Property( e => e.Senha ).IsRequired();
        } );

        modelBuilder.Entity<MonthlyIncome>( entity =>
        {
            entity.HasOne( mi => mi.User )
                .WithMany( u => u.MonthlyIncomes )
                .HasForeignKey( mi => mi.UsuarioId );
            entity.Property( mi => mi.Date )
                .HasConversion(
                    mi => mi.ToDateTime( TimeOnly.MinValue ),
                    mi => DateOnly.FromDateTime( mi )
                )
                .HasColumnType( "date" );
        } );

        modelBuilder.Entity<MonthlySpending>( entity =>
        {
            entity.HasOne( mi => mi.User )
                .WithMany( u => u.MonthlySpendings )
                .HasForeignKey( mi => mi.UsuarioId );
            entity.Property( ms => ms.Date )
                .HasConversion(
                    ms => ms.ToDateTime( TimeOnly.MinValue ),
                    ms => DateOnly.FromDateTime( ms )
                )
                .HasColumnType( "date" );
        } );
    }
}