using TimePlanner.Domain.Interfaces;

namespace TimePlanner.Domain.Services
{
  public class ProductionTimeProvider : ITimeProvider
  {
    public DateTime Now => DateTime.UtcNow;
  }
}
