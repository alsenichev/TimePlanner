using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.Domain.Core.WorkItemsTracking
{
  public record struct Status(
    DateTime StartedAt,
    TimeSpanValue Deposit,
    TimeSpanValue Pause,
    WorkingTime WorkingTime,
    List<WorkItem> WorkItems);

  public record struct WorkingTime(TimeSpanValue Distributed, TimeSpanValue Undistributed)
  {
    public TimeSpanValue Total => Distributed + Undistributed;
  }
}
