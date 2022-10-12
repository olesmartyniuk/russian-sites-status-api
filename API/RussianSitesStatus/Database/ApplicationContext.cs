using Microsoft.EntityFrameworkCore;
using RussianSitesStatus.Database.Models;
using System.Reflection;

namespace RussianSitesStatus.Database;

public class ApplicationContext : DbContext
{
    public DbSet<Site> Sites { get; set; }
    public DbSet<Check> Checks { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<ChecksStatistics> ChecksStatistics { get; set; }


    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {

    }

    public ApplicationContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddEnvironmentVariables()
                .Build();

            optionsBuilder.UseNpgsql(config.GetConnectionString());
        }
    }
}
