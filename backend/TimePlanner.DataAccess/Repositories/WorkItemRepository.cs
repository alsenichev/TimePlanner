﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimePlanner.DataAccess.Entities;
using TimePlanner.DataAccess.Mappers;
using TimePlanner.Domain.Exceptions;
using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Utils;

namespace TimePlanner.DataAccess.Repositories
{
  public class WorkItemRepository : IWorkItemRepository
  {
    private readonly TimePlannerDbContext dbContext;
    private readonly IWorkItemEntityMapper workItemEntityMapper;
    private readonly ILogger<WorkItemRepository> logger;

    private async Task UpdateArchivedAndRepeating()
    {
      var entities = dbContext.WorkItemEntities
        .Where(i => i.Category != Category.Archived.ToString());
      DateTime archiveThreshold = DateTime.Now.AddDays(-30);
      var archive = entities.Where(e => e.CompletedAt.HasValue && e.CompletedAt.Value.Date < archiveThreshold.Date)
        .ToList();
      foreach (var entity in archive)
      {
        entity.Category = Category.Archived.ToString();
      }

      var awaken = entities.Where(e =>
          e.Category == Category.Scheduled.ToString() &&
          e.NextTime.HasValue &&
          e.NextTime.Value <= DateTime.Now)
        .OrderByDescending(e => e.NextTime.Value).ToList();

      if (awaken.Count > 0)
      {
        List<SortData> sort = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
        foreach (var entity in awaken)
        {
          sort = Sorting.ChangeCategory(sort, entity.WorkItemId, Category.Today).ToList();
        }

        var ids = sort.ToDictionary(i => i.Id);
        foreach (var entity in entities)
        {
          entity.SortOrder = ids[entity.WorkItemId].SortOrder;
        }

        foreach (var entity in awaken)
        {
          //TODO - if strict create new entity and reset recurrence
          entity.NextTime = CalclateNextTime(entity);
          entity.Category = Category.Today.ToString();
        }
      }

      try
      {
        dbContext.UpdateRange(archive);
        dbContext.UpdateRange(awaken);
        await dbContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to update archived/awaken work items.");

        throw new DataAccessException();
      }
    }

    private DateTime? CalclateNextTime(WorkItemEntity entity)
    {
      return DateTime.Now.AddDays(1);
      // TODO read entity's recurrence formula and use RecurrenceService
    }

    public WorkItemRepository(
      TimePlannerDbContext dbContext,
      IWorkItemEntityMapper workItemEntityMapper,
      ILogger<WorkItemRepository> logger)
    {
      this.dbContext = dbContext;
      this.workItemEntityMapper = workItemEntityMapper;
      this.logger = logger;
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
      Dictionary<Guid, SortData> ordered = Sorting.AddItem(sortModels, sortModel).ToDictionary(i => i.Id);
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
      List<Duration> durations)
    {
      var entities = dbContext.WorkItemEntities.Include(i => i.Durations)
        .Where(i => i.Category != Category.Archived.ToString());
      WorkItemEntity? entity = entities.FirstOrDefault(e => e.WorkItemId == workItemId);
      if (entity == null)
      {
        throw new EntityMissingException();
      }

      entity.Name = name;
      entity.Durations = durations.Select(d => workItemEntityMapper.Map(workItemId, d)).ToList();

      // update sorting
      List<SortData> models = new List<SortData>();
      if (entity.Category != targetCategory.ToString())
      {
        models = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
        Dictionary<Guid, SortData> ordered = Sorting.ChangeCategory(
          models, workItemId, targetCategory).ToDictionary(i => i.Id);
        foreach (var e in entities)
        {
          e.SortOrder = ordered[e.WorkItemId].SortOrder;
        }

        if (targetCategory == Category.Completed)
        {
          entity.CompletedAt = DateTime.Now;
          // TODO - if work item is recurrent create a new one and copy recurrence, reset recurrence at this one.
        }

        if (entity.Category.Equals(Category.Completed.ToString()))
        {
          entity.CompletedAt = null;
        }

        if (targetCategory == Category.Scheduled)
        {
          entity.NextTime = CalclateNextTime(entity);
        }

        if (entity.Category.Equals(Category.Scheduled.ToString()))
        {
          entity.NextTime = null;
        }

        entity.Category = targetCategory.ToString();
      }
      else if(entity.SortOrder != sortOrder)
      {
        models = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
        Dictionary<Guid, SortData> ordered = Sorting.ChangeSortOrder(
          models,
          workItemEntityMapper.MapSortData(entity),
          sortOrder - entity.SortOrder).ToDictionary(i => i.Id);
        foreach (var e in entities)
        {
          e.SortOrder = ordered[e.WorkItemId].SortOrder;
        }
      }

      try
      {
        dbContext.UpdateRange(entities);
        dbContext.Update(entity);
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
      // update sorting and archive
      DateTime archiveThreashold = DateTime.Now.AddDays(-30);
      List<SortData> sortModels = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
      Dictionary<Guid, SortData> ordered = Sorting.DeleteItem(sortModels, workItemId).ToDictionary(i => i.Id);
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
