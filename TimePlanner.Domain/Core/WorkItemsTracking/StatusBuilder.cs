using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.Domain.Core.WorkItemsTracking
{
  public class StatusBuilder
  {
    private readonly WorkItemList workItemList;
    private readonly DaySegment deposit;
    private readonly DaySegment pause;
    private readonly Guid? id;

    private DateTime startedAt;
    private DaySegment timePool;
    private DateTime? breakStartedAt;

    private StatusBuilder(Guid? id, TimeSpanValue deposit, DateTime startedAt)
    {
      workItemList = new WorkItemList();
      this.id = id;
      this.startedAt = startedAt;
      this.timePool = DaySegment.Empty();
      this.pause = DaySegment.Empty();
      this.deposit = new DaySegment(deposit);
    }

    private void UpdateTimePool(
      TimeSpanValue depositUpdate,
      TimeSpanValue pauseUpdate,
      TimeSpanValue distributed)
    {
      DateTime now = DateTime.Now;
      DateOnly requestedDate = DateOnly.FromDateTime(now);
      DateOnly startedAtDate = DateOnly.FromDateTime(startedAt);
      if (!requestedDate.Equals(startedAtDate))
      {
        throw new StatusBuilderException(
          $"Can not update status: the requested date {requestedDate} doesn't match the start date {startedAtDate}");
      }

      TimeSpan result = now - startedAt;
      if (result < TimeSpan.Zero)
      {
        throw new ApplicationException(
          $"Can not update status: the requested time {now} is less than the start time {startedAt}");
      }

      timePool = new DaySegment(result);
      timePool.Increase(depositUpdate);
      timePool.Decrease(pauseUpdate);
      timePool.Decrease(distributed);
    }

    public Status Build()
    {
      return new Status(
        id,
        startedAt,
        breakStartedAt,
        deposit.Value,
        pause.Value,
        new RegisteredTime(workItemList.DistributedTime, timePool.Value),
        WorkItems
      );
    }

    private List<WorkItem> WorkItems => workItemList.WorkItems;

    public static StatusBuilder CreateStatusBuilder(Status status)
    {
      var builder = new StatusBuilder(status.Id.Value, status.Deposit, status.StartedAt);
      builder.breakStartedAt = status.BreakStartedAt;
      builder.pause.Increase(status.Pause);
      builder.UpdateTimePool(status.Deposit, status.Pause, status.RegisteredTime.Distributed);

      builder.workItemList.ImportWorkItems(status.WorkItems);

      return builder;
    }

    public static StatusBuilder Of(TimeSpanValue deposit)
    {
      var builder = new StatusBuilder(null, deposit, DateTime.Now);
      builder.timePool.Increase(deposit);
      return builder;
    }

    public StatusBuilder RegisterPause(TimeSpan timeSpan)
    {
      if (timeSpan >= TimeSpan.Zero)
      {
        timePool.Decrease(timeSpan);
        pause.Increase(timeSpan);
      }
      else
      {
        pause.Decrease(timeSpan.Duration());
        timePool.Increase(timeSpan.Duration());
      }

      return this;
    }

    public StatusBuilder CreateWorkItem(string workItemName)
    {
      workItemList.AddWorkItem(workItemName);
      return this;
    }

    public StatusBuilder DistributeWorkingTime(Guid workItemId, TimeSpan duration)
    {
      if (duration > TimeSpan.Zero)
      {
        timePool.Decrease(duration);
        workItemList.AddToDuration(workItemId, duration);
      }
      else
      {
        workItemList.RemoveFromDuration(workItemId, duration.Duration());
        timePool.Increase(duration.Duration());
      }
      return this;
    }

    public StatusBuilder StartBreak()
    {
      breakStartedAt = DateTime.Now;
      return this;
    }

    public StatusBuilder EndBreak()
    {
      if (!breakStartedAt.HasValue)
      {
        return this;
      }

      DateTime now = DateTime.Now;
      DateOnly requestedDate = DateOnly.FromDateTime(now);
      DateOnly breakAtDate = DateOnly.FromDateTime(breakStartedAt.Value);
      if (!requestedDate.Equals(breakAtDate))
      {
        throw new StatusBuilderException(
          $"Can not end break: the requested date {requestedDate} doesn't match the break start date {breakAtDate}");
      }

      TimeSpan breakTime = now - breakStartedAt.Value;
      if (breakTime < TimeSpan.Zero)
      {
        throw new InvalidOperationException(
          $"Can not end break: the requested time {now} is less than the break start time {breakStartedAt}");
      }

      RegisterPause(breakTime);
      breakStartedAt = null;
      return this;
    }

    public StatusBuilder CancelBreak()
    {
      breakStartedAt = null;
      return this;
    }

    public StatusBuilder FixStartTime(TimeOnly startTime)
    {
      startedAt = new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startTime.Hour, startTime.Minute,
        startTime.Second);
      UpdateTimePool(deposit.Value, pause.Value, workItemList.DistributedTime);
      return this;
    }
  }
}
