using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RussianSitesStatus.Database.Models;

public class Site : Entity
{
    public string Name { get; set; }
    public string Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Check> Checks { get; set; }

    public Site()
    {
        Checks = new List<Check>();
    }
}

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
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
        builder
            .Property(p => p.CreatedAt)
            .IsRequired();
        builder
            .HasIndex(p => p.Url)
            .IsUnique();
    }
}