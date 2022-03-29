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
    int YearEveryN,
    List<int> CustomYears,
    int MonthEveryN,
    List<int> CustomMonths,
    int WeekEveryN,
    List<int> CustomWeeks,
    int DayEveryN,
    List<int> CustomDays);

  public interface IRecurrence
  {
    public DateOnly NextDate();
  }
}
