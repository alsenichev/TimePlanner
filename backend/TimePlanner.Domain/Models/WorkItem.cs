namespace TimePlanner.Domain.Models
{
  /// <summary>
  /// Work item.
  /// </summary>
  public record struct WorkItem(
    Guid? Id,
    string Name,
    List<Duration> Durations,
    Category Category,
    WakingUp? WakingUp,
    Recurrence? Recurrence,
    History History,
    int SortOrder);

  public record struct History(DateTime CreatedAt, DateTime? CompletedAt);

  public record struct Duration(int? id, DateOnly Date, TimeSpanValue Value);

  public record struct WakingUp(DateOnly When, Category Where);

  public record struct Recurrence(int Days);

  public enum Category
  {
    Today,
    Tomorrow,
    NextWeek,
    Completed,
  }
}
