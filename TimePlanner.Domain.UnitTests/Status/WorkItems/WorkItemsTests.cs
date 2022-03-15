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
      string workItemName = fixture.Create<string>();
      var workItemList = new WorkItemList();

      workItemList.AddWorkItem(workItemName);

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(1, workItemList.WorkItems.Count);
      Assert.AreEqual(TimeSpan.Zero, workItemList.WorkItems[0].Duration.Duration);
      Assert.AreEqual(workItemName, workItemList.WorkItems[0].Name);
      Assert.IsNull(workItemList.WorkItems[0].Id);
    }

    [Test]
    public void TestAddWorkItemOverflow()
    {
      string workItemName = fixture.Create<string>();
      var workItemList = new WorkItemList();
      for(int i = 0; i < 24 * 4; i++)
      {
        workItemList.AddWorkItem(fixture.Create<string>());
      }
      var exception = Assert.Throws<NoSegmentsAvailableException>(
        () => workItemList.AddWorkItem(workItemName));

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(24 * 4, workItemList.WorkItems.Count);
    }

    [Test]
    public void TestRemoveWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      workItemList.WorkItems.Add(workItem);
      workItemList.RemoveWorkItem(workItem.Id.Value);

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(0, workItemList.WorkItems.Count);
    }

    [Test]
    public void TestTryRemovingNonExistingWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      workItemList.WorkItems.Add(workItem);

      var ex = Assert.Throws<MissingSegmentException>(
        () => workItemList.RemoveWorkItem(Guid.Empty));

      Assert.AreEqual(2, ex.Index);
    }

    [Test]
    public void TestRemoveMiddleWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItemOne = fixture.Create<WorkItem>();
      var workItemTwo = fixture.Create<WorkItem>();
      var workItemThree = fixture.Create<WorkItem>();
      workItemList.WorkItems.Add(workItemOne);
      workItemList.WorkItems.Add(workItemTwo);
      workItemList.RemoveWorkItem(workItemOne.Id.Value);
      workItemList.WorkItems.Add(workItemThree);

      Assert.AreEqual(workItemTwo.Duration + workItemThree.Duration, workItemList.DistributedTime.Duration);
      Assert.AreEqual(2, workItemList.WorkItems.Count);
      Assert.AreEqual(workItemTwo.Duration, workItemList.WorkItems[0].Duration);
      Assert.AreEqual(workItemTwo.Name, workItemList.WorkItems[0].Name);
      Assert.AreEqual(workItemThree.Duration, workItemList.WorkItems[1].Duration);
      Assert.AreEqual(workItemThree.Name, workItemList.WorkItems[1].Name);
    }

    [Test]
    public void TestAddToDuration()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      var timeSpan = fixture.Create<TimeSpan>();
      workItemList.WorkItems.Add(workItem);

      workItemList.AddToDuration(workItem.Id.Value, timeSpan);

      Assert.AreEqual(workItem.Duration + timeSpan, workItemList.WorkItems[0].Duration.Duration);
    }

    [Test]
    public void TestTryAddingDurationToNonExistingWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();

      var ex = Assert.Throws<MissingSegmentException>(
        () => workItemList.AddToDuration(Guid.Empty, fixture.Create<TimeSpan>()));
      Assert.AreEqual(1, ex.Index);

    }

    [Test]
    public void TestSubtractFromDuration()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();
      var timeSpan = fixture.Create<TimeSpan>();
      workItemList.WorkItems.Add(workItem);

      workItemList.AddToDuration(workItem.Id.Value, timeSpan);

      Assert.AreEqual(workItem.Duration - timeSpan, workItemList.WorkItems[0].Duration.Duration);
    }

    [Test]
    public void TestTrySubtractFromNonExistingWorkItem()
    {
      var workItemList = new WorkItemList();
      var workItem = fixture.Create<WorkItem>();

      var ex = Assert.Throws<MissingSegmentException>(
        () => workItemList.AddToDuration(Guid.Empty, fixture.Create<TimeSpan>()));
      Assert.AreEqual(1, ex.Index);
    }
  }
}
