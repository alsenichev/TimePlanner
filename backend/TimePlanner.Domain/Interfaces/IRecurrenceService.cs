using TimePlanner.Domain.Models;

namespace TimePlanner.Domain.Interfaces
{
  public interface IRecurrenceService
  {
    DateTime? CalculateNextTime(string cronExpression, DateTime? lastFiredAt, DateTime relativeTo);
  }
}
