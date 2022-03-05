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
    public Guid Iteration { get; set; }
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
            .Property(p => p.Id)
            .IsRequired();
        builder
            .Property(p => p.SiteId)
            .IsRequired();
        builder
            .Property(p => p.Status)
            .IsRequired();
        builder
            .Property(p => p.StatusCode)
            .IsRequired();
        builder
            .Property(p => p.CheckedAt)
            .IsRequired();
        builder
            .Property(p => p.SpentTime)
            .IsRequired();
        builder
            .Property(p => p.RegionId)
            .IsRequired();
        builder
            .HasOne(r => r.Site)
            .WithMany(a => a.Checks)
            .HasForeignKey(r => r.SiteId);
        builder
            .HasOne(c => c.Region);
        builder
            .Property(p => p.Iteration)
            .IsRequired();
    }
}
