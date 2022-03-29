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

    public bool IsRecurrent { get; set; }

    public int? YearsEveryN { get; set; }

    public int? MonthsEveryN { get; set; }

    public int? WeeksEveryN { get; set; }

    public int? DaysEveryN { get; set; }

    public string? YearsCustom { get; set; }

    public string? MonthsCustom { get; set; }

    public string? WeeksCustom { get; set; }

    public string? DaysCustom { get; set; }

    public bool? IsAfterPreviousCompleted { get; set; }
  }
}
