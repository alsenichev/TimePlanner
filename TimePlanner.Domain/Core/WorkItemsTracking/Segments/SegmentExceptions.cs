namespace TimePlanner.Domain.Core.WorkItemsTracking.Segments
{
  /// <summary>
  /// Indicates that the time span is outside the acceptable value.
  /// </summary>
  public class SegmentOverflowException : ApplicationException
  {
    public SegmentOverflowException(TimeSpanValue acceptableValue)
    {
      AcceptableValue = acceptableValue;
    }

    public TimeSpanValue AcceptableValue { get; }
  }

  /// <summary>
  /// Indicates that the requested segment does not exist.
  /// </summary>
  public class MissingSegmentException : ApplicationException
  {
    public MissingSegmentException(int index)
    {
      Index = index;
    }

    public int Index { get; }
  }


  /// <summary>
  /// Indicates that there are no more segments available.
  /// </summary>
  public class NoSegmentsAvailableException : ApplicationException
  {
  }
}
