using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.Domain.Core.WorkItemsTracking
{
  public class StatusBuilder
  {
    private readonly DateTime startedAt;
    private readonly TimeSpanValue deposit;
    private readonly WorkItemList workItemList;
    private readonly DaySegment pause;

    private DaySegment timePool;
    private DateTime? breakStartedAt;

    private StatusBuilder(TimeSpanValue deposit, DateTime startedAt)
    {
      workItemList = new WorkItemList();
      this.deposit = deposit;
      this.startedAt = startedAt;
      this.timePool = DaySegment.Empty();
      this.pause = DaySegment.Empty();
    }

    private void UpdateTimePool(DateTime dateTime)
    {
      DateOnly requestedDate = DateOnly.FromDateTime(dateTime);
      DateOnly startedAtDate = DateOnly.FromDateTime(startedAt);
      if (!requestedDate.Equals(startedAtDate))
      {
        throw new StatusBuilderException(
          $"Can not update status: the requested date {requestedDate} doesn't match the start date {startedAtDate}");
      }

      TimeSpan result = dateTime - startedAt;
      if (result < TimeSpan.Zero)
      {
        throw new ApplicationException(
          $"Can not update status: the requested time {dateTime} is less than the start time {startedAt}");
      }

      timePool = new DaySegment(result);
    }

    public Status Build()
    {
      return new Status(
        startedAt,
        breakStartedAt,
        deposit,
        pause.Value,
        new RegisteredTime(workItemList.DistributedTime, timePool.Value),
        WorkItems
      );
    }

    private List<WorkItem> WorkItems => workItemList.GetWorkItems();

    public static StatusBuilder CreateStatusBuilder(Status status, DateTime dateTime)
    {
      var builder = new StatusBuilder(status.Deposit, status.StartedAt);
      builder.breakStartedAt = status.BreakStartedAt;
      builder.pause.Increase(status.Pause);
      builder.UpdateTimePool(dateTime);
      builder.timePool.Increase(status.Deposit);
      builder.timePool.Decrease(status.Pause);

      for (var i = 0; i < status.WorkItems.Count; i++)
      {
        WorkItem workItem = status.WorkItems[i];
        builder.AddWorkItem(workItem.Name);
        builder.DistributeWorkingTime(i, workItem.Duration.Duration);
      }

      return builder;
    }

    public static StatusBuilder Of(TimeSpanValue deposit, DateTime startedAt)
    {
      var builder = new StatusBuilder(deposit, startedAt);
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

    public StatusBuilder AddWorkItem(string workItemName)
    {
      workItemList.AddWorkItem(workItemName);
      return this;
    }

    public StatusBuilder DistributeWorkingTime(int workItemIndex, TimeSpan duration)
    {
      if (duration > TimeSpan.Zero)
      {
        timePool.Decrease(duration);
        workItemList.AddToDuration(workItemIndex, duration);
      }
      else
      {
        workItemList.RemoveFromDuration(workItemIndex, duration.Duration());
        timePool.Increase(duration.Duration());
      }
      return this;
    }

    public StatusBuilder StartBreak(DateTime dateTime)
    {
      breakStartedAt = dateTime;
      return this;
    }

    public StatusBuilder EndBreak(DateTime dateTime)
    {
      if (!breakStartedAt.HasValue)
      {
        return this;
      }
      DateOnly requestedDate = DateOnly.FromDateTime(dateTime);
      DateOnly breakAtDate = DateOnly.FromDateTime(breakStartedAt.Value);
      if (!requestedDate.Equals(breakAtDate))
      {
        throw new StatusBuilderException(
          $"Can not end break: the requested date {requestedDate} doesn't match the break start date {breakAtDate}");
      }

      TimeSpan breakTime = dateTime - breakStartedAt.Value;
      if (breakTime < TimeSpan.Zero)
      {
        throw new ApplicationException(
          $"Can not end break: the requested time {dateTime} is less than the break start time {breakStartedAt}");
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
  }
}
