using TimePlanner.Domain.Core.WorkItemsTracking;

namespace TimePlanner.Domain.Services.Interfaces
{
  public interface IStatusRepository
  {
    Task<Status> GetStatusAsync(Guid statusId);

    Task<List<Status>> GetStatusesAsync(int count);

    Task<Status> SaveStatusAsync(Status status);

    Task DeleteWorkItemAsync(Guid statusId, Guid workItemId);
  }
}
