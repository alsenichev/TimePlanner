using System.Collections.Immutable;
using NUnit.Framework;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Utils;

namespace TimePlanner.DataAccess.UnitTests
{
  public class ReorderingTests
  {
    private WorkItem CreateWorkItem(Category category, int sortOrder = 0)
    {
      return new WorkItem(
        Guid.NewGuid(),
        string.Empty,
        new List<Duration>(),
        category,
        null,
        null,
        new History(),
        sortOrder);
    }

    /// <summary>
    /// WorkItems with all possible categories and some sorting orders.
    /// </summary>
    private ImmutableList<WorkItem> CreateCorrectlyOrderedSut(int minItems)
    {
      var result = new List<WorkItem>();
      var random = new Random();
      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(CreateWorkItem(Category.Today, result.Count));
      }

      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(CreateWorkItem(Category.Tomorrow, result.Count));
      }

      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(CreateWorkItem(Category.NextWeek, result.Count));
      }

      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(CreateWorkItem(Category.Completed, result.Count));
      }

      return result.ToImmutableList();
    }

    private static void VerifyInvariants(ImmutableList<WorkItem> result)
    {
      // The expected result is the ordered list with adjacent correct sortOrders (reset for Completed)
      var activeCount = result.Count(i => i.Category != Category.Completed);
      for (var i = 0; i < activeCount; i++)
      {
        Assert.AreEqual(i, result[i].SortOrder);
      }

      foreach (var item in result.Where(i => i.Category == Category.Completed))
      {
        Assert.AreEqual(int.MaxValue, item.SortOrder);
      }
    }

    private void VerifyOrderChange(List<WorkItem> slice, ImmutableList<WorkItem> result, int orderDiff)
    {
      var ids = slice.Select(i => i.Id).ToHashSet();

      Assert.IsTrue(
        slice.Zip(result.Where(
              i => ids.Contains(i.Id)),
            (s, r) => r.SortOrder - s.SortOrder)
          .All(d => d == orderDiff));
    }

    [Test]
    public void TestAddToday()
    {
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(0);
      WorkItem workItem = CreateWorkItem(Category.Today);

      ImmutableList<WorkItem> result = Sorting.AddToday(source.ToList(), workItem);

      VerifyInvariants(result);
      int updatedSortOrder = result.Single(p => p.Id == workItem.Id).SortOrder;

      // test position of updated index
      var sourceTodayLast = source.FindLastIndex(i => i.Category == Category.Today);
      if (sourceTodayLast > -1)
      {
        Assert.AreEqual(sourceTodayLast + 1, updatedSortOrder);
      }
      else
      {
        Assert.AreEqual(0, updatedSortOrder);
      }

      // test upper items ure untouched
      var unchanged = source.Where(p => p.Category == Category.Today).ToList();
      if (unchanged.Count > 0)
      {
        VerifyOrderChange(unchanged, result, 0);
      }

      // test lower items are increased
      var changed = source.Where(p => p.Category != Category.Today && p.Category != Category.Completed).ToList();
      if (changed.Count > 0)
      {
        VerifyOrderChange(changed, result, 1);
      }
    }

    [Test]
    public void TestAddTomorrow()
    {
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(0);
      WorkItem workItem = CreateWorkItem(Category.Tomorrow);

      ImmutableList<WorkItem> result = Sorting.AddTomorrow(source.ToList(), workItem);

      VerifyInvariants(result);
      int updatedSortOrder = result.Single(p => p.Id == workItem.Id).SortOrder;

      // test position of updated index
      var sourceTomorrowLast = source.FindLastIndex(i => i.Category == Category.Tomorrow);
      if (sourceTomorrowLast > -1)
      {
        Assert.AreEqual(sourceTomorrowLast + 1, updatedSortOrder);
      }
      else
      {
        var sourceTodayLast = source.FindLastIndex(i => i.Category == Category.Today);
        if (sourceTodayLast > -1)
        {
          Assert.AreEqual(sourceTodayLast + 1, updatedSortOrder);
        }
        else
        {
          Assert.AreEqual(0, updatedSortOrder);
        }
      }

      // test upper items ure untouched
      var unchanged = source.Where(p => p.Category == Category.Today || p.Category == Category.Tomorrow).ToList();
      if (unchanged.Count > 0)
      {
        VerifyOrderChange(unchanged, result, 0);
      }

      // test lower items are increased
      var changed = source.Where(p => p.Category == Category.NextWeek).ToList();
      if (changed.Count > 0)
      {
        VerifyOrderChange(changed, result, 1);
      }
    }

    [Test]
    public void TestAddNextWeek()
    {
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(0);
      WorkItem workItem = CreateWorkItem(Category.NextWeek);

      ImmutableList<WorkItem> result = Sorting.AddNextWeek(source.ToList(), workItem);

      VerifyInvariants(result);
      int updatedSortOrder = result.Single(p => p.Id == workItem.Id).SortOrder;

      // test position of updated index
      var sourceNextWeekLast = source.FindLastIndex(i => i.Category == Category.NextWeek);
      if (sourceNextWeekLast > -1)
      {
        Assert.AreEqual(sourceNextWeekLast + 1, updatedSortOrder);
      }
      else
      {
        var sourceCompletedFirst = source.FindIndex(i => i.Category == Category.Completed);
        if (sourceCompletedFirst > -1)
        {
          Assert.AreEqual(sourceCompletedFirst - 1, updatedSortOrder);
        }
        else
        {
          Assert.AreEqual(source.Count, updatedSortOrder);
        }
      }

      // test upper items ure untouched
      var unchanged = source.Where(p => p.Category != Category.Completed).ToList();
      if (unchanged.Count > 0)
      {
        VerifyOrderChange(unchanged, result, 0);
      }
      // we've already verified that completed are int.MaxValue
    }

    [Test]
    public void TestAddCompleted()
    {
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(0);
      WorkItem workItem = CreateWorkItem(Category.Completed);

      ImmutableList<WorkItem> result = Sorting.AddCompleted(source.ToList(), workItem);

      VerifyInvariants(result);

      // test updated index
      Assert.AreEqual(int.MaxValue, result.Single(p => p.Id == workItem.Id).SortOrder);

      // no indexes should have changed
      VerifyOrderChange(source.Where(p => p.Category != Category.Completed).ToList(), result, 0);
    }

    [Test]
    public void TestChangeCategory(
      [Values(Category.Today, Category.Tomorrow, Category.NextWeek, Category.Completed)]
      Category from,
      [Values(Category.Today, Category.Tomorrow, Category.NextWeek)]
      Category to)
    {
      if (from == to)
      {
        return;
      }
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(1);
      WorkItem workItem = source.First(p => p.Category == from);

      ImmutableList<WorkItem> result = Sorting.ChangeCategory(source.ToList(), workItem, to);

      // The expected result is the ordered list with adjacent correct sortOrders (reset for Completed)
      VerifyInvariants(result);

      // test that WorkItem is at the bottom of its category
      int newIndex = result.FindIndex(p => p.Id == workItem.Id);
      Assert.AreEqual(result.Last(p => p.Category == to).SortOrder, newIndex);

      int oldIndex = source.IndexOf(workItem);
      if (newIndex > oldIndex)
      {
        // test that orders above the original location are untouched
        if (oldIndex > 0)
        {
          var above = source.Take(oldIndex).ToList();
          VerifyOrderChange(above, result, 0);
        }
        // test that orders below origin and above insertion are less
        var middle = source.Take(newIndex + 1).Skip(oldIndex + 1).ToList();
        VerifyOrderChange(middle, result, -1);

        // test that orders below insertion are untouched
        var below = source.Skip(newIndex + 1).Where(p => p.Category != Category.Completed).ToList();
        VerifyOrderChange(below, result, 0);
      }

      if (oldIndex > newIndex)
      {
        // test that orders above new location are untouched
        var above = source.Take(newIndex).ToList();
        VerifyOrderChange(above, result, 0);

        // test that orders below new location and above old location are increased
        var middle = source.Take(oldIndex).Skip(newIndex).ToList();
        VerifyOrderChange(middle, result, 1);

        // test that orders below insertion are untouched
        var below = source.Skip(oldIndex + 1).Where(p => p.Category != Category.Completed).ToList();
        VerifyOrderChange(below, result, 0);
      }
    }

    [Test]
    public void TestChangeCategoryToCompleted(
      [Values(Category.Today, Category.Tomorrow, Category.NextWeek)] Category from)
    {
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(1);
      WorkItem workItem = source.First(p => p.Category == from);

      ImmutableList<WorkItem> result = Sorting.ChangeCategory(source.ToList(), workItem, Category.Completed);

      // The expected result is the ordered list with adjacent correct sortOrders (reset for Completed)
      VerifyInvariants(result);

      // test that WorkItem is at the bottom
      Assert.AreEqual(workItem.Id, result.Last().Id);

      // test that orders above old location are untouched
      var above = source.Take(workItem.SortOrder).ToList();
      VerifyOrderChange(above, result, 0);

      // test that orders below old location are decreased
      var middle = source.Where(i => i.SortOrder > workItem.SortOrder).Where(i => i.Category!=Category.Completed).ToList();
      VerifyOrderChange(middle, result, -1);
    }

    [TestCase(Category.Today)]
    [TestCase(Category.Tomorrow)]
    [TestCase(Category.NextWeek)]
    [TestCase(Category.Completed)]
    public void TestDelete(Category category)
    {
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(1);
      WorkItem workItem = source.First(p => p.Category == category);

      ImmutableList<WorkItem> result = Sorting.DeleteWorkItem(source.ToList(), workItem);

      VerifyInvariants(result);

      // Test that orders above deleted are untouched
      // Test that orders below deleted are less by one
      if (workItem.SortOrder > 0)
      {
        var above = source.Take(workItem.SortOrder).ToList();
        VerifyOrderChange(above, result, 0);
      }

      var below = source.Where(p => p.Category != Category.Completed).Skip(workItem.SortOrder + 1).ToList();
      VerifyOrderChange(below, result, -1);
    }

    [TestCase(Category.Today)]
    [TestCase(Category.Tomorrow)]
    [TestCase(Category.NextWeek)]
    public void TestIncreaseSortOrderWithinCategory(Category category)
    {
      // 0, 1, 2, 3, 4 ...
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(4);

      // move 1 to 3
      WorkItem workItem = source.Where(p => p.Category == category).ToList()[1];
      ImmutableList<WorkItem> result = Sorting.ChangeSortOrder(source.ToList(), workItem, 2);

      // The expected result is the ordered list with adjacent correct sortOrders (reset for Completed)
      VerifyInvariants(result);
      WorkItem updated = result.Single(i => i.Id == workItem.Id);

      // test that WorkItem is in the correct place of its category
      Assert.AreEqual(workItem.SortOrder + 2, updated.SortOrder);

      // test that orders above the original location are untouched
      var above = source.Where(p => p.SortOrder < workItem.SortOrder).ToList();
      VerifyOrderChange(above, result, 0);

      // test that orders below origin and above insertion are less
      var middle = source.Where(p => p.SortOrder > workItem.SortOrder && p.SortOrder <= workItem.SortOrder + 2).ToList();
      VerifyOrderChange(middle, result, -1);

      // test that orders below insertion are untouched
      var below = source.Where(p => p.SortOrder > workItem.SortOrder + 2).Where(p => p.Category != Category.Completed).ToList();
      VerifyOrderChange(below, result, 0);
    }

    [TestCase(Category.Today)]
    [TestCase(Category.Tomorrow)]
    [TestCase(Category.NextWeek)]
    public void TestDecreaseSortOrderWithinCategory(Category category)
    {
      // 0, 1, 2, 3, 4 ...
      ImmutableList<WorkItem> source = CreateCorrectlyOrderedSut(4);

      // move 3 to 1
      WorkItem workItem = source.Where(p => p.Category == category).ToList()[^1];
      ImmutableList<WorkItem> result = Sorting.ChangeSortOrder(source.ToList(), workItem, -2);

      // The expected result is the ordered list with adjacent correct sortOrders (reset for Completed)
      VerifyInvariants(result);
      WorkItem updated = result.Single(i => i.Id == workItem.Id);

      // test that WorkItem is in the correct place of its category
      Assert.AreEqual(workItem.SortOrder - 2, updated.SortOrder);

      // test that orders above the target location are untouched
      var above = source.Take(updated.SortOrder).ToList();
      VerifyOrderChange(above, result, 0);

      // test that orders below origin and above insertion are greater
      var middle = source.Where(i => i.SortOrder < workItem.SortOrder && i.SortOrder >= workItem.SortOrder - 2).ToList();
      VerifyOrderChange(middle, result, 1);

      // test that orders below insertion are untouched
      var below = source.Where(i => i.SortOrder > workItem.SortOrder + 2).Where(p => p.Category != Category.Completed).ToList();
      VerifyOrderChange(below, result, 0);
    }

    [TestCase(Category.Today)]
    [TestCase(Category.Tomorrow)]
    [TestCase(Category.NextWeek)]
    public void TestNonAdjacentItems(Category category)
    {
      List<WorkItem> list = CreateCorrectlyOrderedSut(2).ToList();

      var elements = list.Where(i => i.Category == category).TakeLast(2).ToList();
      var first = elements[0];
      var second = elements[1];
      list[second.SortOrder] = list[second.SortOrder] with { SortOrder = 1000};
      var third = CreateWorkItem(category);
      ImmutableList<WorkItem> result;
      switch (category)
      {
        case Category.Today:
          result = Sorting.AddToday(list, third);
          break;
        case Category.Tomorrow:
          result = Sorting.AddTomorrow(list, third);
          break;
        case Category.NextWeek:
          result = Sorting.AddNextWeek(list, third);
          break;
        default:
          throw new ApplicationException("should never happen");
      }
      VerifyInvariants(result);
      var three = result.Where(i => i.Category == category).TakeLast(3).ToList();
      Assert.AreEqual(first.Id, three[0].Id);
      Assert.AreEqual(second.Id, three[1].Id);
      Assert.AreEqual(third.Id, three[2].Id);
    }
  }
}
