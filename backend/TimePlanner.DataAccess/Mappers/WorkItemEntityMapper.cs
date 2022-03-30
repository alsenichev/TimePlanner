﻿using System.Text.Json;
using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Utils;

namespace TimePlanner.DataAccess.Mappers
{
  public class WorkItemEntityMapper : IWorkItemEntityMapper

  {
    private Duration Map(DurationEntity durationEntity)
    {
      return new Duration(durationEntity.DurationEntityId, DateOnly.FromDateTime(durationEntity.Date),
        durationEntity.Value);
    }

    private Category MapCategory(string categoryName)
    {
      if (Enum.TryParse(categoryName, out Category result))
      {
        return result;
      }

      throw new InvalidDataException(nameof(Category));
    }

    public DurationEntity Map(Guid workItemId, Duration duration)
    {
      return new DurationEntity
      {
        DurationEntityId = duration.id ?? 0,
        Date = duration.Date.ToDateTime(TimeOnly.MinValue),
        Value = duration.Value.Duration,
        WorkItemId = workItemId
      };
    }

    public Recurrence ExtractRecurrence(WorkItemEntity entity)
    {
      return new Recurrence(
        entity.WorkItemId,
        entity.YearsEveryN,
        ParseSqlList(entity.YearsCustom),
        entity.MonthsEveryN,
        ParseSqlList(entity.MonthsCustom),
        entity.WeeksEveryN,
        ParseSqlList(entity.WeeksCustom),
        ParseSqlList(entity.WeekDaysCustom),
        entity.DaysEveryN,
        ParseSqlList(entity.DaysCustom),
        entity.RepetitionCount,
        entity.MaxRepetitionCount,
        entity.IsAfterPreviousCompleted);
    }

    public void CopyRecurrence(WorkItemEntity source, WorkItemEntity target)
    {
      target.IsRecurrent = true;
      target.YearsEveryN = source.YearsEveryN;
      target.YearsCustom = source.YearsCustom;
      target.MonthsEveryN = source.MonthsEveryN;
      target.MonthsCustom = source.MonthsCustom;
      target.WeeksEveryN = source.WeeksEveryN;
      target.WeeksCustom = source.WeeksCustom;
      target.WeekDaysCustom = source.WeekDaysCustom;
      target.DaysEveryN = source.DaysEveryN;
      target.DaysCustom = source.DaysCustom;
      target.RepetitionCount = source.RepetitionCount;
      target.MaxRepetitionCount = source.MaxRepetitionCount;
      target.IsAfterPreviousCompleted = source.IsAfterPreviousCompleted;
    }

    public void CleanUpRecurrence(WorkItemEntity entity)
    {
      entity.IsRecurrent = false;
      entity.YearsEveryN = null;
      entity.YearsCustom = null;
      entity.MonthsEveryN = null;
      entity.MonthsCustom = null;
      entity.WeeksEveryN = null;
      entity.WeeksCustom = null;
      entity.WeekDaysCustom = null;
      entity.DaysEveryN = null;
      entity.DaysCustom = null;
      entity.RepetitionCount = null;
      entity.MaxRepetitionCount = null;
      entity.IsAfterPreviousCompleted = null;
    }

    public void AssignRecurrence(WorkItemEntity entity, Recurrence model)
    {
      entity.IsRecurrent = true;
      entity.YearsEveryN = model.YearsEveryN;
      entity.YearsCustom = model.YearsCustom != null ? string.Join(",", model.YearsCustom) : null;
      entity.MonthsEveryN = model.MonthsEveryN;
      entity.MonthsCustom = model.MonthsCustom != null ? string.Join(",", model.MonthsCustom) : null;
      entity.WeeksEveryN = model.WeeksEveryN;
      entity.WeeksCustom = model.WeeksCustom != null ? string.Join(",", model.WeeksCustom) : null;
      entity.WeekDaysCustom = model.WeekDaysCustom != null ? string.Join(",", model.WeekDaysCustom) : null;
      entity.DaysEveryN = model.DaysEveryN;
      entity.DaysCustom = model.DaysCustom != null ? string.Join(",", model.DaysCustom) : null;
      entity.RepetitionCount = model.RepetitionCount;
      entity.MaxRepetitionCount = model.MaxRepetitionCount;
      entity.IsAfterPreviousCompleted = model.IsAfterPreviousCompleted;
    }

    private List<int>? ParseSqlList(string? list)
    {
      if (list == null)
      {
        return null;
      }

      var split = list.Split(",", StringSplitOptions.RemoveEmptyEntries);
      var result = new List<int>();
      foreach (var entry in split)
      {
        if (int.TryParse(entry, out var number))
        {
          result.Add(number);
        }
      }

      return result;
    }

    public SortData MapSortData(WorkItemEntity entity)
    {
      return new SortData(entity.WorkItemId, MapCategory(entity.Category), entity.SortOrder);
    }

    public WorkItem Map(WorkItemEntity entity)
    {
      return new WorkItem(
        entity.WorkItemId,
        entity.Name,
        MapCategory(entity.Category),
        entity.CreatedAt,
        entity.CompletedAt,
        entity.NextTime,
        entity.IsRecurrent ? JsonSerializer.Serialize(ExtractRecurrence(entity)) : null,
        entity.SortOrder,
        entity.Durations.Select(d => Map(d)).ToList());
    }
  }
}
