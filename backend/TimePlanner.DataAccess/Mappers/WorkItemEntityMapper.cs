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
      target.RecurrenceStartsOn = source.RecurrenceStartsOn;
      target.RecurrenceEndsOn = source.RecurrenceEndsOn;
      target.RepetitionsCount = source.RepetitionsCount;
      target.MaxRepetitionsCount = source.MaxRepetitionsCount;
      target.IsIfPreviousCompleted = source.IsIfPreviousCompleted;
    }

    public void CleanUpRecurrence(WorkItemEntity entity)
    {
      entity.CronExpression = null;
      entity.RecurrenceStartsOn = null;
      entity.RecurrenceEndsOn= null;
      entity.RepetitionsCount = null;
      entity.MaxRepetitionsCount = null;
      entity.IsIfPreviousCompleted = null;
      entity.IsOnPause = null;
    }

    public void AssignRecurrence(
      WorkItemEntity entity,
      string cronExpression,
      DateTime? recurrenceStartsOn,
      DateTime? recurrenceEndsOn,
      bool? isAfterPreviousCompleted,
      int? maxRepetitionsCount,
      bool? isOnPause)
    {
      entity.CronExpression = cronExpression;
      entity.RecurrenceStartsOn = recurrenceStartsOn;
      entity.RecurrenceEndsOn = recurrenceEndsOn;
      entity.MaxRepetitionsCount = maxRepetitionsCount;
      entity.IsIfPreviousCompleted = isAfterPreviousCompleted;
      entity.IsOnPause = isOnPause;
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
        entity.RecurrenceStartsOn,
        entity.RecurrenceEndsOn,
        entity.IsIfPreviousCompleted,
        entity.MaxRepetitionsCount,
        entity.IsOnPause,
        entity.SortOrder,
        entity.Durations.Select(d => Map(d)).ToList());
    }
  }
}
