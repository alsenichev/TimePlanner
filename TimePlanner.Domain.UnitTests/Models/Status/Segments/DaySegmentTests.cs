using NUnit.Framework;
using TimePlanner.Domain.Models.Status.Segments;

namespace TimePlanner.Domain.UnitTests.Models.Status.Segments
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
      Assert.AreEqual(TimeSpan.Zero, result.Value);
    }

    [Test]
    public void TestCreateFull()
    {
      var result = DaySegment.FullDay();
      Assert.AreEqual(TimeSpan.FromHours(24), result.Value);
    }

    [TestCase(0)]
    [TestCase(24)]
    [TestCase(12)]
    [TestCase(-12)]
    public void TestIncreaseValidValue(int hours)
    {
      var timeSpan = TimeSpan.FromHours(hours);
      var segment = DaySegment.Empty();
      var result = segment.Increase(timeSpan);

      Assert.IsTrue(result.IsSuccess);
      Assert.True(segment.Value >= TimeSpan.Zero);
      Assert.AreEqual(timeSpan.Duration(), segment.Value);
    }

    [TestCase(5)]
    [TestCase(-16)]
    public void TestIncreaseInvalidValue(int hours)
    {
      var timeSpan = TimeSpan.FromHours(hours);
      var segment = DaySegment.Empty();
      var _ = segment.Increase(TimeSpan.FromHours(22));

      var result = segment.Increase(timeSpan);

      Assert.IsFalse(result.IsSuccess);
      Assert.AreEqual(TimeSpan.FromHours(2), result.Error.AcceptableValue);
    }

    [TestCase(0)]
    [TestCase(24)]
    [TestCase(12)]
    [TestCase(-12)]
    public void TestDecreaseValidValue(int hours)
    {
      var timeSpan = TimeSpan.FromHours(hours);
      var segment = DaySegment.FullDay();
      var result = segment.Decrease(timeSpan);

      Assert.IsTrue(result.IsSuccess);
      Assert.AreEqual(TimeSpan.FromHours(24) - timeSpan.Duration(), segment.Value);
    }

    [TestCase(5)]
    [TestCase(-16)]
    public void TestDecreaseInvalidValue(int hours)
    {
      var timeSpan = TimeSpan.FromHours(hours);
      var segment = DaySegment.Empty();
      var _ = segment.Increase(TimeSpan.FromHours(4));

      var result = segment.Decrease(timeSpan);

      Assert.IsFalse(result.IsSuccess);
      Assert.AreEqual(TimeSpan.FromHours(4), result.Error.AcceptableValue);
    }
  }
}
