using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.DataAccess.Mappers
{
  public class StatusEntityMapper : IStatusEntityMapper
  {
    private readonly IWorkItemEntityMapper workItemEntityMapper;

    public StatusEntityMapper(IWorkItemEntityMapper workItemEntityMapper)
    {
      this.workItemEntityMapper = workItemEntityMapper;
    }

    private TimeSpanValue DistributedTime(List<WorkItemEntity>? workItemEntities)
    {
      return workItemEntities!=null && workItemEntities.Count > 0
        ? workItemEntities.Select(e => e.Duration).Aggregate(TimeSpan.Zero, (i, j) => i + j)
        : TimeSpan.Zero;
    }

    public StatusEntity Map(Status status)
    {
      return new StatusEntity
      {
        StatusEntityId = status.Id ?? Guid.Empty,
        BreakStartedAt = status.BreakStartedAt,
        Deposit = status.Deposit.Duration,
        Pause = status.Pause.Duration,
        StartedAt = status.StartedAt,
        UndistributedTime = status.RegisteredTime.Undistributed.Duration,
        WorkItems = status.WorkItems.Select(i => workItemEntityMapper.Map(i)).ToList()
      };
    }

    public Status Map(StatusEntity entity)
    {
      return new Status(
        entity.StatusEntityId,
        entity.StartedAt,
        entity.BreakStartedAt,
        entity.Deposit,
        entity.Pause,
        new RegisteredTime(DistributedTime(entity.WorkItems), entity.UndistributedTime),
        entity.WorkItems?.Select(i => workItemEntityMapper.Map(i)).ToList() ?? new List<WorkItem>());
    }
  }
}
