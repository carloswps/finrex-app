using Finrex_App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Infra;

public class AppDbContext : DbContext
{
    public AppDbContext( DbContextOptions<AppDbContext> options ) : base( options )
    {
    }

    public DbSet<LoginDb> Contatos { get; set; }
}