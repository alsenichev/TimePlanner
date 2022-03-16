namespace TimePlanner.WebApi.Models.Responses
{
  public record struct StatusResponse(
    Guid Id,
    DateTime StartedAt,
    DateTime? BreakStartedAt,
    TimeSpan Deposit,
    TimeSpan Pause,
    WorkingTimeResponse WorkingTime,
    List<WorkItemResponse> WorkItems);

  public record struct WorkingTimeResponse(TimeSpan Distributed, TimeSpan Undistributed)
  {
    public TimeSpan Total => Distributed + Undistributed;
  }

  public record struct WorkItemResponse(Guid Id, string Name, TimeSpan Duration);
}
