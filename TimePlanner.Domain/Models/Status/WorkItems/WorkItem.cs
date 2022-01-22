namespace TimePlanner.Domain.Models.Status.WorkItems
{
  /// <summary>
  /// Work item object with the basic information.
  /// </summary>
  public record InnerWorkItem(string Name);

  /// <summary>
  /// Work item with expanded information.
  /// </summary>
  public record WorkItem(string Name, TimeSpan Duration) : InnerWorkItem(Name);
}
