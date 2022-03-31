namespace TimePlanner.WebApi.Models.Requests
{
  public class UpdateWorkItemRequest
  {
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool UpdateRecurrence { get; set; } // TODO POCO: extract into separate endpoints.
    public string? CronExpression { get; set; }
    public DateTime? RecurrenceStartsFrom { get; set; }
    public bool? IsAfterPreviousCompleted { get; set; }
    public int? MaxRepetitionsCount { get; set; }
  }
}
