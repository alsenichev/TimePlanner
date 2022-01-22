using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.Models.Status.Segments
{
  /// <summary>
  /// Represents the set of time spans within a 24 hours day.
  /// </summary>
  public class DaySegments
  {
    public const int MaxSegmentsCount = 24 * 4;
    private readonly DaySegment remainingTime;
    private readonly DaySegment[] segments;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DaySegments()
    {
      segments = new DaySegment[MaxSegmentsCount];
      remainingTime = DaySegment.FullDay();
    }

    /// <summary>
    /// The undistributed time.
    /// </summary>
    public TimeSpan UndistributedValue => remainingTime.Value;

    /// <summary>
    /// The total distributed time.
    /// </summary>
    public TimeSpan DistributedValue => segments.All(s => s == null)
      ? TimeSpan.Zero
      : segments.Where(s => s != null).Select(s => s.Value).Aggregate((a, v) => a + v);

    /// <summary>
    /// The list of distributed time spans.
    /// </summary>
    public List<TimeSpan> Segments => segments
      .Where(s => s != null).Select(s => s.Value).ToList();

    /// <summary>
    /// Add new empty time segment.
    /// </summary>
    public IResult<int, NoSegmentsAvailable> CreateNewSegment()
    {
      var index = Array.IndexOf(segments, null);
      if (index == -1)
      {
        return Result.Failure<int, NoSegmentsAvailable>(new NoSegmentsAvailable());
      }

      segments[index] = DaySegment.Empty();
      return Result.Success<int, NoSegmentsAvailable>(index);
    }

    /// <summary>
    /// Add the time span to the segment.
    /// </summary>
    public IVoidResult<ISegmentError> AddToSegment(int segmentIndex, TimeSpan duration)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Length || segments[segmentIndex] == null)
      {
        return Result.Failure(new MissingSegment(segmentIndex));
      }

      return segments[segmentIndex].Increase(duration)
        .Bind(_ => remainingTime.Decrease(duration));
    }

    /// <summary>
    /// Subtract the time span from the segment.
    /// </summary>
    public IVoidResult<ISegmentError> RemoveFromSegment(int segmentIndex, TimeSpan duration)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Length || segments[segmentIndex] == null)
      {
        return Result.Failure(new MissingSegment(segmentIndex));
      }

      return segments[segmentIndex].Decrease(duration)
        .Bind(_ => remainingTime.Increase(duration));
    }

    /// <summary>
    /// Get the time span value of the segment.
    /// </summary>
    public IResult<TimeSpan, MissingSegment> GetSegmentValue(int segmentIndex)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Length || segments[segmentIndex] == null)
      {
        return Result.Failure<TimeSpan, MissingSegment>(new MissingSegment(segmentIndex));
      }

      return Result.Success<TimeSpan, MissingSegment>(segments[segmentIndex].Value);
    }

    /// <summary>
    /// Delete the segment and flush its time to the remaining.
    /// </summary>
    public IVoidResult<MissingSegment> RemoveSegmentAt(int segmentIndex)
    {
      if (segmentIndex < 0 || segmentIndex >= segments.Length || segments[segmentIndex] == null)
      {
        return Result.Failure(new MissingSegment(segmentIndex));
      }

      var _ = remainingTime.Increase(segments[segmentIndex].Value);
      segments[segmentIndex] = null;
      return Result.Success<MissingSegment>();
    }
  }
}
