namespace TimePlanner.Domain.Models
{
  public record struct Status(
    Guid? Id,
    DateTime StartedAt,
    DateTime? BreakStartedAt,
    TimeSpanValue Deposit,
    TimeSpanValue Pause,
    TimeSpanValue UndistributedTime);
}
