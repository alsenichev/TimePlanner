using Microsoft.EntityFrameworkCore;
using TimePlanner.DataAccess.Entities;

namespace TimePlanner.DataAccess
{
  public class TimePlannerDbContext : DbContext
  {
    public TimePlannerDbContext(DbContextOptions<TimePlannerDbContext> options) : base(options)
    {
    }

    public TimePlannerDbContext()
    {
    }

    public DbSet<StatusEntity> StatusEntities { get; set; }
    public DbSet<WorkItemEntity> WorkItemEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<WorkItemEntity>()
        .HasMany(w => w.Durations)
        .WithOne()
        .HasForeignKey(e => e.WorkItemId)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
