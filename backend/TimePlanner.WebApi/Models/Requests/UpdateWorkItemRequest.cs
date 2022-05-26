using System.Text.Json.Serialization;
using TimePlanner.Domain.Models;

namespace TimePlanner.WebApi.Models.Requests
{
  public class UpdateWorkItemRequest
  {
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Category Category { get; set; }

    public int SortOrder { get; set; }

    public string? CronExpression { get; set; }

    public DateTime? RecurrenceStartsOn { get; set; }

    public DateTime? RecurrenceEndsOn { get; set; }

    public bool? IsAfterPreviousCompleted { get; set; }

    public int? MaxRepetitionsCount { get; set; }

    public bool? IsOnPause { get; set; }
  }
}
