using TimePlanner.Domain.Core.WorkItemsTracking.Segments;

namespace TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

/// <summary>
/// The list of work items.
/// </summary>
public class WorkItemList
{
  private readonly DaySegments durations;

  /// <summary>
  /// Constructor.
  /// </summary>
  public WorkItemList()
  {
    const int maxWorkItemsCount = 24 * 4;
    durations = new DaySegments(maxWorkItemsCount);
    WorkItems = new List<WorkItem>(maxWorkItemsCount);
  }

  /// <summary>
  /// Add a new work item.
  /// </summary>
  public void AddWorkItem(string workItemName)
  {
    durations.CreateNewSegment();
    WorkItems.Add(new WorkItem(null, workItemName, TimeSpan.Zero));
  }

  /// <summary>
  /// Remove the work item.
  /// </summary>
  public TimeSpanValue RemoveWorkItem(Guid workItemId)
  {
    int index = WorkItems.FindIndex(i => i.Id == workItemId);
    WorkItems.RemoveAt(index);
    return durations.RemoveSegmentAt(index);
  }

  /// <summary>
  /// Add to the duration of the work item.
  /// </summary>
  public void AddToDuration(Guid workItemId, TimeSpanValue duration)
  {
    int index = WorkItems.FindIndex(i => i.Id == workItemId);
    durations.AddToSegment(index, duration);
    WorkItems[index] = WorkItems[index] with { Duration = durations.GetSegmentValue(index) };
  }

  /// <summary>
  /// Remove time from the work item's duration.
  /// </summary>
  public void RemoveFromDuration(Guid workItemId, TimeSpanValue duration)
  {
    int index = WorkItems.FindIndex(i => i.Id == workItemId);
    durations.RemoveFromSegment(index, duration);
    WorkItems[index] = WorkItems[index] with { Duration = durations.GetSegmentValue(index) };
  }

  public void ImportWorkItems(List<WorkItem> workItems)
  {
    for(int i = 0; i < workItems.Count; i++)
    {
      durations.CreateNewSegment();
      durations.AddToSegment(i, workItems[i].Duration);
      WorkItems.Add(workItems[i]);
    }
  }

  /// <summary>
  /// Total time distributed across work items.
  /// </summary>
  public TimeSpanValue DistributedTime => durations.DistributedValue;

  /// <summary>
  /// Get all work items.
  /// </summary>
  public List<WorkItem> WorkItems { get; }
}
