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
    public Proxy Region { get; set; }
}

//public enum Region
//{
//    Ukraine = 1,
//    Russia = 2,
//    Europe = 3
//}
public enum CheckStatus
{
    Available = 1,
    Unavailable = 2
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
            .Property(p => p.Region)
            .IsRequired();
        builder
            .HasOne(r => r.Site)
            .WithMany(a => a.Checks)
            .HasForeignKey(r => r.SiteId);

    }
}
