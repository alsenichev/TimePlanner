using TimePlanner.Domain.Core.WorkItemsTracking;

namespace TimePlanner.Domain.Services.Interfaces
{
  public interface IStatusRepository
  {
    Task<Status?> GetStatusAsync(DateOnly date);

    Task<Status?> GetPreviousStatusAsync(DateOnly date);

    Task SaveStatusAsync(Status status);
  }
}
