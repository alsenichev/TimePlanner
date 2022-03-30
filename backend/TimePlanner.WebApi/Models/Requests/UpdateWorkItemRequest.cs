namespace TimePlanner.WebApi.Models.Requests
{
  public class UpdateWorkItemRequest
  {
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Recurrence { get; set; }
    public int SortOrder { get; set; }
  }
}
