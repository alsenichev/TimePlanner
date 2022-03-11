namespace TimePlanner.WebApi.Models.Responses
{
  public record struct StatusResponse(
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

  public record struct WorkItemResponse(string Name, TimeSpan Duration);
}
