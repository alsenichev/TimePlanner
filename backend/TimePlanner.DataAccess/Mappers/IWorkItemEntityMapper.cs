using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Utils;

namespace TimePlanner.DataAccess.Mappers
{
  public interface IWorkItemEntityMapper
  {
    WorkItem Map(WorkItemEntity entity);

    SortData MapSortData(WorkItemEntity entity);

    WorkItemEntity Map(WorkItem workItem);

    DurationEntity Map(Guid workItemId, Duration duration);
    RecurrenceEntity? ParseRecurrence(Guid workItemId, string recurence);
  }
}
