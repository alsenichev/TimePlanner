using System.ComponentModel.DataAnnotations;

namespace TimePlanner.DataAccess.Entities
{
  public class RecurrenceEntity
  {
    [Key] public Guid WorkItemId { get; set; }
    public int? YearsEveryN { get; set; }
    public int? MonthsEveryN { get; set; }
    public int? WeeksEveryN { get; set; }
    public int? DaysEveryN { get; set; }
    public string? YearsCustom { get; set; }
    public string? MonthsCustom { get; set; }
    public string? WeeksCustom { get; set; }
    public string? DaysCustom { get; set; }
    public bool IsAfterPreviousCompleted { get; set; }
  }
}
