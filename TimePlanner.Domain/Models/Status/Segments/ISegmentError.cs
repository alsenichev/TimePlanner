namespace TimePlanner.Domain.Models.Status.Segments
{
  /// <summary>
  /// Errors specific to <see cref="DaySegment"/> and <see cref="DaySegments"/>.
  /// </summary>
  public interface ISegmentError
  {
    /// <summary>
    /// Convert to another error type.
    /// </summary>
    T Convert<T>(
      Func<Overflow, T> funcOne,
      Func<MissingSegment, T> funcTwo,
      Func<NoSegmentsAvailable, T> funcThree);
  }

  /// <summary>
  /// Indicates that the time span is outside the acceptable value.
  /// </summary>
  /// <param name="AcceptableValue">The value that can be accepted.</param>
  public record Overflow(TimeSpan AcceptableValue) : ISegmentError
  {
    /// <inheritdoc />
    T ISegmentError.Convert<T>(
      Func<Overflow, T> funcOne,
      Func<MissingSegment, T> funcTwo,
      Func<NoSegmentsAvailable, T> funcThree)
    {
      return funcOne(this);
    }
  }

  /// <summary>
  /// Indicates that the requested segment does not exist.
  /// </summary>
  /// <param name="Index">The segment's index.</param>
  public record MissingSegment(int Index) : ISegmentError
  {
    /// <inheritdoc />
    T ISegmentError.Convert<T>(
      Func<Overflow, T> funcOne,
      Func<MissingSegment, T> funcTwo,
      Func<NoSegmentsAvailable, T> funcThree)
    {
      return funcTwo(this);
    }
  }

  /// <summary>
  /// Indicates that there are no more segments available.
  /// </summary>
  public record NoSegmentsAvailable : ISegmentError
  {
    /// <inheritdoc />
    T ISegmentError.Convert<T>(
      Func<Overflow, T> funcOne,
      Func<MissingSegment, T> funcTwo,
      Func<NoSegmentsAvailable, T> funcThree)
    {
      return funcThree(this);
    }
  }
}
