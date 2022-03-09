using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.Domain.Services.Interfaces;

namespace TimePlanner.DataAccess
{
  public class StatusRepository : IStatusRepository
  {
    private Status? previousStatus;
    private Status? currentStatus;

    public async Task<Status?> GetStatusAsync(DateOnly date)
    {
      if (DateTime.Now.Ticks % 7 == 0)
      {
        throw new DataAccessException("Status table is not accessible.");
      }
      await Task.Delay(800);
      return currentStatus;
    }

    public async Task<Status?> GetPreviousStatusAsync(DateOnly date)
    {
      if (previousStatus is null)
      {
        var builder = StatusBuilder.Of(TimeSpan.FromMinutes(16), date.AddDays(-1).ToDateTime(TimeOnly.MinValue));
        previousStatus = builder.Build();
      }
      if (DateTime.Now.Ticks % 8 == 0)
      {
        throw new DataAccessException("Status table is not accessible.");
      }

      await Task.Delay(1200);
      return previousStatus.Value;
    }

    public async Task SaveStatusAsync(Status status)
    {
      if (DateTime.Now.Ticks % 8 == 0)
      {
        throw new DataAccessException("Status table is not accessible.");
      }
      await Task.Delay(600);
      currentStatus = status;
    }
  }
}
