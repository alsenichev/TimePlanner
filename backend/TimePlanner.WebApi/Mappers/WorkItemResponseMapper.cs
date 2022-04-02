using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Models.Responses;

namespace TimePlanner.WebApi.Mappers;

internal class WorkItemResponseMapper : IWorkItemResponseMapper
{
  private DurationResponse Map(Duration duration)
  {
    return new DurationResponse(duration.id.Value, duration.Date.ToString(), duration.Value.Duration.ToString());
  }

  public WorkItemResponse Map(WorkItem workItem)
  {
    return new WorkItemResponse
    {
      Category = workItem.Category,
      CompletedAt = workItem.CompletedAt,
      CreatedAt = workItem.CreatedAt,
      Durations = workItem.Durations?.Select(d => Map(d)).ToList() ?? new List<DurationResponse>(),
      Id = workItem.Id.Value,
      Name = workItem.Name,
      SortOrder = workItem.SortOrder,
      CronExpression = workItem.CronExpression,
      IsIfPreviousCompleted = workItem.IsIfPreviousCompleted,
      MaxRepetitionCount = workItem.MaxRepetitionCount,
      RecurrenceStartsOn = workItem.RecurrenceStartsOn,
      RecurrenceEndsOn = workItem.RecurrenceEndsOn,
      IsOnPause = workItem.IsOnPause,
      NextTime = workItem.NextTime
    };
  }
}
