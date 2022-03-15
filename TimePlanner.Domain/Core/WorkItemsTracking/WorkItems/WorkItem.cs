using TimePlanner.Domain.Core.WorkItemsTracking.Segments;

namespace TimePlanner.Domain.Core.WorkItemsTracking.WorkItems
{
  /// <summary>
  /// Work item.
  /// </summary>
  public record struct WorkItem(Guid? Id, string Name, TimeSpanValue Duration);
}
