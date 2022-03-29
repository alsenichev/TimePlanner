using System.ComponentModel.DataAnnotations;

namespace TimePlanner.DataAccess.Entities
{
  public class RecurrenceEntity
  {
    [Key]
    public Guid WorkItemId { get; set; }
    public int? YearsEveryN { get; set; }
    public int? MonthsEveryN { get; set; }
    public int? WeeksEveryN { get; set; }
    public int? DaysEveryN { get; set; }
    public List<int>? YearsCustom { get; set; }
    public List<int>? MonthsCustom { get; set; }
    public List<int>? WeeksCustom { get; set; }
    public List<int>? DaysCustom { get; set; }
    public bool IsAfterPreviousCompleted { get; set; }
  }
}
