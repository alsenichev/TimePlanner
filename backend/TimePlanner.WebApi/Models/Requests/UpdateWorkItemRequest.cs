namespace TimePlanner.WebApi.Models.Requests
{
  public record UpdateWorkItemRequest(
    Guid Id,
    string Name,
    string Category,
    string Recurrence,
    int SortOrder);
}
