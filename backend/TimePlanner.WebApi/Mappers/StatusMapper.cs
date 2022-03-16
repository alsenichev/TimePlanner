using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.WebApi.Models.Responses;

namespace TimePlanner.WebApi.Mappers;

internal class StatusMapper : IStatusMapper
{
  public StatusResponse Map(Status status)
  {
    return new StatusResponse(
      status.Id.Value,
      status.StartedAt,
      status.BreakStartedAt,
      status.Deposit.Duration,
      status.Pause.Duration,
      new WorkingTimeResponse(
        status.RegisteredTime.Distributed.Duration,
        status.RegisteredTime.Undistributed.Duration),
      status.WorkItems.Select(
        i => new WorkItemResponse(i.Id.Value, i.Name, i.Duration.Duration)).ToList());
  }
}
