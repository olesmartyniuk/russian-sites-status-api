﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RussianSitesStatus.Database;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RussianSitesStatus.Database.Models.Check", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CheckedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("checked_at");

                    b.Property<long>("RegionId")
                        .HasColumnType("bigint")
                        .HasColumnName("region_id");

                    b.Property<long>("SiteId")
                        .HasColumnType("bigint")
                        .HasColumnName("site_id");

                    b.Property<int>("SpentTime")
                        .HasColumnType("integer")
                        .HasColumnName("spent_time");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<int>("StatusCode")
                        .HasColumnType("integer")
                        .HasColumnName("status_code");

                    b.HasKey("Id");

                    b.HasIndex("CheckedAt");

                    b.HasIndex("RegionId");

                    b.HasIndex("SiteId");

                    b.ToTable("checks", (string)null);
                });

            modelBuilder.Entity("RussianSitesStatus.Database.Models.ChecksStatistics", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("data");

                    b.Property<DateTime>("Day")
                        .HasColumnType("date")
                        .HasColumnName("day");

                    b.Property<long>("SiteId")
                        .HasColumnType("bigint")
                        .HasColumnName("site_id");

                    b.HasKey("Id");

                    b.HasIndex("SiteId");

                    b.ToTable("checks_statistics", (string)null);
                });

            modelBuilder.Entity("RussianSitesStatus.Database.Models.Region", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Code")
                        .HasColumnType("text")
                        .HasColumnName("code");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<bool>("ProxyIsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("proxy_is_active");

                    b.Property<string>("ProxyPassword")
                        .HasColumnType("text")
                        .HasColumnName("proxy_password");

                    b.Property<string>("ProxyUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("proxy_url");

                    b.Property<string>("ProxyUser")
                        .HasColumnType("text")
                        .HasColumnName("proxy_user");

                    b.HasKey("Id");

                    b.ToTable("regions", (string)null);
                });

            modelBuilder.Entity("RussianSitesStatus.Database.Models.Site", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CheckedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("checked_at");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("url");

                    b.HasKey("Id");

                    b.HasIndex("CheckedAt");

                    b.HasIndex("Url")
                        .IsUnique();

                    b.ToTable("sites", (string)null);
                });

            modelBuilder.Entity("RussianSitesStatus.Database.Models.Check", b =>
                {
                    b.HasOne("RussianSitesStatus.Database.Models.Region", "Region")
                        .WithMany()
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RussianSitesStatus.Database.Models.Site", "Site")
                        .WithMany("Checks")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Region");

                    b.Navigation("Site");
                });

            modelBuilder.Entity("RussianSitesStatus.Database.Models.ChecksStatistics", b =>
                {
                    b.HasOne("RussianSitesStatus.Database.Models.Site", "Site")
                        .WithMany()
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Site");
                });

            modelBuilder.Entity("RussianSitesStatus.Database.Models.Site", b =>
                {
                    b.Navigation("Checks");
                });
#pragma warning restore 612, 618
        }
    }
}
