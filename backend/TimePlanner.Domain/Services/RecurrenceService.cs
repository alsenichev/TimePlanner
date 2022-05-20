using Cronos;
using System.Text.RegularExpressions;
using TimePlanner.Domain.Interfaces;

namespace TimePlanner.Domain.Services
{
  public class RecurrenceService :  IRecurrenceService
  {
    private static readonly Regex daysAfterRegex = new Regex(@"^daysAfterCompletion: (?<number>\d{1,3})$");

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
        return expression.GetNextOccurrence(relativeTo);
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
      if(days < 1 || days > 365)
      {
        throw new ApplicationException("Days range must be between 1-365");
      }
      return lastFiredAt.Value.AddDays(days);
    }
  }
}
