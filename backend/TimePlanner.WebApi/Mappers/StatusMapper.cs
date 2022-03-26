using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Models.Responses;

namespace TimePlanner.WebApi.Mappers;

internal class StatusMapper : IStatusResponseMapper
{
  public StatusResponse Map(Status status)
  {
    return new StatusResponse(
      status.Id.Value,
      status.StartedAt,
      status.BreakStartedAt,
      status.Deposit.Duration,
      status.Pause.Duration,
      status.UndistributedTime.Duration);
  }
}
