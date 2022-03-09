using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.WebApi.Models.Responses;

namespace TimePlanner.WebApi.Mappers;

internal class StatusMapper : IStatusMapper
{
  public StatusResponse Map(Status status)
  {
    return new StatusResponse(
      status.StartedAt,
      status.Deposit.Duration,
      status.Pause.Duration,
      new WorkingTimeResponse(
        status.WorkingTime.Distributed.Duration,
        status.WorkingTime.Undistributed.Duration),
      status.WorkItems.Select(
        i => new WorkItemResponse(i.Name, i.Duration.Duration)).ToList());
  }
}
