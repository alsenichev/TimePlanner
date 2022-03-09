using TimePlanner.Domain.Core.WorkItemsTracking.Segments;

namespace TimePlanner.Domain.Core.WorkItemsTracking.WorkItems
{
  /// <summary>
  /// Work item.
  /// </summary>
  public record struct WorkItem(string Name, TimeSpanValue Duration);
}
