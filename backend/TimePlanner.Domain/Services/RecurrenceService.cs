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

    public DateTime CalculateNextTime(Recurrence recurrence)
    {
      throw new ApplicationException("The recurrence value can not be correctly processed.");
    }
  }
}
