using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.Domain.Core.WorkItemsTracking
{
  public class StatusBuilder
  {
    private readonly TimeDistribution timeCounter;
    private readonly WorkItemList workItemList;
    private readonly TimeSpanValue deposit;
    private readonly DateTime startedAt;

    private StatusBuilder(TimeSpanValue deposit, DateTime startedAt)
    {
      timeCounter = new TimeDistribution();
      workItemList = new WorkItemList();
      this.deposit = deposit;
      this.startedAt = startedAt;
    }

    public Status Build()
    {
      return new Status(
        startedAt,
        deposit,
        timeCounter.LeisureTime,
        new WorkingTime(workItemList.TotalDuration, timeCounter.WorkTime - workItemList.TotalDuration),
        WorkItems
      );
    }

    private List<WorkItem> WorkItems => workItemList.GetWorkItems();

    public static StatusBuilder CreateStatusBuilder(Status status)
    {
      var builder = new StatusBuilder(status.Deposit, status.StartedAt);
      builder.timeCounter.AddWorkingTime(status.WorkingTime.Total);
      builder.timeCounter.AddLeisureTime(status.Pause);

      for (var i = 0; i < status.WorkItems.Count; i++)
      {
        WorkItem workItem = status.WorkItems[i];
        builder.workItemList.AddWorkItem(workItem.Name);
        builder.workItemList.AddToDuration(i, workItem.Duration);
      }

      return builder;
    }

    public static StatusBuilder Of(TimeSpanValue deposit, DateTime startedAt)
    {
      var builder = new StatusBuilder(deposit, startedAt);
      builder.RegisterWorkingTime(deposit.Duration);
      return builder;
    }

    public StatusBuilder RegisterWorkingTime(TimeSpan timeSpan)
    {
      if (timeSpan >= TimeSpan.Zero)
      {
        timeCounter.AddWorkingTime(timeSpan);
      }
      else
      {
        timeCounter.RemoveWorkingTime(timeSpan);
      }

      return this;
    }

    public StatusBuilder RegisterLeisureTime(TimeSpan timeSpan)
    {
      if (timeSpan >= TimeSpan.Zero)
      {
        timeCounter.AddLeisureTime(timeSpan);
      }
      else
      {
        timeCounter.RemoveLeisureTime(timeSpan);
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
        workItemList.AddToDuration(workItemIndex, duration);
      }
      else
      {
        workItemList.RemoveFromDuration(workItemIndex, duration);
      }
      return this;
    }
  }
}
