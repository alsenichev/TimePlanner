using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.DataAccess.Mappers
{
  public class WorkItemEntityMapper : IWorkItemEntityMapper

  {
    public WorkItem Map(WorkItemEntity entity)
    {
      return new WorkItem(entity.WorkItemEntityId, entity.Name, entity.Duration);
    }

    public WorkItemEntity Map(WorkItem workItem)
    {
      return new WorkItemEntity
      {
        WorkItemEntityId = workItem.Id ?? Guid.Empty,
        Duration = workItem.Duration.Duration,
        Name = workItem.Name
      };
    }
  }
}
