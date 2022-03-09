namespace TimePlanner.Domain.Core.WorkItemsTracking.Segments
{
  /// <summary>
  /// Represents a time span that is between 0 and 24 hours inclusive.
  /// </summary>
  public record DaySegment
  {
    /// <summary>
    /// Creates a <see cref="DaySegment" /> with the 0 hours value.
    /// </summary>
    public static DaySegment Empty()
    {
      return new DaySegment(TimeSpan.Zero);
    }

    /// <summary>
    /// Creates a <see cref="DaySegment" /> with the 24 hours value.
    /// </summary>
    public static DaySegment FullDay()
    {
      return new DaySegment(twentyFourHours);
    }

    private static readonly TimeSpanValue twentyFourHours = TimeSpan.FromHours(24);

    private DaySegment(TimeSpanValue value)
    {
      Value = value;
    }

    /// <summary>
    /// The time span value of this <see cref="DaySegment" />.
    /// </summary>
    public TimeSpanValue Value { get; private set; }

    /// <summary>
    /// Adds the time span to the current value.
    /// </summary>
    public void Increase(TimeSpanValue timeSpanValue)
    {
      var available = twentyFourHours - Value;
      if (timeSpanValue.Duration > available)
      {
        throw new SegmentOverflowException(available.Duration);
      }

      Value += timeSpanValue.Duration;
    }

    /// <summary>
    /// Subtract the time span from the current value.
    /// </summary>
    /// <remarks>The absolute value is used.</remarks>
    public void Decrease(TimeSpanValue timeSpanValue)
    {
      if (timeSpanValue.Duration > Value)
      {
        throw new SegmentOverflowException(Value.Duration);
      }

      Value -= timeSpanValue.Duration;
    }
  }
}
