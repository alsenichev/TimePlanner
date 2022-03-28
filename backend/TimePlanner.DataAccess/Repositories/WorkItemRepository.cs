using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
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


    public WorkItemRepository(
      TimePlannerDbContext dbContext,
      IWorkItemEntityMapper workItemEntityMapper,
      ILogger<WorkItemRepository> logger)
    {
      this.dbContext = dbContext;
      this.workItemEntityMapper = workItemEntityMapper;
      this.logger = logger;
    }

    public Task<List<WorkItem>> GetWorkItemsAsync()
    {
      return dbContext
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
      // update sorting and archive
      DateTime archiveThreashold = DateTime.Now.AddDays(-30);
      var entities = dbContext.WorkItemEntities.Where(i => i.Category != Category.Archived.ToString());
      List<SortData> sortModels = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
      var sortModel = new SortData(Guid.NewGuid(), Category.Today, 0);
      Dictionary<Guid, SortData> ordered = Sorting.AddItem(sortModels, sortModel).ToDictionary(i => i.Id);
      foreach (var e in entities)
      {
        e.SortOrder = ordered[e.WorkItemId].SortOrder;
        if (e.CompletedAt.HasValue && e.CompletedAt.Value.Date < archiveThreashold.Date)
        {
          e.Category = Category.Archived.ToString();
        }
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

    public async Task<WorkItem> UpdateWorkItemAsync(WorkItem workItem)
    {
      var entities = dbContext.WorkItemEntities.Include(i => i.Durations)
        .Where(i => i.Category != Category.Archived.ToString());
      WorkItemEntity? sourceEntity = entities.FirstOrDefault(e => e.WorkItemId == workItem.Id.Value);
      if (sourceEntity == null)
      {
        throw new EntityMissingException();
      }
      WorkItem sourceModel = workItemEntityMapper.Map(sourceEntity);

      // update sorting and archive
      DateTime archiveThreashold = DateTime.Now.AddDays(-30);
      List<SortData> models = new List<SortData>();
      if (sourceModel.Category != workItem.Category)
      {
        models = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
        Dictionary<Guid, SortData> ordered = Sorting.ChangeCategory(
          models, workItem.Id.Value, workItem.Category).ToDictionary(i => i.Id);
        foreach (var e in entities)
        {
          e.SortOrder = ordered[e.WorkItemId].SortOrder;
        }

        workItem.SortOrder = ordered[workItem.Id.Value].SortOrder;
      }
      else if(sourceModel.SortOrder != workItem.SortOrder)
      {
        models = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
        Dictionary<Guid, SortData> ordered = Sorting.ChangeSortOrder(
          models,
          workItemEntityMapper.MapSortData(sourceEntity),
          workItem.SortOrder - sourceModel.SortOrder).ToDictionary(i => i.Id);
        foreach (var e in entities)
        {
          e.SortOrder = ordered[e.WorkItemId].SortOrder;
        }
        workItem.SortOrder = ordered[workItem.Id.Value].SortOrder;
      }
      foreach (var e in entities)
      {
        if (e.CompletedAt.HasValue && e.CompletedAt.Value.Date < archiveThreashold.Date)
        {
          e.Category = Category.Archived.ToString();
        }
      }
      try
      {
        dbContext.UpdateRange(entities);
        dbContext.Update(workItemEntityMapper.UpdateFrom(workItem, sourceEntity));
        await dbContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to update the work item.");

        throw new DataAccessException();
      }

      return await GetWorkItemAsync(sourceEntity.WorkItemId);
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
