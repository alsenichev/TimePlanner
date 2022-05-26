namespace TimePlanner.Domain.Interfaces
{
  /// <summary>
  /// Hides the current date time
  /// </summary>
  public interface ITimeProvider
  {
    /// <summary>
    /// Get the current DateTime value.
    /// </summary>
    public DateTime Now { get; }
  }
}
