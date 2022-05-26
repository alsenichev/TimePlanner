using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;

namespace TimePlanner.DataAccess.Mappers
{
  public class StatusEntityMapper : IStatusEntityMapper
  {
    private readonly IWorkItemEntityMapper workItemEntityMapper;

    public StatusEntityMapper(IWorkItemEntityMapper workItemEntityMapper)
    {
      this.workItemEntityMapper = workItemEntityMapper;
    }

    public StatusEntity Map(Status status)
    {
      return new StatusEntity
      {
        StatusId = status.Id ?? Guid.Empty,
        BreakStartedAt = status.BreakStartedAt,
        Deposit = status.Deposit.Duration,
        Pause = status.Pause.Duration,
        StartedAt = status.StartedAt,
        UndistributedTime = status.UndistributedTime.Duration,
      };
    }

    public Status Map(StatusEntity entity)
    {
      return new Status(
        entity.StatusId,
        entity.StartedAt,
        entity.BreakStartedAt,
        entity.Deposit,
        entity.Pause,
        entity.UndistributedTime);
    }
  }
}
