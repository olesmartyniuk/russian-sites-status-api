using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace RussianSitesStatus.Database.Models;

public class ChecksStatistics : Entity
{
    public long SiteId { get; set; }
    public Site Site { get; set; }
    public DateTime Day { get; set; }
    public string Data { get; set; }
}

public class ChecksStatisticsConfiguration : IEntityTypeConfiguration<ChecksStatistics>
{
    public void Configure(EntityTypeBuilder<ChecksStatistics> builder)
    {
        builder
            .ToTable("checks_statistics");
        builder
            .Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();
        builder
            .Property(p => p.SiteId)
            .HasColumnName("site_id")
            .IsRequired();
        builder
            .Property(p => p.Day)
            .HasColumnName("day")
            .HasColumnType("date")
            .IsRequired();

        builder
            .Property(p => p.Data)
            .HasColumnName("data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder
            .HasOne(r => r.Site);
    }
}