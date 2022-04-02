using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimePlanner.DataAccess.Entities
{
  [Table("WorkItems")]
  public class WorkItemEntity
  {
    #region base properties
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid WorkItemId { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
    #endregion

    #region recurrence
    public string? CronExpression { get; set; }

    public DateTime? RecurrenceStartsOn { get; set; }

    public DateTime? RecurrenceEndsOn { get; set; }

    public int? RepetitionsCount { get; set; }

    public int? MaxRepetitionsCount { get; set; }

    public bool? IsIfPreviousCompleted { get; set; }

    public bool ? IsOnPause { get; set; }

    public DateTime? NextTime { get; set; }
    #endregion

    #region durations
    public List<DurationEntity>? Durations { get; set; }
    #endregion
  }
}
