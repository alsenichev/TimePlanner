using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;

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

    public void CopyRecurrence(WorkItemEntity source, WorkItemEntity target)
    {
      target.CronExpression = source.CronExpression;
      target.RecurrenceStartsFrom = source.RecurrenceStartsFrom;
      target.RepetitionCount = source.RepetitionCount;
      target.MaxRepetitionCount = source.MaxRepetitionCount;
      target.IsAfterPreviousCompleted = source.IsAfterPreviousCompleted;
    }

    public void CleanUpRecurrence(WorkItemEntity entity)
    {
      entity.CronExpression = null;
      entity.RecurrenceStartsFrom = null;
      entity.RepetitionCount = null;
      entity.MaxRepetitionCount = null;
      entity.IsAfterPreviousCompleted = null;
    }

    public void AssignRecurrence(
      WorkItemEntity entity,
      string cronExpression,
      bool? isAfterPreviousCompleted,
      DateTime? recurrenceStartsFrom,
      int? maxRepetitionsCount)
    {
      entity.CronExpression = cronExpression;
      entity.RecurrenceStartsFrom = recurrenceStartsFrom;
      entity.MaxRepetitionCount = maxRepetitionsCount;
      entity.IsAfterPreviousCompleted = isAfterPreviousCompleted;
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
        entity.CronExpression,
        entity.RecurrenceStartsFrom,
        entity.IsAfterPreviousCompleted,
        entity.MaxRepetitionCount,
        entity.RepetitionCount,
        entity.SortOrder,
        entity.Durations.Select(d => Map(d)).ToList());
    }
  }
}
