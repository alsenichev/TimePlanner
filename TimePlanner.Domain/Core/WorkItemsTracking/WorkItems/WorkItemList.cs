using System.Collections.ObjectModel;
using TimePlanner.Domain.Core.WorkItemsTracking.Segments;

namespace TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

/// <summary>
/// The list of work items.
/// </summary>
public class WorkItemList
{
  private readonly DaySegments durations;
  private readonly List<WorkItem> workItems;

  /// <summary>
  /// Constructor.
  /// </summary>
  public WorkItemList()
  {
    const int maxWorkItemsCount = 24 * 4;
    durations = new DaySegments(maxWorkItemsCount);
    workItems = new List<WorkItem>(maxWorkItemsCount);
  }

  /// <summary>
  /// Add a new work item.
  /// </summary>
  public void CreateWorkItem(string workItemName)
  {
    durations.CreateNewSegment();
    workItems.Add(new WorkItem(null, workItemName, TimeSpan.Zero));
  }

  public void AddWorkItem(WorkItem workItem)
  {
    int i = durations.CreateNewSegment();
    durations.AddToSegment(i, workItem.Duration);
    workItems.Add(workItem);
  }

  /// <summary>
  /// Remove the work item.
  /// </summary>
  public TimeSpanValue RemoveWorkItem(Guid workItemId)
  {
    int index = workItems.FindIndex(i => i.Id == workItemId);
    if (index < 0)
    {
      throw new MissingSegmentException();
    }
    workItems.RemoveAt(index);
    return durations.RemoveSegmentAt(index);
  }

  /// <summary>
  /// Add to the duration of the work item.
  /// </summary>
  public void AddToDuration(Guid workItemId, TimeSpanValue duration)
  {
    int index = workItems.FindIndex(i => i.Id == workItemId);
    durations.AddToSegment(index, duration);
    workItems[index] = workItems[index] with { Duration = durations.GetSegmentValue(index) };
  }

  /// <summary>
  /// Remove time from the work item's duration.
  /// </summary>
  public void RemoveFromDuration(Guid workItemId, TimeSpanValue duration)
  {
    int index = workItems.FindIndex(i => i.Id == workItemId);
    durations.RemoveFromSegment(index, duration);
    workItems[index] = workItems[index] with { Duration = durations.GetSegmentValue(index) };
  }

  /// <summary>
  /// Total time distributed across work items.
  /// </summary>
  public TimeSpanValue DistributedTime => durations.DistributedValue;

  /// <summary>
  /// Get all work items.
  /// </summary>
  public IReadOnlyCollection<WorkItem> WorkItems => new ReadOnlyCollection<WorkItem>(workItems);
}
