using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RussianSitesStatus.Database.Models;

public class Region : Entity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string ProxyUrl { get; set; } //contains port
    public string ProxyUser { get; set; }
    public string ProxyPassword { get; set; }
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