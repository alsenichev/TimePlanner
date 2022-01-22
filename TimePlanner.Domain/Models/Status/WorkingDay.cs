using TimePlanner.Domain.Models.Status.Segments;
using TimePlanner.Domain.Models.Status.WorkItems;
using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.Models.Status
{
  /// <summary>
  /// Represents the working day.
  /// </summary>
  public record WorkingDay
  {
    private static readonly TimeSpan workingDayNormalDuration = TimeSpan.FromHours(8);

    private readonly TimeDistribution timeCounter;
    private readonly WorkItemList workItemList;

    private WorkingDay()
    {
      timeCounter = new TimeDistribution();
      workItemList = new WorkItemList();
    }

    public IVoidResult<IStatusError> Init(DateTime startedAt, TimeSpan deposit)
    {
      IStatusError map(ISegmentError error) => error.Convert<IStatusError>(
        o => new DurationOverflow(o.AcceptableValue),
        _ => new UnexpectedError(),
        _ => new UnexpectedError()
      );
      StartedAt = startedAt;
      Deposit = deposit;
      return timeCounter.AddWorkingTime(deposit).MapError(map);
    }

    public TimeSpan WorkTime => timeCounter.WorkTime;

    public TimeSpan UndistributedWorkingTime => timeCounter.WorkTime - workItemList.TotalDuration;

    public TimeSpan DistributedWorkingTime => workItemList.TotalDuration;

    public TimeSpan Deposit { get; private set; }

    public TimeSpan Pause => timeCounter.LeisureTime;

    public DateTime StartedAt { get; set; }

    public List<WorkItem> WorkItems => workItemList.GetWorkItems();

    public IVoidResult<IStatusError> RegisterWorkingTime(TimeSpan timeSpan)
    {
      IStatusError map(ISegmentError error) => error.Convert<IStatusError>(
        o => new DurationOverflow(o.AcceptableValue),
        _ => new UnexpectedError(),
        _ => new UnexpectedError()
      );

      return timeSpan >= TimeSpan.Zero
        ? timeCounter.AddWorkingTime(timeSpan).MapError(map)
        : timeCounter.RemoveWorkingTime(timeSpan).MapError(map);
    }

    public IVoidResult<IStatusError> RegisterLeisureTime(TimeSpan timeSpan)
    {
      IStatusError map(ISegmentError error) => error.Convert<IStatusError>(
        o => new DurationOverflow(o.AcceptableValue),
        _ => new UnexpectedError(),
        _ => new UnexpectedError()
      );

      return timeSpan >= TimeSpan.Zero
        ? timeCounter.AddLeisureTime(timeSpan).MapError(map)
        : timeCounter.RemoveLeisureTime(timeSpan).MapError(map);
    }

    public IVoidResult<IStatusError> DistributeWorkingTime(int workItemIndex, TimeSpan duration)
    {
      return workItemList.AddToDuration(workItemIndex, duration);
    }

    public TimeSpan GetUnregisteredTimeRelativeToNow(DateTime now)
    {
      DateTime coveredUntil = StartedAt + UndistributedWorkingTime + Pause;
      return now - coveredUntil;
    }

    /// <summary>
    /// Indicates that the tracked time exceeds the daily limit.
    /// </summary>
    public bool IsOvertime(out TimeSpan overtime)
    {
      overtime = DistributedWorkingTime +
                 UndistributedWorkingTime -
                 workingDayNormalDuration;

      return overtime > TimeSpan.Zero;
    }
  }
}
