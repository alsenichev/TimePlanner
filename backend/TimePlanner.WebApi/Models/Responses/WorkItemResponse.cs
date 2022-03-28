using System.Text.Json.Serialization;
using TimePlanner.Domain.Models;

namespace TimePlanner.WebApi.Models.Responses
{
  public class WorkItemResponse
  {
    public Guid Id { get; set; }

    public string Name { get; set; }

    public List<DurationResponse> Durations { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Category Category { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? RecurrenceDays { get; set; }

    public int SortOrder { get; set; }

    public DateTime? NextTime { get; set; }
  }

  public record struct DurationResponse(int id, string date, string Value);

}
