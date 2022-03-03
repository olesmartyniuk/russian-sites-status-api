using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RussianSitesStatus.Database.Models;

public class Region : Entity
{
    public string Name { get; set; }
    public string ProxyUrl { get; set; } //contains port
}

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder
            .Property(p => p.Id)
            .IsRequired();
        builder
            .Property(p => p.Name)
            .IsRequired();
        builder
            .Property(p => p.ProxyUrl)
            .IsRequired();
    }
}