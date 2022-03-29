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

    public Recurrence Map(RecurrenceEntity entity)
    {
      return new Recurrence(
        entity.WorkItemId,
        entity.YearsEveryN,
        entity.YearsCustom,
        entity.MonthsEveryN,
        entity.MonthsCustom,
        entity.WeeksEveryN,
        entity.WeeksCustom,
        entity.DaysEveryN,
        entity.DaysCustom,
        entity.IsAfterPreviousCompleted);
    }

    public RecurrenceEntity Map(Recurrence model)
    {
      return new RecurrenceEntity
      {
        WorkItemId = model.WorkItemId,
        YearsEveryN = model.YearsEveryN,
        YearsCustom = model.YearsCustom,
        MonthsEveryN = model.MonthsEveryN,
        MonthsCustom = model.MonthsCustom,
        WeeksEveryN = model.WeeksEveryN,
        WeeksCustom = model.WeeksCustom,
        DaysEveryN = model.DaysEveryN,
        DaysCustom = model.DaysCustom,
        IsAfterPreviousCompleted = model.IsAfterPreviousCompleted
      };
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
        entity.Recurrence != null ? JsonSerializer.Serialize(entity.Recurrence!) : null,
        entity.SortOrder,
        entity.Durations.Select(d => Map(d)).ToList());
    }

    public WorkItemEntity Map(WorkItem workItem)
    {
      return new WorkItemEntity
      {
        WorkItemId = workItem.Id ?? Guid.Empty,
        Name = workItem.Name,
        Category = workItem.Category.ToString(),
        CompletedAt = workItem.CompletedAt,
        CreatedAt = workItem.CreatedAt,
        Durations = workItem.Durations.Select(d => Map(workItem.Id ?? Guid.Empty, d)).ToList(),
        NextTime = workItem.NextTime,
        SortOrder = workItem.SortOrder
      };
    }
  }
}
