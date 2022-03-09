using AutoFixture;
using AutoFixture.NUnit3;
using NUnit.Framework;
using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.UnitTests.Status.Segments;

/// <summary>
/// Tests for <see cref="DaySegments" />
/// </summary>
public class DaySegmentsTests
{
  private static readonly TimeSpan twentyFour = TimeSpan.FromHours(24);
  private const int defaultMaxSegmentsCount = 24 * 4;

  private Fixture fixture;

  [SetUp]
  public void SetUp()
  {
    fixture = new Fixture();
    fixture.Register(() => TimeSpan.FromMinutes(new Random().Next(0, 1440)));
  }

  [Test]
  public void TestCreate()
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);

    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue.Duration);
    Assert.AreEqual(twentyFour, segments.UndistributedValue.Duration);
    CollectionAssert.IsEmpty(segments.Segments);
  }

  [Test]
  public void TestCreateNewSegment()
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);

    int result = segments.CreateNewSegment();

    Assert.AreEqual(0, result);
    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue.Duration);
    Assert.AreEqual(twentyFour, segments.UndistributedValue.Duration);
    Assert.AreEqual(1, segments.Segments.Count);
    Assert.AreEqual(TimeSpan.Zero, segments.Segments[0].Duration);
  }

  [Theory]
  [AutoData]
  public void TestAddTimeToExistingSegment(TimeSpan timeSpan)
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);

    int segmentIndex= segments.CreateNewSegment();
    segments.AddToSegment(0, timeSpan);

    Assert.AreEqual(0, segmentIndex);
    Assert.AreEqual(timeSpan, segments.DistributedValue.Duration);
    Assert.AreEqual(twentyFour - segments.DistributedValue, segments.UndistributedValue);
    Assert.AreEqual(1, segments.Segments.Count);
    Assert.AreEqual(timeSpan, segments.Segments[0].Duration);
  }

  [Test]
  public void TestTryAddTimeToNonExistingSegment()
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);

    segments.CreateNewSegment();
    var exception = Assert.Throws<MissingSegmentException>(
      () => segments.AddToSegment(1, fixture.Create<TimeSpan>()));

    Assert.AreEqual(1, exception.Index);
    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue.Duration);
    Assert.AreEqual(twentyFour, segments.UndistributedValue.Duration);
    Assert.AreEqual(1, segments.Segments.Count);
    Assert.AreEqual(TimeSpan.Zero, segments.Segments[0].Duration);
  }

  [Test]
  public void TestCreateNewSegmentFailed()
  {
    const int maxSegments = 3;
    var segments = new DaySegments(maxSegments);
    segments.CreateNewSegment();
    segments.CreateNewSegment();
    segments.CreateNewSegment();

    var exception = Assert.Throws<NoSegmentsAvailableException>(
      () => segments.CreateNewSegment());

    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue.Duration);
    Assert.AreEqual(twentyFour, segments.UndistributedValue.Duration);
    Assert.AreEqual(maxSegments, segments.Segments.Count);
  }

  [Test]
  public void TestGetSegmentValue()
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);
    TimeSpan timeSpan = fixture.Create<TimeSpan>();
    segments.CreateNewSegment();
    segments.AddToSegment(0, timeSpan);
    TimeSpanValue result = segments.GetSegmentValue(0);

    Assert.AreEqual(timeSpan.Duration(), result.Duration);
  }

  [Test]
  public void TestTryGetNonExistingSegmentValue()
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);
    segments.CreateNewSegment();

    var exception = Assert.Throws<MissingSegmentException>(
      () => segments.GetSegmentValue(1));

    Assert.AreEqual(1,exception.Index);
  }

  [Theory]
  [AutoData]
  public void TestRemoveFromSegment(TimeSpan timeSpan)
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);

    segments.CreateNewSegment();
    segments.AddToSegment(0, timeSpan);
    segments.RemoveFromSegment(0, timeSpan);

    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue.Duration);
    Assert.AreEqual(twentyFour - segments.DistributedValue, segments.UndistributedValue);
    Assert.AreEqual(1, segments.Segments.Count);
    Assert.AreEqual(TimeSpan.Zero, segments.Segments[0].Duration);
  }

  [Theory]
  [AutoData]
  public void TestRemoveTooMuchFromSegment(TimeSpan timeSpan)
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);

    segments.CreateNewSegment();
    segments.AddToSegment(0, timeSpan);
    
    var exception = Assert.Throws<SegmentOverflowException>(
      () =>  segments.RemoveFromSegment(0, timeSpan + TimeSpan.FromMinutes(1)));

    Assert.AreEqual(timeSpan, exception?.AcceptableValue.Duration);
  }

  [Theory]
  [AutoData]
  public void TestTryRemoveFromNonExistingSegment(TimeSpan timeSpan)
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);
    segments.CreateNewSegment();
    var exception = Assert.Throws<MissingSegmentException>(
      () => segments.RemoveFromSegment(1, timeSpan));

    Assert.AreEqual(1,exception?.Index);
  }

  [Test]
  public void TestRemoveSegment()
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);

    segments.CreateNewSegment();
    segments.RemoveSegmentAt(0);

    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue.Duration);
    Assert.AreEqual(twentyFour, segments.UndistributedValue.Duration);
    Assert.AreEqual(0, segments.Segments.Count);
  }

  [Test]
  public void TestTryRemoveNonExistingSegment()
  {
    var segments = new DaySegments(defaultMaxSegmentsCount);

    segments.CreateNewSegment();

    var exception = Assert.Throws<MissingSegmentException>(
      () => segments.RemoveSegmentAt(1));

    Assert.AreEqual(1, exception?.Index);
  }

  [Test]
  public void TestRemoveAndAddMiddleSegment()
  {
    var segments = new DaySegments(5);

    segments.CreateNewSegment();
    segments.AddToSegment(0, TimeSpan.FromMinutes(1));
    segments.CreateNewSegment();
    segments.AddToSegment(1, TimeSpan.FromMinutes(2));
    segments.RemoveSegmentAt(0);
    segments.CreateNewSegment();
    segments.AddToSegment(1, TimeSpan.FromMinutes(3));

    Assert.AreEqual(TimeSpan.FromMinutes(5), segments.DistributedValue.Duration);
    Assert.AreEqual(2, segments.Segments.Count);
    // when we remove a segments, higher segments shift to fill the gap
    Assert.AreEqual(TimeSpan.FromMinutes(2), segments.Segments[0].Duration);
    Assert.AreEqual(TimeSpan.FromMinutes(3), segments.Segments[1].Duration);
  }
}
