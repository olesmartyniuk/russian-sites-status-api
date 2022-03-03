using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RussianSitesStatus.Database.Models;

//TODOALEX: Create migration for this entity
public class Proxy : Entity
{
    public string Name { get; set; }
    public string Url { get; set; } //contains port

    public Proxy()
    {
    }
}

public class RegionConfiguration : IEntityTypeConfiguration<Proxy>
{
    public void Configure(EntityTypeBuilder<Proxy> builder)
    {
        builder
            .Property(p => p.Id)
            .IsRequired();
        builder
            .Property(p => p.Name)
            .IsRequired();
        builder
            .Property(p => p.Url)
            .IsRequired();
    }
}