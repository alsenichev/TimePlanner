using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Models;

namespace TimePlanner.Domain.Services
{
  public class RecurrenceService : IRecurrenceService
  {
    private readonly ITimeProvider timeProvider;

    public RecurrenceService(ITimeProvider timeProvider)
    {
      this.timeProvider = timeProvider;
    }

    public DateTime? CalculateNextTime(Recurrence recurrence)
    {
      var baseDate = timeProvider.Now;
      if (recurrence.DaysEveryN.HasValue)
      {
        return baseDate.AddDays(recurrence.DaysEveryN.Value);
      }

      if (recurrence.DaysCustom != null && recurrence.DaysCustom.Count > 0)
      {
        var newDay = recurrence.DaysCustom.FirstOrDefault(d => d > baseDate.Day);
        if (newDay == 0)
        {
          return new DateTime(baseDate.Year, baseDate.Month, 1).AddMonths(1).AddDays(recurrence.DaysCustom[0]);
        }
      }

      throw new ApplicationException("The recurrence value can not be correctly processed.");
    }
  }
}
