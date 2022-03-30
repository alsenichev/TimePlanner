using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Utils;

namespace TimePlanner.DataAccess.Mappers
{
  public interface IWorkItemEntityMapper
  {
    WorkItem Map(WorkItemEntity entity);

    SortData MapSortData(WorkItemEntity entity);

    Recurrence ExtractRecurrence(WorkItemEntity entity);

    void CopyRecurrence(WorkItemEntity source, WorkItemEntity target);

    void CleanUpRecurrence(WorkItemEntity entity);

    void AssignRecurrence(WorkItemEntity source, Recurrence target);
  }
}
