using Cronos;
using System.Text.RegularExpressions;
using TimePlanner.Domain.Interfaces;

namespace TimePlanner.Domain.Services
{
  public class RecurrenceService :  IRecurrenceService
  {
    private static readonly Regex daysAfterRegex = new Regex("^daysAfterCompletion: (?<number>[1-365])$");

    public DateTime? CalculateNextTime(string cronExpression, DateTime? lastFiredAt, DateTime relativeTo)
    {
      var match = daysAfterRegex.Match(cronExpression);
      if (match.Success)
      {
        return ParseDaysAfterCompletion(lastFiredAt, match.Groups["number"].Value);
      }
      try
      {
        var expression = CronExpression.Parse(cronExpression);
        var next = expression.GetNextOccurrence(new DateTimeOffset(relativeTo), TimeZoneInfo.Local);
        return next?.DateTime;
      }
      catch (CronFormatException)
      {
        throw new ApplicationException($"Cron expression can not be parsed: {cronExpression}");
      }
    }

    private DateTime? ParseDaysAfterCompletion(DateTime? lastFiredAt, string value)
    {
      if(lastFiredAt == null)
      {
        throw new ApplicationException("Previous execution time is not specified.");
      }
      int days = int.Parse(value);
      return lastFiredAt.Value.AddDays(days);
    }
  }
}
