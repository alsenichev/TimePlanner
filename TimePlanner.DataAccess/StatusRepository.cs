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
      return currentStatus;
    }

    public async Task<Status?> GetPreviousStatusAsync(DateOnly date)
    {
      if (previousStatus is null)
      {
        var builder = StatusBuilder.Of(TimeSpan.FromMinutes(16), date.AddDays(-1).ToDateTime(TimeOnly.MinValue));
        previousStatus = builder.Build();
      }

      return previousStatus.Value;
    }

    public async Task SaveStatusAsync(Status status)
    {
      currentStatus = status;
    }
  }
}
