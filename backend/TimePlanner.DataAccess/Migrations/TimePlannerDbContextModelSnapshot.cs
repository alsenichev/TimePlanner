using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TimePlanner.DataAccess;

#nullable disable

namespace TimePlanner.DataAccess.Migrations
{
    [DbContext(typeof(TimePlannerDbContext))]
    partial class TimePlannerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TimePlanner.DataAccess.Entities.DurationEntity", b =>
                {
                    b.Property<int>("DurationEntityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("DurationEntityId"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("Value")
                        .HasColumnType("interval");

                    b.Property<Guid>("WorkItemId")
                        .HasColumnType("uuid");

                    b.HasKey("DurationEntityId");

                    b.HasIndex("WorkItemId");

                    b.ToTable("Durations");
                });

            modelBuilder.Entity("TimePlanner.DataAccess.Entities.StatusEntity", b =>
                {
                    b.Property<Guid>("StatusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("BreakStartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("Deposit")
                        .HasColumnType("interval");

                    b.Property<TimeSpan>("Pause")
                        .HasColumnType("interval");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("UndistributedTime")
                        .HasColumnType("interval");

                    b.HasKey("StatusId");

                    b.ToTable("Statuses");
                });

            modelBuilder.Entity("TimePlanner.DataAccess.Entities.WorkItemEntity", b =>
                {
                    b.Property<Guid>("WorkItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CronExpression")
                        .HasColumnType("text");

                    b.Property<bool?>("IsIfPreviousCompleted")
                        .HasColumnType("boolean");

                    b.Property<bool?>("IsOnPause")
                        .HasColumnType("boolean");

                    b.Property<int?>("MaxRepetitionCount")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("NextTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("RecurrenceEndsOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("RecurrenceStartsOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("RepetitionCount")
                        .HasColumnType("integer");

                    b.Property<int>("SortOrder")
                        .HasColumnType("integer");

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
