using TimePlanner.Domain.Models;

namespace TimePlanner.WebApi.Models.Requests
{
  public record UpdateWorkItemRequest(
    Guid Id,
    string Name,
    string Category,
    string Recurrence,
    int SortOrder,
    List<DurationRequest> durations);

  public record struct DurationRequest(int? id, string date, string value);
}