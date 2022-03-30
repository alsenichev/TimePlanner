﻿using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimePlanner.DataAccess.Entities;
using TimePlanner.DataAccess.Mappers;
using TimePlanner.Domain.Exceptions;
using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;

namespace TimePlanner.DataAccess.Repositories
{
  public class WorkItemRepository : IWorkItemRepository
  {
    private readonly TimePlannerDbContext dbContext;
    private readonly IWorkItemEntityMapper workItemEntityMapper;
    private readonly IRecurrenceService recurrenceService;
    private readonly ILogger<WorkItemRepository> logger;

    private async Task UpdateArchivedAndRepeating()
    {
      var entities = dbContext.WorkItemEntities.Where(i => i.Category != Category.Archived.ToString());
      
      var archived = UpdateArchived(entities);

      var awaken = entities.Where(e =>
          e.Category == Category.Scheduled.ToString() &&
          e.NextTime.HasValue &&
          e.NextTime.Value.Date <= DateTime.Now.Date)
        .OrderByDescending(e => e.NextTime.Value).ToList();

      if (awaken.Count > 0)
      {
        List<SortData> sort = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
        foreach (var entity in awaken)
        {
          sort = SortingService.ChangeCategory(sort, entity.WorkItemId, Category.Today).ToList();
        }

        var ids = sort.ToDictionary(i => i.Id);
        foreach (var entity in entities)
        {
          entity.SortOrder = ids[entity.WorkItemId].SortOrder;
        }

        foreach (var entity in awaken)
        {
          CreateNextRecurrentWorkItemInstance(entity);
          entity.NextTime = null;
          entity.Category = Category.Today.ToString();
        }
      }

      try
      {
        dbContext.UpdateRange(archived);
        dbContext.UpdateRange(awaken);
        await dbContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to update archived/awaken work items.");

        throw new DataAccessException();
      }
    }

    private void CreateNextRecurrentWorkItemInstance(WorkItemEntity entity)
    {
      if (entity.RepetitionCount.HasValue && entity.MaxRepetitionCount.HasValue &&
          entity.RepetitionCount.Value == entity.MaxRepetitionCount.Value)
      {
        // no more repetitions for this work item
        workItemEntityMapper.CleanUpRecurrence(entity);
        return;
      }

      if (entity.IsAfterPreviousCompleted.HasValue && entity.IsAfterPreviousCompleted.Value)
      {
        // new item will be created after this one is complete
        return;
      }

      // create new item
      var newEntity = new WorkItemEntity
      {
        Category = Category.Scheduled.ToString(),
        CreatedAt = DateTime.Now,
        Name = entity.Name,
        SortOrder = int.MaxValue,
        NextTime = recurrenceService.CalculateNextTime(workItemEntityMapper.ExtractRecurrence(entity))
      };
      workItemEntityMapper.CopyRecurrence(entity, newEntity);
      if (newEntity.RepetitionCount.HasValue)
      {
        newEntity.RepetitionCount++;
      }
      workItemEntityMapper.CleanUpRecurrence(entity);
      dbContext.Add(newEntity);
    }

    private static List<WorkItemEntity> UpdateArchived(IQueryable<WorkItemEntity> entities)
    {
      DateTime archiveThreshold = DateTime.Now.AddDays(-7);
      var archive = entities.Where(e => e.CompletedAt.HasValue && e.CompletedAt.Value.Date < archiveThreshold.Date)
        .ToList();
      foreach (var entity in archive)
      {
        entity.Category = Category.Archived.ToString();
      }

      return archive;
    }

    private void UpdateName(string name, WorkItemEntity entity)
    {
      entity.Name = name;
      dbContext.Update(entity);
    }

    private void UpdateCategory(IQueryable<WorkItemEntity> entities, WorkItemEntity entity, Category targetCategory)
    {
      SortForCategoryChange(entities, entity.WorkItemId, targetCategory);

      if (targetCategory == Category.Completed)
      {
        entity.CompletedAt = DateTime.Now;
        if (entity.RepetitionStartDate.HasValue)
        {
          var newEntity = new WorkItemEntity
          {
            Category = Category.Scheduled.ToString(),
            CreatedAt = DateTime.Now,
            Name = entity.Name,
            SortOrder = int.MaxValue,
            NextTime = recurrenceService.CalculateNextTime(workItemEntityMapper.ExtractRecurrence(entity))
          };
          workItemEntityMapper.CopyRecurrence(entity, newEntity);
          if (newEntity.RepetitionCount.HasValue)
          {
            newEntity.RepetitionCount++;
          }
          workItemEntityMapper.CleanUpRecurrence(entity);
          dbContext.Add(newEntity);
        }
      }
      entity.Category = targetCategory.ToString();
      dbContext.UpdateRange(entities);
    }

    private void UpdateSortOrder(IQueryable<WorkItemEntity> entities, WorkItemEntity entity, int sortOrder)
    {
      List<SortData> sortData = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
      Dictionary<Guid, SortData> ordered = SortingService.ChangeSortOrder(
        sortData,
        workItemEntityMapper.MapSortData(entity),
        sortOrder - entity.SortOrder).ToDictionary(i => i.Id);
      foreach (var e in entities)
      {
        e.SortOrder = ordered[e.WorkItemId].SortOrder;
      }
      dbContext.UpdateRange(entities);
    }

    private void UpdateRecurrence(IQueryable<WorkItemEntity> entities, WorkItemEntity entity, string recurrence)
    {
      if (!entity.RepetitionStartDate.HasValue)
      {
        // assign recurrence
        Recurrence targetRecurrence = JsonSerializer.Deserialize<Recurrence>(recurrence);
        workItemEntityMapper.AssignRecurrence(entity, targetRecurrence);
        entity.Category = Category.Scheduled.ToString();
        entity.NextTime = recurrenceService.CalculateNextTime(targetRecurrence);

        SortForCategoryChange(entities, entity.WorkItemId, Category.Scheduled);
        dbContext.UpdateRange(entities);
      }
      else if (string.IsNullOrEmpty(recurrence))
      {
        // reset existing recurrence
        workItemEntityMapper.CleanUpRecurrence(entity);
        entity.Category = Category.Today.ToString();
        entity.NextTime = null;

        SortForCategoryChange(entities, entity.WorkItemId, Category.Today);
        dbContext.UpdateRange(entities);
      }
      else
      {
        // replace existing recurrence
        Recurrence target = JsonSerializer.Deserialize<Recurrence>(recurrence);
        workItemEntityMapper.AssignRecurrence(entity, target);
        entity.Category = Category.Scheduled.ToString();
        entity.NextTime = recurrenceService.CalculateNextTime(target);

        dbContext.Update(entity);
      }
    }

    private void SortForCategoryChange(IQueryable<WorkItemEntity> entities, Guid workItemId, Category targetCategory)
    {
      // sort
      var models = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
      Dictionary<Guid, SortData> ordered = SortingService.ChangeCategory(
        models, workItemId, targetCategory).ToDictionary(i => i.Id);
      foreach (var e in entities)
      {
        e.SortOrder = ordered[e.WorkItemId].SortOrder;
      }
    }

    public WorkItemRepository(
      TimePlannerDbContext dbContext,
      IWorkItemEntityMapper workItemEntityMapper,
      ILogger<WorkItemRepository> logger, IRecurrenceService recurrenceService)
    {
      this.dbContext = dbContext;
      this.workItemEntityMapper = workItemEntityMapper;
      this.logger = logger;
      this.recurrenceService = recurrenceService;
    }

    public async Task<List<WorkItem>> GetWorkItemsAsync()
    {
      await UpdateArchivedAndRepeating();

      return await dbContext
        .WorkItemEntities
        .Where(i => i.Category != Category.Archived.ToString())
        .Include(w => w.Durations)
        .OrderBy(e => e.SortOrder)
        .AsNoTracking()
        .Select(e => workItemEntityMapper.Map(e))
        .ToListAsync();
    }

    public async Task<WorkItem> GetWorkItemAsync(Guid workItemId)
    {
      WorkItemEntity? entity;
      try
      {
        entity = await dbContext
          .WorkItemEntities
          .Include(w => w.Durations)
          .AsNoTracking()
          .FirstOrDefaultAsync(s => s.WorkItemId.Equals(workItemId));
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to get work items by id {workItemId}.", workItemId);

        throw new DataAccessException();
      }

      if (entity is null)
      {
        throw new EntityMissingException();
      }

      return workItemEntityMapper.Map(entity);
    }

    public async Task<WorkItem> CreateWorkItemAsync(string name)
    {
      // update sorting
      var entities = dbContext.WorkItemEntities.Where(i => i.Category != Category.Archived.ToString());
      List<SortData> sortModels = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
      var sortModel = new SortData(Guid.NewGuid(), Category.Today, 0);
      Dictionary<Guid, SortData> ordered = SortingService.AddItem(sortModels, sortModel).ToDictionary(i => i.Id);
      foreach (var e in entities)
      {
        e.SortOrder = ordered[e.WorkItemId].SortOrder;
      }
      
      var entity = new WorkItemEntity
      {
        Category = Category.Today.ToString(),
        CreatedAt = DateTime.Now,
        Name = name,
        SortOrder = ordered[sortModel.Id].SortOrder
      };
      try
      {
        dbContext.UpdateRange(entities);
        await dbContext.AddAsync(entity);
        await dbContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to create the work item.");

        throw new DataAccessException();
      }

      return await GetWorkItemAsync(entity.WorkItemId);
    }

    public async Task<WorkItem> UpdateWorkItemAsync(
      Guid workItemId,
      string name,
      Category targetCategory,
      int sortOrder,
      string recurrence)
    {
      IQueryable<WorkItemEntity> entities = dbContext.WorkItemEntities
        .Where(i => i.Category != Category.Archived.ToString());
      WorkItemEntity? entity = entities.FirstOrDefault(e => e.WorkItemId == workItemId);
      if (entity == null)
      {
        throw new EntityMissingException();
      }

      if (!entity.Name.Equals(name))
      {
        UpdateName(name, entity);
      }
      else if (entity.SortOrder != sortOrder)
      {
        UpdateSortOrder(entities, entity, sortOrder);
      }
      else if (!entity.Category.Equals(targetCategory.ToString()))
      {
        UpdateCategory(entities, entity, targetCategory);
      }
      else if (entity.RepetitionStartDate.HasValue || !string.IsNullOrEmpty(recurrence))
      {
        UpdateRecurrence(entities, entity, recurrence);
      }
      else
      {
        return await GetWorkItemAsync(entity.WorkItemId);
      }

      try
      {
        await dbContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to update the work item.");

        throw new DataAccessException();
      }

      return await GetWorkItemAsync(entity.WorkItemId);
    }

    public async Task DeleteWorkItemAsync(Guid workItemId)
    {
      var entities = dbContext.WorkItemEntities.Include(i => i.Durations).Where(i => i.Category != Category.Archived.ToString());
      WorkItemEntity? entity = entities.FirstOrDefault(e => e.WorkItemId == workItemId);
      if (entity == null)
      {
        throw new EntityMissingException();
      }
      // update sorting
      DateTime archiveThreashold = DateTime.Now.AddDays(-30);
      List<SortData> sortModels = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
      Dictionary<Guid, SortData> ordered = SortingService.DeleteItem(sortModels, workItemId).ToDictionary(i => i.Id);
      foreach (var e in entities.Where(e => e.WorkItemId != workItemId))
      {
        e.SortOrder = ordered[e.WorkItemId].SortOrder;
        if (e.CompletedAt.HasValue && e.CompletedAt.Value.Date < archiveThreashold.Date)
        {
          e.Category = Category.Archived.ToString();
        }
      }

      try
      {
        dbContext.UpdateRange(entities);
        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync();
      }
      catch (Exception e)
      {
        logger.LogError(e, "Failed to delete the work item with id {workItemId}.", workItemId);

        throw new DataAccessException();
      }
    }
  }
}
