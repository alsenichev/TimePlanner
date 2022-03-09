namespace TimePlanner.Domain.Core.WorkItemsTracking.Segments
{
  public record struct TimeSpanValue
  {
    public TimeSpanValue(TimeSpan timeSpan)
    {
      Duration = timeSpan.Duration();
    }

    public TimeSpan Duration { get; }

    public static implicit operator TimeSpanValue(TimeSpan timeSpan)
    {
      return new TimeSpanValue(timeSpan);
    }

    public static explicit operator TimeSpan(TimeSpanValue timeSpanValue)
    {
      return timeSpanValue.Duration;
    }

    public static TimeSpanValue operator -(TimeSpanValue one, TimeSpanValue two)
    {
      return one.Duration - two.Duration;
    }

    public static TimeSpanValue operator +(TimeSpanValue one, TimeSpanValue two)
    {
      return one.Duration + two.Duration;
    }

    public static bool operator >(TimeSpanValue one, TimeSpanValue two)
    {
      return one.Duration > two.Duration;
    }

    public static bool operator <(TimeSpanValue one, TimeSpanValue two)
    {
      return one.Duration < two.Duration;
    }

  }
}
