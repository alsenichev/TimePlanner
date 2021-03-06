using TimePlanner.Domain.Exceptions;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.WorkItemsTracking.Segments;

namespace TimePlanner.Domain.Services
{
  public class StatusBuilder
  {
    private readonly DaySegment deposit;
    private readonly DaySegment pause;
    private readonly Guid? id;

    private DateTime startedAt;
    private DaySegment timePool;
    private DateTime? breakStartedAt;

    private StatusBuilder(Guid? id, TimeSpanValue deposit, DateTime startedAt)
    {
      this.id = id;
      this.startedAt = startedAt;
      timePool = DaySegment.Empty();
      pause = DaySegment.Empty();
      this.deposit = new DaySegment(deposit);
    }

    private void UpdateTimePool(
      TimeSpanValue depositUpdate,
      TimeSpanValue pauseUpdate)
    {
      var now = DateTime.UtcNow;
      var requestedDate = DateOnly.FromDateTime(now);
      var startedAtDate = DateOnly.FromDateTime(startedAt);
      if (!requestedDate.Equals(startedAtDate))
      {
        throw new StatusBuilderException(
          $"Can not update status: the requested date {requestedDate} doesn't match the start date {startedAtDate}");
      }

      var result = now - startedAt;
      if (result < TimeSpan.Zero)
      {
        throw new ApplicationException(
          $"Can not update status: the requested time {now} is less than the start time {startedAt}");
      }

      timePool = new DaySegment(result);
      timePool.Increase(depositUpdate);
      timePool.Decrease(pauseUpdate);
    }

    public Status Build()
    {
      return new Status(
        id,
        startedAt,
        breakStartedAt,
        deposit.Value,
        pause.Value,
        timePool.Value
      );
    }

    public static StatusBuilder CreateStatusBuilder(Status status)
    {
      var builder = new StatusBuilder(status.Id.Value, status.Deposit, status.StartedAt);
      builder.breakStartedAt = status.BreakStartedAt;
      builder.pause.Increase(status.Pause);
      builder.UpdateTimePool(status.Deposit, status.Pause);

      return builder;
    }

    public static StatusBuilder Of(TimeSpanValue deposit)
    {
      var builder = new StatusBuilder(null, deposit, DateTime.UtcNow);
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

    public StatusBuilder StartBreak()
    {
      breakStartedAt = DateTime.UtcNow;
      return this;
    }

    public StatusBuilder EndBreak()
    {
      if (!breakStartedAt.HasValue)
      {
        return this;
      }

      var now = DateTime.UtcNow;
      var requestedDate = DateOnly.FromDateTime(now);
      var breakAtDate = DateOnly.FromDateTime(breakStartedAt.Value);
      if (!requestedDate.Equals(breakAtDate))
      {
        throw new StatusBuilderException(
          $"Can not end break: the requested date {requestedDate} doesn't match the break start date {breakAtDate}");
      }

      var breakTime = now - breakStartedAt.Value;
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
      UpdateTimePool(deposit.Value, pause.Value);
      return this;
    }
  }
}
