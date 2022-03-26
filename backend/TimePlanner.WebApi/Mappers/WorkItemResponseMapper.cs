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
      CompletedAt = workItem.History.CompletedAt,
      CreatedAt = workItem.History.CreatedAt,
      Durations = workItem.Durations?.Select(d => Map(d)).ToList() ?? new List<DurationResponse>(),
      Id = workItem.Id.Value,
      Name = workItem.Name,
      RecurrenceDays = workItem.Recurrence.HasValue ? workItem.Recurrence.Value.Days : null,
      SortOrder = workItem.SortOrder,
      WakingUp = workItem.WakingUp.HasValue
        ? new WakingUpResponse
        {
          When = workItem.WakingUp.Value.When.ToString(),
          Where = workItem.WakingUp.Value.Where
        }
        : null
    };
  }
}
