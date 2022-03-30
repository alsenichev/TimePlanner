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
    List<int>? WeekDaysCustom,
    int? DaysEveryN,
    List<int>? DaysCustom,
    int? RepetitionCount,
    int? MaxRepetitionCount,
    bool? IsAfterPreviousCompleted);
}
