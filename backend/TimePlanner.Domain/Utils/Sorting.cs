using System.Collections.Immutable;
using TimePlanner.Domain.Models;

namespace TimePlanner.Domain.Utils
{
  public record struct SortData(Guid Id, Category Category, int SortOrder);

  public static class Sorting
  {
    private static List<SortData> PrepareSource(List<SortData> source)
    {
      return source.OrderBy(i => i.Category).ThenBy(i => i.SortOrder).Select((d, i) =>
      {
        d.SortOrder = d.Category == Category.Completed || d.Category == Category.Scheduled ? int.MaxValue : i;
        return d;
      }).ToList();
    }

    private static ImmutableList<SortData> InsertAndUpdateIndexers(
      List<SortData> source,
      SortData item,
      int index)
    {
      item.SortOrder = index;
      source.Insert(index, item);
      for (var i = index + 1; i < source.Count; i++)
      {
        if (source[i].Category != Category.Completed && source[i].Category != Category.Scheduled)
        {
          int sortOrder = source[i].SortOrder + 1;
          source[i] = source[i] with { SortOrder = sortOrder };
        }
      }

      return source.ToImmutableList();
    }

    private static ImmutableList<SortData> AddTodayInternal(List<SortData> source, SortData item)
    {
      // empty list or no today items: add to the top
      if (source.Count == 0 || source.All(i => i.Category != Category.Today))
      {
        item.SortOrder = 0;
        return InsertAndUpdateIndexers(source, item, 0);
      }

      // category exists: add to its bottom
      var lastInCategory = source.FindLastIndex(p => p.Category == Category.Today);
      item.SortOrder = source[lastInCategory].SortOrder + 1;
      return InsertAndUpdateIndexers(source, item, lastInCategory + 1);
    }

    private static ImmutableList<SortData> AddTomorrowInternal(List<SortData> source, SortData item)
    {
      // empty list or only next week or completed/scheduled: add to the top
      if (source.Count == 0 ||
          source.All(i => i.Category == Category.NextWeek ||
                          i.Category == Category.Completed ||
                          i.Category == Category.Scheduled))
      {
        return InsertAndUpdateIndexers(source, item, 0);
      }

      // existing tomorrow: add to its bottom
      var tomorrowLast = source.FindLastIndex(p => p.Category == Category.Tomorrow);
      if (tomorrowLast > -1)
      {
        return InsertAndUpdateIndexers(source, item, tomorrowLast + 1);
      }

      // add to the bottom of today
      var todayLast = source.FindLastIndex(p => p.Category == Category.Today);
      return InsertAndUpdateIndexers(source, item, todayLast + 1);
    }

    private static ImmutableList<SortData> AddNextWeekInternal(List<SortData> source, SortData item)
    {
      // empty list or only completed/scheduled: add to the top
      if (source.Count == 0 || source.All(
            i => i.Category == Category.Completed || i.Category == Category.Scheduled))
      {
        return InsertAndUpdateIndexers(source, item, 0);
      }

      // existing next week: add to its bottom
      var nextWeekLast = source.FindLastIndex(p => p.Category == Category.NextWeek);
      if (nextWeekLast > -1)
      {
        return InsertAndUpdateIndexers(source, item, nextWeekLast + 1);
      }

      // add to the bottom of today or tomorrow
      var todayLast = source.FindLastIndex(p => p.Category == Category.Today || p.Category == Category.Tomorrow);
      return InsertAndUpdateIndexers(source, item, todayLast + 1);
    }

    private static ImmutableList<SortData> AddCompletedInternal(List<SortData> source, SortData item)
    {
      item.SortOrder = int.MaxValue;
      source.Add(item);
      return source.ToImmutableList();
    }

    private static ImmutableList<SortData> AddScheduledInternal(List<SortData> source, SortData item)
    {
      item.SortOrder = int.MaxValue;
      source.Add(item);
      return source.ToImmutableList();
    }

    public static ImmutableList<SortData> AddItem(List<SortData> source, SortData item)
    {
      var preparedSource = PrepareSource(source);
      switch (item.Category)
      {
        case Category.Today:
          return AddTodayInternal(preparedSource, item);
        case Category.Tomorrow:
          return AddTomorrowInternal(preparedSource, item);
        case Category.NextWeek:
          return AddNextWeekInternal(preparedSource, item);
        case Category.Completed:
          return AddCompletedInternal(preparedSource, item);
        case Category.Scheduled:
          return AddScheduledInternal(preparedSource, item);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public static ImmutableList<SortData> DeleteItem(List<SortData> rawSource, Guid itemId)
    {
      var source = PrepareSource(rawSource);
      SortData? item = source.SingleOrDefault(i => i.Id == itemId);
      if (item == null)
      {
        throw new ApplicationException("The work item is not present in the collection.");
      }

      source.Remove(item.Value);
      for (var i = item.Value.SortOrder; i < source.Count; i++)
      {
        if (source[i].Category != Category.Completed && source[i].Category != Category.Scheduled)
        {
          var sortOrder = source[i].SortOrder - 1;
          source[i] = source[i] with { SortOrder = sortOrder };
        }
      }

      return source.ToImmutableList();
    }

    public static ImmutableList<SortData> ChangeCategory(
      List<SortData> rawSource,
      Guid itemId,
      Category targetCategory)
    {
      List<SortData> deleted = DeleteItem(rawSource, itemId).ToList();
      SortData item = new SortData(itemId, targetCategory, 0);
      switch (targetCategory)
      {
        case Category.Today:
          return AddTodayInternal(deleted, item);
        case Category.Tomorrow:
          return AddTomorrowInternal(deleted, item);
        case Category.NextWeek:
          return AddNextWeekInternal(deleted, item);
        case Category.Completed:
          return AddCompletedInternal(deleted, item);
        case Category.Scheduled:
          return AddScheduledInternal(deleted, item);
        default:
          throw new ArgumentOutOfRangeException(nameof(targetCategory), targetCategory, null);
      }
    }

    public static ImmutableList<SortData> ChangeSortOrder(List<SortData> rawSource, SortData item, int diff)
    {
      if (item.Category == Category.Completed || item.Category == Category.Scheduled)
      {
        throw new ApplicationException($"Reordering {item.Category} category is not supported.");
      }

      var source = PrepareSource(rawSource);
      var firstIndex = source.FindIndex(p => p.Category == item.Category);
      if (firstIndex == -1)
      {
        throw new ApplicationException("Can not reorder an empty category.");
      }

      var lastIndex = source.FindLastIndex(p => p.Category == item.Category);
      if (lastIndex == firstIndex)
      {
        return source.ToImmutableList();
      }

      var index = source.IndexOf(item);
      if (index + diff > lastIndex || index + diff < firstIndex)
      {
        throw new ApplicationException("The reordering target index is out of range.");
      }

      var deleted = DeleteItem(source, item.Id).ToList();
      return InsertAndUpdateIndexers(deleted, item, index + diff);
    }
  }
}
