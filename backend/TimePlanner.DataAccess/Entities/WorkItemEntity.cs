using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimePlanner.DataAccess.Entities
{
  [Table("WorkItems")]
  public class WorkItemEntity
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid WorkItemId { get; set; }

    public string Name { get; set; } = null!;

    public List<DurationEntity>? Durations { get; set; }

    public string Category { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime? NextTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Cron { get; set; }

    public DateTime? RepetitionStartDate { get; set; }

    public int? RepetitionCount { get; set; }

    public int? MaxRepetitionCount { get; set; }

    public bool? IsAfterPreviousCompleted { get; set; }
  }
}
