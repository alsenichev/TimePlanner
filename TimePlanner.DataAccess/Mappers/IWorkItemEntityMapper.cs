using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.DataAccess.Mappers
{
  public interface IWorkItemEntityMapper
  {
    WorkItem Map(WorkItemEntity entity);

    WorkItemEntity Map(WorkItem workItem);
  }
}
