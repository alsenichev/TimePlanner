using TimePlanner.Domain.Models.Status.Segments;
using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.Models.Status.WorkItems;

/// <summary>
/// The ist of work items.
/// </summary>
public class WorkItemList
{
  private readonly DaySegments durations;
  private readonly InnerWorkItem[] workItems;

  /// <summary>
  /// Constructor.
  /// </summary>
  public WorkItemList()
  {
    durations = new DaySegments();
    workItems = new InnerWorkItem[DaySegments.MaxSegmentsCount];
  }

  /// <summary>
  /// Add a new work item.
  /// </summary>
  public IResult<int, TooManyWorkItems> AddWorkItem(InnerWorkItem innerWorkItem)
  {
    return durations.CreateNewSegment()
      .Tee(i => workItems[i] = null)
      .MapError(e => new TooManyWorkItems());
  }

  /// <summary>
  /// Remove the work item.
  /// </summary>
  public IVoidResult<WorkItemDoesNotExist> RemoveWorkItem(int index)
  {
    return durations.RemoveSegmentAt(index)
      .Tee(() => { workItems[index] = null; })
      .MapError(e => new WorkItemDoesNotExist(e.Index));
  }

  /// <summary>
  /// Add to the duration of the work item.
  /// </summary>
  public IVoidResult<IStatusError> AddToDuration(int index, TimeSpan duration)
  {
    IStatusError map(ISegmentError error) => error.Convert<IStatusError>(
      o => new DurationOverflow(o.AcceptableValue),
      m => new WorkItemDoesNotExist(m.Index),
      _ => new UnexpectedError());

    return durations.AddToSegment(index, duration).MapError(map);
  }

  /// <summary>
  /// Remove time from the work item's duration.
  /// </summary>
  public IVoidResult<IStatusError> RemoveFromDuration(int index, TimeSpan duration)
  {
    IStatusError map(ISegmentError error) => error switch
    {
      Overflow overflow => new DurationOverflow(overflow.AcceptableValue),
      MissingSegment missingSegment => new WorkItemDoesNotExist(missingSegment.Index),
      _ => new UnexpectedError()
    };

    return durations.RemoveFromSegment(index, duration).MapError(map);
  }

  /// <summary>
  /// Total time distributed across work items.
  /// </summary>
  public TimeSpan TotalDuration => durations.DistributedValue;

  /// <summary>
  /// Get all work items.
  /// </summary>
  public List<WorkItem> GetWorkItems()
  {
    return workItems.Zip(durations.Segments)
      .Select(t => new WorkItem(t.First.Name, t.Second)).ToList();
  }
}
