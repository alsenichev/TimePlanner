using TimePlanner.Domain.Core.WorkItemsTracking;

namespace TimePlanner.WebApi.Services;

public interface IStatusService
{
  Task<List<Status>> GetStatuses(int count);

  Task<Status> GetCurrentStatusAsync();

  Task<Status> AddWorkItemAsync(Guid statusId, string requestName);

  Task<Status> DistributeWorkingTimeAsync(Guid statusId, Guid workItemId, TimeSpan duration);

  Task<Status> SetPause(Guid statusId, TimeSpan duration);

  Task<Status> StartBreak(Guid statusId);

  Task<Status> EndBreak(Guid statusId);

  Task<Status> CancelBreak(Guid statusId);

  Task<Status> DeleteWorkItemAsync(Guid statusId, Guid workItemId);

  Task<Status> FixStartTime(Guid statusId, TimeOnly startTime);
}
