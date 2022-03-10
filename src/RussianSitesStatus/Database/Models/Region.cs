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
    public bool ProxyIsActive { get; set; }
}

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder
            .ToTable("regions");
        builder
            .Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();
        builder
            .Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired();
        builder
            .Property(p => p.Code)
            .HasColumnName("code");            
        builder
            .Property(p => p.ProxyUrl)
            .HasColumnName("proxy_url")
            .IsRequired();
        builder
            .Property(p => p.ProxyUser)
            .HasColumnName("proxy_user");
        builder
            .Property(p => p.ProxyPassword)
            .HasColumnName("proxy_password");
        builder
            .Property(p => p.ProxyIsActive)
            .HasColumnName("proxy_is_active");
    }
}