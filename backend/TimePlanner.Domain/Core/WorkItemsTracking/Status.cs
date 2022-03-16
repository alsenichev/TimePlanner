using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.Domain.Core.WorkItemsTracking
{
  public record struct Status(
    Guid? Id,
    DateTime StartedAt,
    DateTime? BreakStartedAt,
    TimeSpanValue Deposit,
    TimeSpanValue Pause,
    RegisteredTime RegisteredTime,
    List<WorkItem> WorkItems);

  public record struct RegisteredTime(TimeSpanValue Distributed, TimeSpanValue Undistributed)
  {
    public TimeSpanValue Total => Distributed + Undistributed;
  }
}
