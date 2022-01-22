using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.Models.Status.Segments
{
  /// <summary>
  /// Represents a time span that is between 0 and 24 hours inclusive.
  /// </summary>
  public record DaySegment
  {
    /// <summary>
    /// Creates a <see cref="DaySegment"/> with the 0 hours value.
    /// </summary>
    public static DaySegment Empty()
    {
      return new DaySegment(TimeSpan.Zero);
    }

    /// <summary>
    /// Creates a <see cref="DaySegment"/> with the 24 hours value.
    /// </summary>
    public static DaySegment FullDay()
    {
      return new DaySegment(twentyFourHours);
    }

    private static readonly TimeSpan twentyFourHours = TimeSpan.FromHours(24);

    private DaySegment(TimeSpan value)
    {
      Value = value;
    }

    /// <summary>
    /// The time span value of this <see cref="DaySegment"/>.
    /// </summary>
    public TimeSpan Value { get; private set; }

    /// <summary>
    /// Adds the time span to the current value.
    /// </summary>
    /// <remarks>The absolute value is used.</remarks>
    public IVoidResult<Overflow> Increase(TimeSpan absValue)
    {
      var available = twentyFourHours - Value;
      if (absValue.Duration() > available)
      {
        return Result.Failure(new Overflow(available));
      }

      Value += absValue.Duration();
      return Result.Success<Overflow>();
    }

    /// <summary>
    /// Subtract the time span from the current value.
    /// </summary>
    /// <remarks>The absolute value is used.</remarks>
    public IVoidResult<Overflow> Decrease(TimeSpan absValue)
    {
      if (absValue.Duration() > Value)
      {
        return Result.Failure(new Overflow(Value));
      }

      Value -= absValue.Duration();
      return Result.Success<Overflow>();
    }
  }
}
