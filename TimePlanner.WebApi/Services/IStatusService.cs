using TimePlanner.Domain.Core.WorkItemsTracking;

namespace TimePlanner.WebApi.Services;

public interface IStatusService
{
  Task<Status> GetStatusAsync(DateTime dateTime);

  Task<Status> AddWorkItemAsync(DateTime dateTime, string requestName);

  Task<Status> DistributeWorkingTimeAsync(DateTime dateTime, int workItemIndex, TimeSpan duration);

  Task<Status> SetPause(DateTime dateTime, TimeSpan duration);

  Task<Status> StartBreak(DateTime dateTime);

  Task<Status> EndBreak(DateTime dateTime);

  Task<Status> CancelBreak(DateTime dateTime);
}
