using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Utils;

namespace TimePlanner.DataAccess.Mappers
{
  public class WorkItemEntityMapper : IWorkItemEntityMapper

  {
    private Duration Map(DurationEntity durationEntity)
    {
      return new Duration(durationEntity.DurationEntityId, DateOnly.FromDateTime(durationEntity.Date), durationEntity.Value);
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

    public SortData MapSortData(WorkItemEntity entity)
    {
      return new SortData(entity.WorkItemId, MapCategory(entity.Category), entity.SortOrder);
    }

    public WorkItem Map(WorkItemEntity entity)
    {
      return new WorkItem(
        entity.WorkItemId,
        entity.Name,
        entity.Durations?.Select(d => Map(d)).ToList()?? new List<Duration>(),
        MapCategory(entity.Category),
        entity.WakingUpWhen.HasValue && entity.WakingUpWhere != null
          ? new WakingUp(DateOnly.FromDateTime(entity.WakingUpWhen.Value), MapCategory(entity.WakingUpWhere))
          : null,
        entity.RecurrenceDays.HasValue ? new Recurrence(entity.RecurrenceDays.Value) : null,
        new History(entity.CreatedAt, entity.CompletedAt),
        entity.SortOrder);
    }

    public WorkItemEntity Map(WorkItem workItem)
    {
      return new WorkItemEntity
      {
        WorkItemId = workItem.Id ?? Guid.Empty,
        Name = workItem.Name,
        Category = workItem.Category.ToString(),
        CompletedAt = workItem.History.CompletedAt,
        CreatedAt = workItem.History.CreatedAt,
        Durations = workItem.Durations.Select(d => Map(workItem.Id ?? Guid.Empty, d)).ToList(),
        RecurrenceDays = workItem.Recurrence?.Days,
        WakingUpWhen = workItem.WakingUp?.When.ToDateTime(TimeOnly.MinValue),
        WakingUpWhere = workItem.WakingUp?.Where.ToString(),
        SortOrder = workItem.SortOrder
      };
    }

    public WorkItemEntity UpdateFrom(WorkItem workItem, WorkItemEntity source)
    {
      source.Name = workItem.Name;
      source.Category = workItem.Category.ToString();
      source.CompletedAt = workItem.History.CompletedAt;
      source.CreatedAt = workItem.History.CreatedAt;
      source.Durations = workItem.Durations.Select(d => Map(workItem.Id ?? Guid.Empty, d)).ToList();
      source.RecurrenceDays = workItem.Recurrence?.Days;
      source.WakingUpWhen = workItem.WakingUp?.When.ToDateTime(TimeOnly.MinValue);
      source.WakingUpWhere = workItem.WakingUp?.Where.ToString();
      source.SortOrder = workItem.SortOrder;
      return source;
    }
  }
}
