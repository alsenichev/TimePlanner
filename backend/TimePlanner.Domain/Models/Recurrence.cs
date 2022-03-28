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

  public record struct Days(int Number, bool IsStrict);

  public record struct EveryNWeeks(List<DayOfWeek> WeekDays, int Which, bool IsStrict);

  public record struct EveryNMonths(List<int> Days, List<Month> Which, bool IsStrict);

  public record struct EveryNYears(List<int> Days, List<Month> Months, int Which, bool IsStrict);

  public interface IRecurrence
  {
    public DateOnly NextDate();
  }
}
