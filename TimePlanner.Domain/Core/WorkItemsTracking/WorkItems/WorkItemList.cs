using TimePlanner.Domain.Core.WorkItemsTracking.Segments;

namespace TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

/// <summary>
/// The list of work items.
/// </summary>
public class WorkItemList
{
  /// <summary>
  /// Encapsulates work item information
  /// </summary>
  private record InnerWorkItem(string Name);

  private readonly DaySegments durations;
  private readonly List<InnerWorkItem> workItems;

  /// <summary>
  /// Constructor.
  /// </summary>
  public WorkItemList()
  {
    const int maxWorkItemsCount = 24 * 4;
    durations = new DaySegments(maxWorkItemsCount);
    workItems = new List<InnerWorkItem>(maxWorkItemsCount);
  }

  /// <summary>
  /// Add a new work item.
  /// </summary>
  public void AddWorkItem(string workItemName)
  {
    durations.CreateNewSegment();
    workItems.Add(new InnerWorkItem(workItemName));
  }

  /// <summary>
  /// Remove the work item.
  /// </summary>
  public void RemoveWorkItem(int index)
  {
    durations.RemoveSegmentAt(index);
    workItems.RemoveAt(index);
  }

  /// <summary>
  /// Add to the duration of the work item.
  /// </summary>
  public void AddToDuration(int index, TimeSpanValue duration)
  {
    durations.AddToSegment(index, duration);
  }

  /// <summary>
  /// Remove time from the work item's duration.
  /// </summary>
  public void RemoveFromDuration(int index, TimeSpanValue duration)
  {
    durations.RemoveFromSegment(index, duration);
  }

  /// <summary>
  /// Total time distributed across work items.
  /// </summary>
  public TimeSpanValue DistributedTime => durations.DistributedValue;

 /// <summary>
  /// Get all work items.
  /// </summary>
  public List<WorkItem> GetWorkItems()
  {
    return workItems.Zip(durations.Segments)
      .Select(t => new WorkItem(t.First.Name, t.Second)).ToList();
  }
}
