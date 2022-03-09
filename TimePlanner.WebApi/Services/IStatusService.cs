using TimePlanner.Domain.Core.WorkItemsTracking;

namespace TimePlanner.WebApi.Services;

public interface IStatusService
{
  Task<Status> GetStatusAsync(DateTime dateTime);

  Task<Status> AddWorkItemAsync(DateTime dateTime, string requestName);

  Task<Status> DistributeWorkingTimeAsync(DateTime dateTime, int workItemIndex, TimeSpan duration);
}
