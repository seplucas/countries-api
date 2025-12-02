// CountriesApp.Infrastructure/Data/CountriesAppDbContext.cs
using Microsoft.EntityFrameworkCore;
using CountriesApp.Domain.Entities;

namespace CountriesApp.Infrastructure.Data;

public class CountriesAppDbContext(DbContextOptions<CountriesAppDbContext> options) : DbContext(options) 
{
    public DbSet<Country> Countries { get; set; } = null!;  
    public DbSet<City> Cities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Code).HasMaxLength(3);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(c => c.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}