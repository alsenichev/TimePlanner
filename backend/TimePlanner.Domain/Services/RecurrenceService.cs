using Cronos;
using TimePlanner.Domain.Interfaces;

namespace TimePlanner.Domain.Services
{
  public class RecurrenceService : IRecurrenceService
  {
    public DateTime? CalculateNextTime(string cronExpression, DateTime relativeTo)
    {
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
  }
}
