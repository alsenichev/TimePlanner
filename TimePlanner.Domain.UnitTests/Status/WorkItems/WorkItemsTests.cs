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
      fixture.Register(() => TimeSpan.FromMinutes(new Random().Next(15, 1440)));
    }


    [Test]
    public void TestCreateWorkItemsList()
    {
      var workItemList = new WorkItemList();

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(0, workItemList.GetWorkItems().Count);
    }

    [Test]
    public void TestAddWorkItem()
    {
      string workItemName = fixture.Create<string>();
      var workItemList = new WorkItemList();

      workItemList.AddWorkItem(workItemName);

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(1, workItemList.GetWorkItems().Count);
      Assert.AreEqual(TimeSpan.Zero, workItemList.GetWorkItems()[0].Duration.Duration);
      Assert.AreEqual(workItemName, workItemList.GetWorkItems()[0].Name);
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
      Assert.AreEqual(24 * 4, workItemList.GetWorkItems().Count);
    }

    [Test]
    public void TestRemoveWorkItem()
    {
      var workItemList = new WorkItemList();

      workItemList.AddWorkItem(fixture.Create<string>());
      workItemList.RemoveWorkItem(0);

      Assert.AreEqual(TimeSpan.Zero, workItemList.DistributedTime.Duration);
      Assert.AreEqual(0, workItemList.GetWorkItems().Count);
    }

    [Test]
    public void TestTryRemovingNonExistingWorkItem()
    {
      var workItemList = new WorkItemList();

      workItemList.AddWorkItem(fixture.Create<string>());
      var ex = Assert.Throws<MissingSegmentException>(
        () => workItemList.RemoveWorkItem(2));

      Assert.AreEqual(2, ex.Index);
    }

    [Test]
    public void TestRemoveMiddleWorkItem()
    {
      var workItemList = new WorkItemList();
      workItemList.AddWorkItem("1");
      workItemList.AddToDuration(0, TimeSpan.FromMinutes(1));
      workItemList.AddWorkItem("2");
      workItemList.AddToDuration(1, TimeSpan.FromMinutes(2));

      workItemList.RemoveWorkItem(0);
      workItemList.AddWorkItem("new");
      workItemList.AddToDuration(1, TimeSpan.FromMinutes(3));

      Assert.AreEqual(TimeSpan.FromMinutes(5), workItemList.DistributedTime.Duration);
      Assert.AreEqual(2, workItemList.GetWorkItems().Count);
      Assert.AreEqual(TimeSpan.FromMinutes(2), workItemList.GetWorkItems()[0].Duration.Duration);
      Assert.AreEqual("2", workItemList.GetWorkItems()[0].Name);
      Assert.AreEqual(TimeSpan.FromMinutes(3), workItemList.GetWorkItems()[1].Duration.Duration);
      Assert.AreEqual("new", workItemList.GetWorkItems()[1].Name);
    }

    [Test]
    public void TestAddToDuration()
    {
      TimeSpan timeSpan = fixture.Create<TimeSpan>();
      var workItemsList = new WorkItemList();
      workItemsList.AddWorkItem(fixture.Create<string>());

      workItemsList.AddToDuration(0, timeSpan);

      Assert.AreEqual(timeSpan, workItemsList.GetWorkItems()[0].Duration.Duration);
    }

    [Test]
    public void TestTryAddingDurationToNonExistingWorkItem()
    {
      var workItemsList = new WorkItemList();
      workItemsList.AddWorkItem(fixture.Create<string>());

      var ex = Assert.Throws<MissingSegmentException>(
        ()=> workItemsList.AddToDuration(1, fixture.Create<TimeSpan>()));
      Assert.AreEqual(1, ex.Index);

    }

    [Test]
    public void TestSubtractFromDuration()
    {
      TimeSpan timeSpan = fixture.Create<TimeSpan>();
      var workItemsList = new WorkItemList();
      workItemsList.AddWorkItem(fixture.Create<string>());
      workItemsList.AddToDuration(0, timeSpan);

      workItemsList.RemoveFromDuration(0, TimeSpan.FromMinutes(5));

      Assert.AreEqual(timeSpan-TimeSpan.FromMinutes(5), workItemsList.GetWorkItems()[0].Duration.Duration);
    }

    [Test]
    public void TestTrySubtractFromNonExistingWorkItem()
    {
      var workItemsList = new WorkItemList();
      workItemsList.AddWorkItem(fixture.Create<string>());

      var ex = Assert.Throws<MissingSegmentException>(
        ()=> workItemsList.RemoveFromDuration(1, TimeSpan.FromMinutes(5)));

      Assert.AreEqual(1, ex.Index);
    }
  }
}
