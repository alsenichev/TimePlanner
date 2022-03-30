using System.Text.Json;
using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;
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
        entity.RepetitionStartDate,
        entity.Cron,
        entity.RepetitionCount,
        entity.MaxRepetitionCount,
        entity.IsAfterPreviousCompleted);
    }

    public void CopyRecurrence(WorkItemEntity source, WorkItemEntity target)
    {
      target.Cron = source.Cron;
      target.RepetitionStartDate = source.RepetitionStartDate;
      target.RepetitionCount = source.RepetitionCount;
      target.MaxRepetitionCount = source.MaxRepetitionCount;
      target.IsAfterPreviousCompleted = source.IsAfterPreviousCompleted;
    }

    public void CleanUpRecurrence(WorkItemEntity entity)
    {
      entity.Cron = null;
      entity.RepetitionStartDate = null;
      entity.RepetitionCount = null;
      entity.MaxRepetitionCount = null;
      entity.IsAfterPreviousCompleted = null;
    }

    public void AssignRecurrence(WorkItemEntity entity, Recurrence model)
    {
      entity.Cron = model.Cron;
      entity.RepetitionStartDate = model.RepetitionStartDate;
      entity.RepetitionCount = model.RepetitionCount;
      entity.MaxRepetitionCount = model.MaxRepetitionCount;
      entity.IsAfterPreviousCompleted = model.IsAfterPreviousCompleted;
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
        entity.RepetitionStartDate.HasValue ? JsonSerializer.Serialize(ExtractRecurrence(entity)) : null,
        entity.SortOrder,
        entity.Durations.Select(d => Map(d)).ToList());
    }
  }
}
