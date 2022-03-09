using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using TimePlanner.Domain.Core.WorkItemsTracking;

namespace TimePlanner.Domain.UnitTests.Status
{
  /// <summary>
  /// Tests for <see cref="StatusBuilder" />.
  /// </summary>
  public class WorkingDayTests
  {
    private Fixture fixture;

    /// <summary>
    /// Test setup.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
      fixture = new Fixture();
      fixture.Register(() => new DateOnly(2022, 5, 16));
      fixture.Register(() => TimeSpan.FromMinutes(new Random().Next(1, 720)));
      fixture.Customize(new AutoMoqCustomization());
    }

    ///// <summary>
    ///// Test creation of the working day.
    ///// </summary>
    //[Theory]
    //[AutoData]
    //public void TestCreateNewDay(DateTime creationTime, TimeSpan deposit)
    //{
    //  var actual = WorkingDay.Start(creationTime, deposit);

    //  Assert.NotNull(actual.WorkItems);
    //  Assert.AreEqual(creationTime, actual.StartedAt);
    //  Assert.GreaterOrEqual(actual.DistributedWorkingTime, actual.UndistributedWorkingTime);
    //  Assert.AreEqual(deposit, actual.Deposit);
    //  Assert.AreEqual(TimeSpan.Zero, actual.Pause);
    //}

    ///// <summary>
    ///// Test adding a value to the pause.
    ///// </summary>
    //[Test]
    //public void TestAddToPause()
    //{
    //  var addition = TimeSpan.FromMinutes(15);
    //  var workingDay = fixture.Create<WorkingDay>();
    //  var existingPause = workingDay.Pause;
    //  var result = workingDay.RegisterLeisureTime(addition);
    //  if (result.IsSuccess)
    //  {
    //    Assert.AreEqual(existingPause + addition, workingDay.Pause);
    //  }
    //  else
    //  {
    //    StringAssert.Contains("Only", result.Messages.First());
    //  }
    //}

    //[Test]
    //public void TestFixPause()
    //{
    //  var subtraction = -TimeSpan.FromMinutes(15);
    //  var workingDay = fixture.Create<WorkingDay>();
    //  var existingPause = workingDay.Pause;
    //  var result = workingDay.RegisterLeisureTime(subtraction);
    //  if (result.IsSuccess)
    //  {
    //    Assert.AreEqual(existingPause - subtraction, workingDay.Pause);
    //  }
    //  else
    //  {
    //    StringAssert.Contains("Only", result.Messages.First());
    //  }
    //}

    //[Test]
    //public void TestAddToWorkingTime()
    //{
    //  var addition = TimeSpan.FromMinutes(15);
    //  var workingDay = fixture.Create<WorkingDay>();
    //  var existingTime = workingDay.WorkTime;
    //  var result = workingDay.RegisterWorkingTime(addition);
    //  if (result.IsSuccess)
    //  {
    //    Assert.AreEqual(existingTime + addition, workingDay.WorkTime);
    //  }
    //  else
    //  {
    //    StringAssert.Contains("Only", result.Messages.First());
    //  }
    //}

    //[Test]
    //public void TestFixWorkingTime()
    //{
    //  var subtraction = -TimeSpan.FromMinutes(15);
    //  var workingDay = fixture.Create<WorkingDay>();
    //  var existingTime = workingDay.WorkTime;
    //  var result = workingDay.RegisterWorkingTime(subtraction);
    //  if (result.IsSuccess)
    //  {
    //    Assert.AreEqual(existingTime - subtraction, workingDay.WorkTime);
    //  }
    //  else
    //  {
    //    StringAssert.Contains("Only", result.Messages.First());
    //  }
    //}

    //[Test]
    //public void TestAddToTaskDuration()
    //{
    //  var addition = TimeSpan.FromMinutes(30);
    //  fixture.RepeatCount = 3;
    //  var workingDay = fixture.Create<WorkingDay>();
    //  var existingWorkingTime = workingDay.DistributedWorkingTime;

    //  var (isSuccess, messages) = workingDay.DistributeWorkingTime(2, addition);

    //  if (isSuccess)
    //  {
    //    Assert.AreEqual(existingWorkingTime + addition, workingDay.DistributedWorkingTime);
    //  }
    //  else
    //  {
    //    StringAssert.Contains("nly", messages.First());
    //  }
    //}

    //[Test]
    //public void TestAddToNonExistingTaskDuration()
    //{
    //  fixture.RepeatCount = 3;
    //  var day = fixture.Create<WorkingDay>();

    //  var actual = day.DistributeWorkingTime(3, TimeSpan.FromHours(1));

    //  Assert.IsFalse(actual.IsSuccess);
    //  StringAssert.Contains("does not exist", actual.Messages.First());
    //}

    //[Test]
    //public void TestFixStartTime()
    //{
    //  var day = fixture.Create<WorkingDay>();
    //  var existingStartedAt = day.StartedAt;
    //  var actual = day.FixStartedAt(existingStartedAt.AddHours(1));
    //  if (actual.IsSuccess)
    //  {
    //    Assert.AreEqual(existingStartedAt.AddHours(1), day.StartedAt);
    //  }
    //  else
    //  {
    //    StringAssert.Contains("Only", actual.Messages.First());
    //  }
    //}
  }
}
