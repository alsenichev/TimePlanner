namespace TimePlanner.Domain.Models
{
  public record struct Recurrence(
    Guid WorkItemId,
    DateTime? RepetitionStartDate,
    string Cron,
    int? RepetitionCount,
    int? MaxRepetitionCount,
    bool? IsAfterPreviousCompleted);
}
