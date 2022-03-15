namespace TimePlanner.Domain.Core.WorkItemsTracking.Segments
{
  /// <summary>
  /// Represents the set of time spans within a 24 hours day.
  /// </summary>
  public class DaySegments
  {
    private readonly int maxSegmentsCount;
    private readonly DaySegment remainingTime;
    private readonly List<DaySegment> segments;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DaySegments(int maxSegmentsCount)
    {
      this.maxSegmentsCount = maxSegmentsCount;
      segments = new List<DaySegment>(maxSegmentsCount);
      remainingTime = DaySegment.FullDay();
    }

    /// <summary>
    /// The undistributed time.
    /// </summary>
    public TimeSpanValue UndistributedValue => remainingTime.Value;

    /// <summary>
    /// The total distributed time.
    /// </summary>
    public TimeSpanValue DistributedValue => segments.Count == 0
      ? TimeSpan.Zero
      : segments.Select(s => s.Value).Aggregate((a, v) => a + v);

    /// <summary>
    /// The list of distributed time spans.
    /// </summary>
    public List<TimeSpanValue> Segments => segments.Select(s => s.Value).ToList();

    /// <summary>
    /// Add new empty time segment.
    /// </summary>
    public int CreateNewSegment()
    {
      if (segments.Count == maxSegmentsCount)
      {
        throw new NoSegmentsAvailableException();
      }

      segments.Add(DaySegment.Empty());
      return segments.Count - 1;
    }

    /// <summary>
    /// Add the time span to the segment.
    /// </summary>
    public void AddToSegment(int segmentIndex, TimeSpanValue duration)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Count)
      {
        throw new MissingSegmentException(segmentIndex);
      }

      segments[segmentIndex].Increase(duration);
      remainingTime.Decrease(duration);
    }

    /// <summary>
    /// Set new value of the segment.
    /// </summary>
    public void ResetSegment(int segmentIndex, TimeSpanValue duration)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Count)
      {
        throw new MissingSegmentException(segmentIndex);
      }

      TimeSpanValue currentValue = segments[segmentIndex].Value;
      remainingTime.Increase(currentValue);
      segments[segmentIndex].Decrease(currentValue);
      AddToSegment(segmentIndex, duration);
    }

    /// <summary>
    /// Subtract the time span from the segment.
    /// </summary>
    public void RemoveFromSegment(int segmentIndex, TimeSpanValue duration)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Count)
      {
        throw new MissingSegmentException(segmentIndex);
      }

      segments[segmentIndex].Decrease(duration);
      remainingTime.Increase(duration);
    }

    /// <summary>
    /// Get the time span value of the segment.
    /// </summary>
    public TimeSpanValue GetSegmentValue(int segmentIndex)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Count)
      {
        throw new MissingSegmentException(segmentIndex);
      }

      return segments[segmentIndex].Value;
    }

    /// <summary>
    /// Delete the segment and flush its time to the remaining.
    /// </summary>
    public TimeSpanValue RemoveSegmentAt(int segmentIndex)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Count)
      {
        throw new MissingSegmentException(segmentIndex);
      }

      TimeSpanValue result = segments[segmentIndex].Value;
      remainingTime.Increase(result);
      segments.RemoveAt(segmentIndex);
      return result;
    }
  }
}
