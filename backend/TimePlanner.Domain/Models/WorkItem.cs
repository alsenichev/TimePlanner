namespace TimePlanner.Domain.Models
{
  /// <summary>
  /// Work item.
  /// </summary>
  public record struct WorkItem(
    Guid? Id,
    string Name,
    Category Category,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    DateTime? NextTime,
    string? Recurrence,
    int SortOrder,
    List<Duration> Durations);

  public record struct Duration(int? id, DateOnly Date, TimeSpanValue Value);

  public enum Category
  {
    Today,
    Tomorrow,
    NextWeek,
    Completed,
    Scheduled,
    Archived
  }
}
