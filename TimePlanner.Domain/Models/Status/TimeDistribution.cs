using TimePlanner.Domain.Models.Status.Segments;
using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.Models.Status
{
  record TimeDistribution
  {
    private readonly DaySegments segments;

    public TimeDistribution()
    {
      segments = new DaySegments();
      segments.CreateNewSegment();
      segments.CreateNewSegment();
    }

    public TimeSpan WorkTime => segments.GetSegmentValue(0).Value;
    public TimeSpan LeisureTime => segments.GetSegmentValue(1).Value;
    public TimeSpan RemainingTime => segments.UndistributedValue;

    public IVoidResult<ISegmentError> AddWorkingTime(TimeSpan duration)
    {
      return segments.AddToSegment(0, duration);
    }

    public IVoidResult<ISegmentError> RemoveWorkingTime(TimeSpan duration)
    {
      return segments.RemoveFromSegment(0, duration);
    }

    public IVoidResult<ISegmentError> AddLeisureTime(TimeSpan duration)
    {
      return segments.AddToSegment(1, duration);
    }

    public IVoidResult<ISegmentError> RemoveLeisureTime(TimeSpan duration)
    {
      return segments.RemoveFromSegment(1, duration);
    }
  }
}
