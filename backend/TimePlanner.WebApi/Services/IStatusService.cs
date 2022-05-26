using TimePlanner.Domain.Models;

namespace TimePlanner.WebApi.Services;

public interface IStatusService
{
  Task<List<Status>> GetStatuses(int count);

  Task<Status> GetCurrentStatusAsync();

  Task<Status> SetPause(Guid statusId, TimeSpan duration);

  Task<Status> StartBreak(Guid statusId);

  Task<Status> EndBreak(Guid statusId);

  Task<Status> CancelBreak(Guid statusId);

  Task<Status> FixStartTime(Guid statusId, TimeOnly startTime);
}
