using TimePlanner.Domain.Models;

namespace TimePlanner.WebApi.Models.Requests
{
  public record UpdateWorkItemRequest(
    Guid id,
    string name,
    List<DurationRequest> durations,
    string category,
    WakingUpRequest? wakingUp,
    int? recurrenceDays,
    DateTime createdAt,
    DateTime? completedAt,
    int sortOrder);

  public record struct DurationRequest(int? id, string date, string value);

  public record struct WakingUpRequest(string when, string where);
}