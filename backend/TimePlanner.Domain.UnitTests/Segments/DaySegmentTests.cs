using NUnit.Framework;
using TimePlanner.Domain.Exceptions;
using TimePlanner.Domain.WorkItemsTracking.Segments;

namespace TimePlanner.Domain.UnitTests.Segments
{
  /// <summary>
  /// Tests for <see cref="DaySegment" />
  /// </summary>
  public class DaySegmentTests
  {
    [Test]
    public void TestCreateEmpty()
    {
      var result = DaySegment.Empty();
      Assert.AreEqual(TimeSpan.Zero, result.Value.Duration);
    }

    [Test]
    public void TestCreateFull()
    {
      var result = DaySegment.FullDay();
      Assert.AreEqual(TimeSpan.FromHours(24), result.Value.Duration);
    }

    [TestCase(0)]
    [TestCase(24)]
    [TestCase(12)]
    [TestCase(-12)]
    public void TestIncreaseValidValue(int hours)
    {
      var timeSpan = TimeSpan.FromHours(hours);
      var segment = DaySegment.Empty();
      segment.Increase(timeSpan);

      Assert.True(segment.Value.Duration >= TimeSpan.Zero);
      Assert.AreEqual(timeSpan.Duration(), segment.Value.Duration);
    }

    [TestCase(5)]
    [TestCase(-16)]
    public void TestIncreaseInvalidValue(int hours)
    {
      var timeSpan = TimeSpan.FromHours(hours);
      var segment = DaySegment.Empty();
      segment.Increase(TimeSpan.FromHours(22));

      var exception = Assert.Throws<SegmentOverflowException>(
        () => segment.Increase(timeSpan));

      Assert.AreEqual(TimeSpan.FromHours(2), exception?.AcceptableValue.Duration);
    }

    [TestCase(0)]
    [TestCase(24)]
    [TestCase(12)]
    [TestCase(-12)]
    public void TestDecreaseValidValue(int hours)
    {
      var timeSpan = TimeSpan.FromHours(hours);
      var segment = DaySegment.FullDay();
      segment.Decrease(timeSpan);

      Assert.AreEqual(TimeSpan.FromHours(24) - timeSpan.Duration(), segment.Value.Duration);
    }

    [TestCase(5)]
    [TestCase(-16)]
    public void TestDecreaseInvalidValue(int hours)
    {
      var timeSpan = TimeSpan.FromHours(hours);
      var segment = DaySegment.Empty();
      segment.Increase(TimeSpan.FromHours(4));

      var exception = Assert.Throws<SegmentOverflowException>(
        () => segment.Decrease(timeSpan));

      Assert.AreEqual(TimeSpan.FromHours(4), exception?.AcceptableValue.Duration);
    }
  }
}
