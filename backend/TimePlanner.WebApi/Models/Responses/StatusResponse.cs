namespace TimePlanner.WebApi.Models.Responses
{
  public record struct StatusResponse(
    Guid Id,
    DateTime StartedAt,
    DateTime? BreakStartedAt,
    TimeSpan Deposit,
    TimeSpan Pause,
    TimeSpan UndistributedTime);
}
