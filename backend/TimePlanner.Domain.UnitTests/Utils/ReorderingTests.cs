using System.Collections.Immutable;
using NUnit.Framework;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.UnitTests.Utils
{
  public class ReorderingTests
  {
 /// <summary>
    /// SortData list with all possible categories and some sorting orders.
    /// </summary>
    private ImmutableList<SortData> CreateCorrectlyOrderedSut(int minItems)
    {
      var result = new List<SortData>();
      var random = new Random();
      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(new SortData(Guid.NewGuid(),Category.Today, result.Count));
      }

      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(new SortData(Guid.NewGuid(),Category.Tomorrow, result.Count));
      }

      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(new SortData(Guid.NewGuid(),Category.NextWeek, result.Count));
      }

      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(new SortData(Guid.NewGuid(),Category.Completed, result.Count));
      }

      for (var i = 0; i < random.Next(minItems, 25); i++)
      {
        result.Add(new SortData(Guid.NewGuid(), Category.Scheduled, result.Count));
      }

      return result.ToImmutableList();
    }

    private static void VerifyInvariants(ImmutableList<SortData> result)
    {
      // The expected result is the ordered list with adjacent correct sortOrders (int max value for Completed/Scheduled)
      var activeCount = result.Count(i => i.Category != Category.Completed && i.Category != Category.Scheduled);
      for (var i = 0; i < activeCount; i++)
      {
        Assert.AreEqual(i, result[i].SortOrder);
      }

      foreach (var item in result.Where(i => i.Category == Category.Completed || i.Category == Category.Scheduled))
      {
        Assert.AreEqual(int.MaxValue, item.SortOrder);
      }
    }

    private void VerifyOrderChange(List<SortData> slice, ImmutableList<SortData> result, int orderDiff)
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
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(0);
      SortData item = new SortData(Guid.NewGuid(), Category.Today, 0);

      ImmutableList<SortData> result = Sorting.AddItem(source.ToList(), item);

      VerifyInvariants(result);
      int updatedSortOrder = result.Single(p => p.Id == item.Id).SortOrder;

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
      var changed = source.Where(
        p => p.Category != Category.Today &&
             p.Category != Category.Completed &&
             p.Category != Category.Scheduled).ToList();
      if (changed.Count > 0)
      {
        VerifyOrderChange(changed, result, 1);
      }
    }

    [Test]
    public void TestAddTomorrow()
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(0);
      SortData item = new SortData(Guid.NewGuid(), Category.Tomorrow, 0);

      ImmutableList<SortData> result = Sorting.AddItem(source.ToList(), item);

      VerifyInvariants(result);
      int updatedSortOrder = result.Single(p => p.Id == item.Id).SortOrder;

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
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(0);
      SortData item = new SortData(Guid.NewGuid(), Category.NextWeek, 0);

      ImmutableList<SortData> result = Sorting.AddItem(source.ToList(), item);

      VerifyInvariants(result);
      int updatedSortOrder = result.Single(p => p.Id == item.Id).SortOrder;

      // test position of updated index
      var sourceNextWeekLast = source.FindLastIndex(i => i.Category == Category.NextWeek);
      if (sourceNextWeekLast > -1)
      {
        Assert.AreEqual(sourceNextWeekLast + 1, updatedSortOrder);
      }
      else
      {
        var sourceUnsortedFirst = source.FindIndex(i => i.Category == Category.Completed || i.Category == Category.Scheduled);
        if (sourceUnsortedFirst > -1)
        {
          Assert.AreEqual(sourceUnsortedFirst, updatedSortOrder);
        }
        else
        {
          Assert.AreEqual(source.Count, updatedSortOrder);
        }
      }

      // test upper items ure untouched
      var unchanged = source.Where(p => p.Category != Category.Completed && p.Category != Category.Scheduled).ToList();
      if (unchanged.Count > 0)
      {
        VerifyOrderChange(unchanged, result, 0);
      }
      // we've already verified that completed are int.MaxValue
    }

    [Test]
    public void TestAddCompleted()
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(0);
      SortData item = new SortData(Guid.NewGuid(), Category.Completed, 0);

      ImmutableList<SortData> result = Sorting.AddItem(source.ToList(), item);

      VerifyInvariants(result);

      // test updated index
      Assert.AreEqual(int.MaxValue, result.Single(p => p.Id == item.Id).SortOrder);

      // no indexes should have changed
      VerifyOrderChange(
        source.Where(p => p.Category != Category.Completed && p.Category != Category.Scheduled).ToList(), result, 0);
    }

    [Test]
    public void TestAddScheduled()
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(0);
      SortData item = new SortData(Guid.NewGuid(), Category.Scheduled, 0);

      ImmutableList<SortData> result = Sorting.AddItem(source.ToList(), item);

      VerifyInvariants(result);

      // test updated index
      Assert.AreEqual(int.MaxValue, result.Single(p => p.Id == item.Id).SortOrder);

      // no indexes should have changed
      VerifyOrderChange(
        source.Where(p => p.Category != Category.Completed && p.Category != Category.Scheduled).ToList(), result, 0);
    }

    [Test]
    public void TestChangeActiveCategory(
      [Values(Category.Today, Category.Tomorrow, Category.NextWeek, Category.Completed, Category.Scheduled)]
      Category from,
      [Values(Category.Today, Category.Tomorrow, Category.NextWeek)]
      Category to)
    {
      if (from == to)
      {
        return;
      }
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(1);
      SortData item = source.First(p => p.Category == from);

      ImmutableList<SortData> result = Sorting.ChangeCategory(source.ToList(), item.Id, to);

      VerifyInvariants(result);

      // test that SortData is at the bottom of its category
      int newIndex = result.FindIndex(p => p.Id == item.Id);
      Assert.AreEqual(result.Last(p => p.Category == to).SortOrder, newIndex);

      int oldIndex = source.IndexOf(item);
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
        var below = source.Skip(newIndex + 1)
          .Where(p => p.Category != Category.Completed && p.Category != Category.Scheduled).ToList();
        VerifyOrderChange(below, result, 0);
      }

      if (oldIndex > newIndex)
      {
        // test that orders above new location are untouched
        var above = source.Take(newIndex).ToList();
        VerifyOrderChange(above, result, 0);

        // test that orders below new location and above old location are increased
        var middle = source.Take(oldIndex).Skip(newIndex).Where(i => i.Category != Category.Completed && i.Category != Category.Scheduled).ToList();
        VerifyOrderChange(middle, result, 1);

        // test that orders below insertion are untouched
        var below = source.Skip(oldIndex + 1)
          .Where(p => p.Category != Category.Completed && p.Category != Category.Scheduled).ToList();
        VerifyOrderChange(below, result, 0);
      }
    }

    [Test]
    public void TestChangeActiveCategoryToCompleted(
      [Values(Category.Today, Category.Tomorrow, Category.NextWeek)] Category from)
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(1);
      SortData item = source.First(p => p.Category == from);

      ImmutableList<SortData> result = Sorting.ChangeCategory(source.ToList(), item.Id, Category.Completed);

      VerifyInvariants(result);

      // test that SortData is at the bottom
      Assert.AreEqual(item.Id, result.Last().Id);

      // test that orders above old location are untouched
      var above = source.Take(item.SortOrder).ToList();
      VerifyOrderChange(above, result, 0);

      // test that orders below old location are decreased
      var middle = source.Where(i => i.SortOrder > item.SortOrder)
        .Where(i => i.Category != Category.Completed && i.Category != Category.Scheduled ).ToList();
      VerifyOrderChange(middle, result, -1);
    }

    [Test]
    public void TestChangeScheduledCategoryToCompleted()
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(1);
      SortData item = source.First(p => p.Category == Category.Scheduled);

      ImmutableList<SortData> result = Sorting.ChangeCategory(source.ToList(), item.Id, Category.Completed);

      VerifyInvariants(result);

      // test that SortData is at the bottom
      Assert.AreEqual(item.Id, result.Last().Id);

      // test that active categories are untouched
      var above = source.Where(i => i.Category != Category.Completed && i.Category != Category.Scheduled).ToList();
      VerifyOrderChange(above, result, 0);
    }

    [Test]
    public void TestChangeCategoryToScheduled(
      [Values(Category.Today, Category.Tomorrow, Category.NextWeek)] Category from)
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(1);
      SortData item = source.First(p => p.Category == from);

      ImmutableList<SortData> result = Sorting.ChangeCategory(source.ToList(), item.Id, Category.Scheduled);

      VerifyInvariants(result);

      // test that SortData is at the bottom
      Assert.AreEqual(item.Id, result.Last().Id);

      // test that orders above old location are untouched
      var above = source.Take(item.SortOrder).ToList();
      VerifyOrderChange(above, result, 0);

      // test that orders below old location are decreased
      var middle = source.Where(i => i.SortOrder > item.SortOrder)
        .Where(i => i.Category != Category.Completed && i.Category != Category.Scheduled).ToList();
      VerifyOrderChange(middle, result, -1);
    }

    [Test]
    public void TestChangeCompletedCategoryToScheduled()
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(1);
      SortData item = source.First(p => p.Category == Category.Completed);

      ImmutableList<SortData> result = Sorting.ChangeCategory(source.ToList(), item.Id, Category.Scheduled);

      VerifyInvariants(result);

      // test that SortData is at the bottom
      Assert.AreEqual(item.Id, result.Last().Id);

      // test that active categories are untouched
      var above = source.Where(i => i.Category != Category.Completed && i.Category != Category.Scheduled).ToList();
      VerifyOrderChange(above, result, 0);
    }

    [TestCase(Category.Today)]
    [TestCase(Category.Tomorrow)]
    [TestCase(Category.NextWeek)]
    public void TestDeleteFromActiveCategory(Category category)
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(1);
      SortData item = source.First(p => p.Category == category);

      ImmutableList<SortData> result = Sorting.DeleteItem(source.ToList(), item.Id);

      VerifyInvariants(result);

      // Test that orders above deleted are untouched
      // Test that orders below deleted are less by one
      if (item.SortOrder > 0)
      {
        var above = source.Take(item.SortOrder).ToList();
        VerifyOrderChange(above, result, 0);
      }

      var below = source.Where(p => p.Category != Category.Completed && p.Category != Category.Scheduled)
        .Skip(item.SortOrder + 1).ToList();
      VerifyOrderChange(below, result, -1);
    }

    [TestCase(Category.Completed)]
    [TestCase(Category.Scheduled)]
    public void TestDeleteFromNonSortedCategory(Category category)
    {
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(1);
      SortData item = source.First(p => p.Category == category);

      ImmutableList<SortData> result = Sorting.DeleteItem(source.ToList(), item.Id);

      VerifyInvariants(result);

      // Test that active categories are untouched
      var active = source.Where(i => i.Category != Category.Completed && i.Category != Category.Scheduled).ToList();
      VerifyOrderChange(active, result, 0);
    }

    [TestCase(Category.Today)]
    [TestCase(Category.Tomorrow)]
    [TestCase(Category.NextWeek)]
    public void TestIncreaseSortOrderWithinCategory(Category category)
    {
      // 0, 1, 2, 3, 4 ...
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(4);

      // move 1 to 3
      SortData SortData = source.Where(p => p.Category == category).ToList()[1];
      ImmutableList<SortData> result = Sorting.ChangeSortOrder(source.ToList(), SortData, 2);

      // The expected result is the ordered list with adjacent correct sortOrders (reset for Completed)
      VerifyInvariants(result);
      SortData updated = result.Single(i => i.Id == SortData.Id);

      // test that SortData is in the correct place of its category
      Assert.AreEqual(SortData.SortOrder + 2, updated.SortOrder);

      // test that orders above the original location are untouched
      var above = source.Where(p => p.SortOrder < SortData.SortOrder).ToList();
      VerifyOrderChange(above, result, 0);

      // test that orders below origin and above insertion are less
      var middle = source.Where(p => p.SortOrder > SortData.SortOrder && p.SortOrder <= SortData.SortOrder + 2).ToList();
      VerifyOrderChange(middle, result, -1);

      // test that orders below insertion are untouched
      var below = source.Where(p => p.SortOrder > SortData.SortOrder + 2)
        .Where(p => p.Category != Category.Completed && p.Category != Category.Scheduled).ToList();
      VerifyOrderChange(below, result, 0);
    }

    [TestCase(Category.Today)]
    [TestCase(Category.Tomorrow)]
    [TestCase(Category.NextWeek)]
    public void TestDecreaseSortOrderWithinCategory(Category category)
    {
      // 0, 1, 2, 3, 4 ...
      ImmutableList<SortData> source = CreateCorrectlyOrderedSut(4);

      // move 3 to 1
      SortData SortData = source.Where(p => p.Category == category).ToList()[^1];
      ImmutableList<SortData> result = Sorting.ChangeSortOrder(source.ToList(), SortData, -2);

      // The expected result is the ordered list with adjacent correct sortOrders (reset for Completed)
      VerifyInvariants(result);
      SortData updated = result.Single(i => i.Id == SortData.Id);

      // test that SortData is in the correct place of its category
      Assert.AreEqual(SortData.SortOrder - 2, updated.SortOrder);

      // test that orders above the target location are untouched
      var above = source.Take(updated.SortOrder).ToList();
      VerifyOrderChange(above, result, 0);

      // test that orders below origin and above insertion are greater
      var middle = source.Where(i => i.SortOrder < SortData.SortOrder && i.SortOrder >= SortData.SortOrder - 2).ToList();
      VerifyOrderChange(middle, result, 1);

      // test that orders below insertion are untouched
      var below = source.Where(i => i.SortOrder > SortData.SortOrder + 2)
        .Where(p => p.Category != Category.Completed && p.Category != Category.Scheduled).ToList();
      VerifyOrderChange(below, result, 0);
    }

    [TestCase(Category.Today)]
    [TestCase(Category.Tomorrow)]
    [TestCase(Category.NextWeek)]
    public void TestNonAdjacentItems(Category category)
    {
      List<SortData> list = CreateCorrectlyOrderedSut(2).ToList();

      var elements = list.Where(i => i.Category == category).TakeLast(2).ToList();
      var first = elements[0];
      var second = elements[1];
      list[second.SortOrder] = list[second.SortOrder] with { SortOrder = 1000};
      var third = new SortData(Guid.NewGuid(), category, 0);
      ImmutableList<SortData> result = Sorting.AddItem(list, third);

      VerifyInvariants(result);

      var three = result.Where(i => i.Category == category).TakeLast(3).ToList();
      Assert.AreEqual(first.Id, three[0].Id);
      Assert.AreEqual(second.Id, three[1].Id);
      Assert.AreEqual(third.Id, three[2].Id);
    }
  }
}
