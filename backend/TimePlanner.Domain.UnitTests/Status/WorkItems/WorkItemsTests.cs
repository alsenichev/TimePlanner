using AutoFixture;
using NUnit.Framework;
using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Core.WorkItemsTracking.WorkItems;

namespace TimePlanner.Domain.UnitTests.Status.WorkItems
{
  /// <summary>
  /// Tests for <see cref="WorkItemList" />.
  /// </summary>
  public class WorkItemsTests
  {
    private Fixture fixture;

    [SetUp]
    public void SetUp()
    {
      fixture = new Fixture();
      fixture.Register(() => TimeSpan.FromMinutes(new Random().Next(15, 60)));
    }


    [Test]
    public void TestCreateWorkItemsList()
    {
      var workItemList = new WorkItemList();

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(0, workItemList.WorkItems.Count);
    }

    [Test]
    public void TestAddWorkItem()
    {
      var workItemName = fixture.Create<string>();
      var workItemList = new WorkItemList();

      workItemList.CreateWorkItem(workItemName);

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(1, workItemList.WorkItems.Count);
      Assert.AreEqual(TimeSpan.Zero, workItemList.WorkItems.First().Duration.Duration);
      Assert.AreEqual(workItemName, workItemList.WorkItems.First().Name);
      Assert.IsNull(workItemList.WorkItems.First().Id);
    }

    [Test]
    public void TestAddWorkItemOverflow()
    {
      var workItemName = fixture.Create<string>();
      var workItemList = new WorkItemList();
      for (var i = 0; i < 24 * 4; i++)
      {
        workItemList.CreateWorkItem(fixture.Create<string>());
      }

      var exception = Assert.Throws<NoSegmentsAvailableException>(
        () => workItemList.CreateWorkItem(workItemName));

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(24 * 4, workItemList.WorkItems.Count);
    }

    [Test]
    public void TestRemoveWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      workItemList.AddWorkItem(workItem);
      workItemList.RemoveWorkItem(workItem.Id.Value);

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(0, workItemList.WorkItems.Count);
    }

    [Test]
    public void TestTryRemovingNonExistingWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      workItemList.AddWorkItem(workItem);

      Assert.Throws<MissingSegmentException>(
        () => workItemList.RemoveWorkItem(Guid.Empty));
    }

    [Test]
    public void TestRemoveMiddleWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItemOne = fixture.Create<WorkItem>();
      var workItemTwo = fixture.Create<WorkItem>();
      var workItemThree = fixture.Create<WorkItem>();
      workItemList.AddWorkItem(workItemOne);
      workItemList.AddWorkItem(workItemTwo);
      workItemList.RemoveWorkItem(workItemOne.Id.Value);
      workItemList.AddWorkItem(workItemThree);

      Assert.AreEqual(workItemTwo.Duration + workItemThree.Duration, workItemList.DistributedTime);
      Assert.AreEqual(2, workItemList.WorkItems.Count);
      Assert.AreEqual(workItemTwo.Duration, workItemList.WorkItems.First().Duration);
      Assert.AreEqual(workItemTwo.Name, workItemList.WorkItems.First().Name);
      Assert.AreEqual(workItemThree.Duration, workItemList.WorkItems.ToList()[1].Duration);
      Assert.AreEqual(workItemThree.Name, workItemList.WorkItems.ToList()[1].Name);
    }

    [Test]
    public void TestAddToDuration()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      var timeSpan = fixture.Create<TimeSpan>();
      workItemList.AddWorkItem(workItem);

      workItemList.AddToDuration(workItem.Id.Value, timeSpan);

      Assert.AreEqual(workItem.Duration + timeSpan, workItemList.WorkItems.First().Duration);
    }

    [Test]
    public void TestTryAddingDurationToNonExistingWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      workItemList.AddWorkItem(workItem);

      Assert.Throws<MissingSegmentException>(
        () => workItemList.AddToDuration(Guid.Empty, fixture.Create<TimeSpan>()));
    }

    [Test]
    public void TestSubtractFromDuration()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      var timeSpan = workItem.Duration - TimeSpan.FromMinutes(1);
      workItemList.AddWorkItem(workItem);

      workItemList.RemoveFromDuration(workItem.Id.Value, timeSpan);

      Assert.AreEqual(TimeSpan.FromMinutes(1), workItemList.WorkItems.First().Duration.Duration);
    }

    [Test]
    public void TestTrySubtractFromNonExistingWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      workItemList.AddWorkItem(workItem);

      Assert.Throws<MissingSegmentException>(
        () => workItemList.AddToDuration(Guid.Empty, fixture.Create<TimeSpan>()));
    }
  }
}
