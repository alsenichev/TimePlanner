﻿using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;

namespace TimePlanner.DataAccess.Mappers
{
  public interface IWorkItemEntityMapper
  {
    WorkItem Map(WorkItemEntity entity);

    SortData MapSortData(WorkItemEntity entity);

    void CopyRecurrence(WorkItemEntity source, WorkItemEntity target);

    void CleanUpRecurrence(WorkItemEntity entity);

    void AssignRecurrence(
      WorkItemEntity source,
      string cronExpression,
      DateTime? recurrenceStartsOn,
      DateTime? recurrenceEndsOn,
      bool? isAfterPreviousCompleted,
      int? maxRepetitionsCount,
      bool? isOnPause);
  }
}
