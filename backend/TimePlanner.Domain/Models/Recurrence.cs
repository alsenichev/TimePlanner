namespace TimePlanner.Domain.Models
{
  public record struct Recurrence(
    Guid WorkItemId,
    int? YearsEveryN,
    List<int>? YearsCustom,
    int? MonthsEveryN,
    List<int>? MonthsCustom,
    int? WeeksEveryN,
    List<int>? WeeksCustom,
    int? DaysEveryN,
    List<int>? DaysCustom,
    bool? IsAfterPreviousCompleted);
}
