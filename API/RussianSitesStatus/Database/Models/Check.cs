using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RussianSitesStatus.Database.Models;

public class Check : Entity
{
    public long SiteId { get; set; }
    public Site Site { get; set; }
    public CheckStatus Status { get; set; }
    public int StatusCode { get; set; }
    public int SpentTime { get; set; }
    public DateTime CheckedAt { get; set; }
    public long RegionId { get; set; }
    public Region Region { get; set; }
}

public enum CheckStatus
{
    Available = 1,
    Unknown = 2,
    Unavailable = 3
}

public class CheckConfiguration : IEntityTypeConfiguration<Check>
{
    public void Configure(EntityTypeBuilder<Check> builder)
    {
        builder
            .ToTable("checks");
        builder
            .Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();
        builder
            .Property(p => p.SiteId)
            .HasColumnName("site_id")
            .IsRequired();
        builder
            .Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired();
        builder
            .Property(p => p.StatusCode)
            .HasColumnName("status_code")
            .IsRequired();
        builder
            .Property(p => p.CheckedAt)
            .HasColumnName("checked_at")
            .IsRequired();
        builder
            .HasIndex(p => p.CheckedAt);
        builder
            .Property(p => p.SpentTime)
            .HasColumnName("spent_time")
            .IsRequired();
        builder
            .Property(p => p.RegionId)
            .HasColumnName("region_id")
            .IsRequired();
        builder
            .HasOne(r => r.Site)
            .WithMany(a => a.Checks)
            .HasForeignKey(r => r.SiteId);
        builder
            .HasOne(c => c.Region);
    }
}
