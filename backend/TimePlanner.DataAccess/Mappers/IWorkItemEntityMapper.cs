using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;

namespace TimePlanner.DataAccess.Mappers
{
  public interface IWorkItemEntityMapper
  {
    WorkItem Map(WorkItemEntity entity);

    WorkItemEntity CreateEntity(WorkItem workItem);

    SortData MapSortData(WorkItemEntity entity);

    void UpdateEntity(WorkItemEntity entity, WorkItem workItem);

    void CleanUpRecurrence(WorkItemEntity entity);

    void CopyRecurrence(WorkItemEntity entity, WorkItemEntity newEntity);
  }
}
