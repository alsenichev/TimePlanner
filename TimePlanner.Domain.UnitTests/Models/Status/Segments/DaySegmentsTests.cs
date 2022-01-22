using AutoFixture;
using AutoFixture.NUnit3;
using NUnit.Framework;
using TimePlanner.Domain.Models.Status.Segments;
using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.UnitTests.Models.Status.Segments;

/// <summary>
/// Tests for <see cref="DaySegments" />
/// </summary>
public class DaySegmentsTests
{
  private static readonly TimeSpan twentyFour = TimeSpan.FromHours(24);

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
    var segments = new DaySegments();

    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue);
    Assert.AreEqual(twentyFour, segments.UndistributedValue);
    CollectionAssert.IsEmpty(segments.Segments);
  }

  [Test]
  public void TestCreateNewSegment()
  {
    var segments = new DaySegments();

    IResult<int, NoSegmentsAvailable> result = segments.CreateNewSegment();

    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual(0, result.Value);
    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue);
    Assert.AreEqual(twentyFour, segments.UndistributedValue);
    Assert.AreEqual(1, segments.Segments.Count);
    Assert.AreEqual(TimeSpan.Zero, segments.Segments[0]);
  }

  [Theory]
  [AutoData]
  public void TestAddTimeToExistingSegment(TimeSpan timeSpan)
  {
    var segments = new DaySegments();

    var _ = segments.CreateNewSegment();
    var result = segments.AddToSegment(0, timeSpan);

    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual(timeSpan, segments.DistributedValue);
    Assert.AreEqual(twentyFour - segments.DistributedValue, segments.UndistributedValue);
    Assert.AreEqual(1, segments.Segments.Count);
    Assert.AreEqual(timeSpan, segments.Segments[0]);
  }

  [Test]
  public void TestTryAddTimeToNonExistingSegment()
  {
    var segments = new DaySegments();

    segments.CreateNewSegment();
    var result = segments.AddToSegment(1, fixture.Create<TimeSpan>());

    Assert.IsFalse(result.IsSuccess);
    Assert.IsInstanceOf<MissingSegment>(result.Error);
    Assert.AreEqual(1, ((MissingSegment)result.Error).Index);
    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue);
    Assert.AreEqual(twentyFour, segments.UndistributedValue);
    Assert.AreEqual(1, segments.Segments.Count);
    Assert.AreEqual(TimeSpan.Zero, segments.Segments[0]);
  }

  [Test]
  public void TestCreateNewSegmentFailed()
  {
    var segments = new DaySegments();
    for (var i = 0; i < 24 * 4; i++)
    {
      segments.CreateNewSegment();
    }

    var result = segments.CreateNewSegment();

    Assert.IsFalse(result.IsSuccess);
    Assert.IsInstanceOf<NoSegmentsAvailable>(result.Error);
    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue);
    Assert.AreEqual(twentyFour, segments.UndistributedValue);
    Assert.AreEqual(24 * 4, segments.Segments.Count);
  }

  [Test]
  public void TestGetSegmentValue()
  {
    var segments = new DaySegments();
    TimeSpan timeSpan = fixture.Create<TimeSpan>();
    segments.CreateNewSegment();
    segments.AddToSegment(0, timeSpan);
    IResult<TimeSpan, MissingSegment> result = segments.GetSegmentValue(0);

    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual(timeSpan, result.Value);
  }

  [Test]
  public void TestTryGetNonExistingSegmentValue()
  {
    var segments = new DaySegments();
    segments.CreateNewSegment();
    IResult<TimeSpan, MissingSegment> result = segments.GetSegmentValue(1);

    Assert.IsFalse(result.IsSuccess);
    Assert.IsInstanceOf<MissingSegment>(result.Error);
    Assert.AreEqual(1, ((MissingSegment)result.Error).Index);
  }

  [Theory]
  [AutoData]
  public void TestRemoveFromSegment(TimeSpan timeSpan)
  {
    var segments = new DaySegments();

    segments.CreateNewSegment();
    segments.AddToSegment(0, timeSpan);
    IVoidResult<ISegmentError> result = segments.RemoveFromSegment(0, timeSpan);

    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue);
    Assert.AreEqual(twentyFour - segments.DistributedValue, segments.UndistributedValue);
    Assert.AreEqual(1, segments.Segments.Count);
    Assert.AreEqual(TimeSpan.Zero, segments.Segments[0]);
  }

  [Theory]
  [AutoData]
  public void TestRemoveTooMuchFromSegment(TimeSpan timeSpan)
  {
    var segments = new DaySegments();

    segments.CreateNewSegment();
    segments.AddToSegment(0, timeSpan);
    var result = segments.RemoveFromSegment(0, timeSpan + TimeSpan.FromMinutes(1));

    Assert.IsFalse(result.IsSuccess);
    Assert.IsInstanceOf<Overflow>(result.Error);
    Assert.AreEqual(timeSpan, ((Overflow)result.Error).AcceptableValue);
  }

  [Theory]
  [AutoData]
  public void TestTryRemoveFromNonExistingSegment(TimeSpan timeSpan)
  {
    var segments = new DaySegments();
    segments.CreateNewSegment();
    IVoidResult<ISegmentError> result = segments.RemoveFromSegment(1, timeSpan);

    Assert.IsFalse(result.IsSuccess);
    Assert.IsInstanceOf<MissingSegment>(result.Error);
    Assert.AreEqual(1, ((MissingSegment)result.Error).Index);
  }

  [Test]
  public void TestRemoveSegment()
  {
    var segments = new DaySegments();

    segments.CreateNewSegment();
    var result = segments.RemoveSegmentAt(0);

    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual(TimeSpan.Zero, segments.DistributedValue);
    Assert.AreEqual(twentyFour, segments.UndistributedValue);
    Assert.AreEqual(0, segments.Segments.Count);
  }

  [Test]
  public void TestTryRemoveNonExistingSegment()
  {
    var segments = new DaySegments();

    segments.CreateNewSegment();

    IVoidResult<MissingSegment> result = segments.RemoveSegmentAt(1);

    Assert.IsFalse(result.IsSuccess);
    Assert.IsInstanceOf<MissingSegment>(result.Error);
    Assert.AreEqual(1, ((MissingSegment)result.Error).Index);
  }
}
