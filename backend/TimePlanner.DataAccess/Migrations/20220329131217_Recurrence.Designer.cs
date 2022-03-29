﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimePlanner.DataAccess;

#nullable disable

namespace TimePlanner.DataAccess.Migrations
{
    [DbContext(typeof(TimePlannerDbContext))]
    [Migration("20220329131217_Recurrence")]
    partial class Recurrence
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("TimePlanner.DataAccess.Entities.DurationEntity", b =>
                {
                    b.Property<int>("DurationEntityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DurationEntityId"), 1L, 1);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("Value")
                        .HasColumnType("time");

                    b.Property<Guid>("WorkItemId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("DurationEntityId");

                    b.HasIndex("WorkItemId");

                    b.ToTable("Durations");
                });

            modelBuilder.Entity("TimePlanner.DataAccess.Entities.StatusEntity", b =>
                {
                    b.Property<Guid>("StatusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("BreakStartedAt")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("Deposit")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("Pause")
                        .HasColumnType("time");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("UndistributedTime")
                        .HasColumnType("time");

                    b.HasKey("StatusId");

                    b.ToTable("Statuses");
                });

            modelBuilder.Entity("TimePlanner.DataAccess.Entities.WorkItemEntity", b =>
                {
                    b.Property<Guid>("WorkItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DaysCustom")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DaysEveryN")
                        .HasColumnType("int");

                    b.Property<bool?>("IsAfterPreviousCompleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRecurrent")
                        .HasColumnType("bit");

                    b.Property<string>("MonthsCustom")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MonthsEveryN")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("NextTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.Property<string>("WeeksCustom")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WeeksEveryN")
                        .HasColumnType("int");

                    b.Property<string>("YearsCustom")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("YearsEveryN")
                        .HasColumnType("int");

                    b.HasKey("WorkItemId");

                    b.ToTable("WorkItems");
                });

            modelBuilder.Entity("TimePlanner.DataAccess.Entities.DurationEntity", b =>
                {
                    b.HasOne("TimePlanner.DataAccess.Entities.WorkItemEntity", null)
                        .WithMany("Durations")
                        .HasForeignKey("WorkItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TimePlanner.DataAccess.Entities.WorkItemEntity", b =>
                {
                    b.Navigation("Durations");
                });
#pragma warning restore 612, 618
        }
    }
}
