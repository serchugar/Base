using Microsoft.EntityFrameworkCore;
using Serchugar.Base.Backend.Tests.Manual.Services.Countries;
using Serchugar.Base.Backend.Tests.Manual.Services.Teams;
using Serchugar.Base.Backend.Tests.Manual.Services.Users;

namespace Serchugar.Base.Backend.Tests.Manual.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<CountryModel> Countries { get; set; }
    public DbSet<TeamModel> Teams { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}