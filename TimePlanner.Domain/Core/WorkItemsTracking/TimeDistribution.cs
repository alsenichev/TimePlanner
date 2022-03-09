using TimePlanner.Domain.Core.WorkItemsTracking.Segments;

namespace TimePlanner.Domain.Core.WorkItemsTracking
{
  /// <summary>
  /// Describes how the time during the day is distributed.
  /// </summary>
  internal record TimeDistribution
  {
    private readonly DaySegments segments;

    public TimeDistribution()
    {
      segments = new DaySegments(2);
      segments.CreateNewSegment();
      segments.CreateNewSegment();
    }

    /// <summary>
    /// Working time of the day.
    /// </summary>
    public TimeSpanValue WorkTime => segments.GetSegmentValue(0);

    /// <summary>
    /// Leisure time of the day.
    /// </summary>
    public TimeSpanValue LeisureTime => segments.GetSegmentValue(1);

    /// <summary>
    /// The time of the day that is not counted yet.
    /// </summary>
    public TimeSpanValue RemainingTime => segments.UndistributedValue;

    public void AddWorkingTime(TimeSpanValue duration)
    {
      segments.AddToSegment(0, duration);
    }

    public void RemoveWorkingTime(TimeSpanValue duration)
    {
      segments.RemoveFromSegment(0, duration);
    }

    public void AddLeisureTime(TimeSpanValue duration)
    {
      segments.AddToSegment(1, duration);
    }

    public void RemoveLeisureTime(TimeSpan duration)
    {
      segments.RemoveFromSegment(1, duration);
    }
  }
}
