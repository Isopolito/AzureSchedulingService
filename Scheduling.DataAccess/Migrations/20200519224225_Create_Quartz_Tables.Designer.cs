﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Scheduling.DataAccess.Contexts;

namespace Scheduling.DataAccess.Migrations
{
    [DbContext(typeof(SchedulingContext))]
    [Migration("20200519224225_Create_Quartz_Tables")]
    partial class Create_Quartz_Tables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Scheduling.DataAccess.Entities.Job", b =>
                {
                    b.Property<int>("JobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CronExpressionOverride")
                        .HasColumnType("nvarchar(998)")
                        .HasMaxLength(998);

                    b.Property<string>("DomainName")
                        .HasColumnType("nvarchar(2048)")
                        .HasMaxLength(2048);

                    b.Property<DateTime?>("EndAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("JobIdentifier")
                        .IsRequired()
                        .HasColumnType("nvarchar(75)")
                        .HasMaxLength(75);

                    b.Property<int>("RepeatEndStrategyId")
                        .HasColumnType("int");

                    b.Property<int>("RepeatIntervalId")
                        .HasColumnType("int");

                    b.Property<int>("RepeatOccurrenceNumber")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("SubscriptionName")
                        .IsRequired()
                        .HasColumnType("nvarchar(75)")
                        .HasMaxLength(75);

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.HasKey("JobId");

                    b.HasIndex("RepeatEndStrategyId");

                    b.HasIndex("RepeatIntervalId");

                    b.HasIndex("SubscriptionName", "JobIdentifier")
                        .IsUnique()
                        .HasName("UX_scheduling_Job_JobIdentifier_SubscriptionName");

                    b.ToTable("Job","scheduling");
                });

            modelBuilder.Entity("Scheduling.DataAccess.Entities.RepeatEndStrategy", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(75)")
                        .HasMaxLength(75);

                    b.HasKey("Id");

                    b.ToTable("RepeatEndStrategy","scheduling");

                    b.HasData(
                        new
                        {
                            Id = 0,
                            Name = "Not Used"
                        },
                        new
                        {
                            Id = 1,
                            Name = "Never"
                        },
                        new
                        {
                            Id = 3,
                            Name = "After Occurrence Number"
                        },
                        new
                        {
                            Id = 2,
                            Name = "On End Date"
                        });
                });

            modelBuilder.Entity("Scheduling.DataAccess.Entities.RepeatInterval", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(75)")
                        .HasMaxLength(75);

                    b.HasKey("Id");

                    b.ToTable("RepeatInterval","scheduling");

                    b.HasData(
                        new
                        {
                            Id = 0,
                            Name = "Not Used"
                        },
                        new
                        {
                            Id = 1,
                            Name = "Never"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Daily"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Weekly"
                        },
                        new
                        {
                            Id = 5,
                            Name = "Monthly"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Bi-Monthly"
                        },
                        new
                        {
                            Id = 6,
                            Name = "Quarterly"
                        });
                });

            modelBuilder.Entity("Scheduling.DataAccess.Entities.Job", b =>
                {
                    b.HasOne("Scheduling.DataAccess.Entities.RepeatEndStrategy", "RepeatEndStrategy")
                        .WithMany()
                        .HasForeignKey("RepeatEndStrategyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Scheduling.DataAccess.Entities.RepeatInterval", "RepeatInterval")
                        .WithMany()
                        .HasForeignKey("RepeatIntervalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
