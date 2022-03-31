using Cronos;
using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Models;

namespace TimePlanner.Domain.Services
{
  public class RecurrenceService : IRecurrenceService
  {
    public DateTime? CalculateNextTime(string cronExpression, DateTime relativeTo)
    {
      try
      {
        CronExpression expression = CronExpression.Parse(cronExpression);
        DateTimeOffset? next = expression.GetNextOccurrence(relativeTo.ToUniversalTime());
        return next?.DateTime; ;
      }
      catch (CronFormatException)
      {
        throw new ApplicationException($"Cron expression can not be parsed: {cronExpression}");
      }
    }
  }
}
