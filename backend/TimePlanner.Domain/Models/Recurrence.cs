namespace TimePlanner.Domain.Models
{
  public enum DayOfWeek
  {
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
  }

  public enum Month
  {
    January,
    February,
    March,
    April,
    May,
    June,
    July,
    August,
    September,
    October,
    November,
    December
  }

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
    bool IsAfterPreviousCompleted);

  public interface IRecurrence
  {
    public DateOnly NextDate();
  }
}
