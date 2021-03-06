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

    public async Task<List<WorkItem>> GetWorkItemsAsync()
    {
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

    public async Task<WorkItem> CreateWorkItemAsync(string name, int sortOrder, Dictionary<Guid, SortData> sortData)
    {
      var entities = dbContext.WorkItemEntities.Where(i => i.Category != Category.Archived.ToString());
      DateTime archiveThreashold = DateTime.UtcNow.AddDays(-30);
      foreach (var e in entities)
      {
        e.SortOrder = sortData[e.WorkItemId].SortOrder;
        if (e.CompletedAt.HasValue && e.CompletedAt.Value.Date < archiveThreashold.Date)
        {
          e.Category = Category.Archived.ToString();
        }
      }

      var entity = new WorkItemEntity
      {
        Category = Category.Today.ToString(),
        CreatedAt = DateTime.UtcNow,
        Name = name,
        SortOrder = sortOrder
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
      WorkItem workItem,
      Dictionary<Guid, SortData>? sortData,
      WorkItem? repeatedWorkItem)
    {
      IQueryable<WorkItemEntity> entities = dbContext.WorkItemEntities
        .Where(i => i.Category != Category.Archived.ToString());
      WorkItemEntity? entity = entities.FirstOrDefault(e => e.WorkItemId == workItem.Id);

      if (entity == null)
      {
        throw new EntityMissingException();
      }

      workItemEntityMapper.UpdateEntity(entity, workItem);
      if (sortData != null)
      {
        foreach (var e in entities)
        {
          e.SortOrder = sortData[e.WorkItemId].SortOrder;
        }
        dbContext.UpdateRange(entities);
      }
      else
      {
        dbContext.Update(entity);
      }
      if (repeatedWorkItem != null)
      {
        await dbContext.AddAsync(workItemEntityMapper.CreateEntity(repeatedWorkItem.Value));
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

    public async Task UpdateWorkItemsAsync(
      Dictionary<Guid, (int, Category, DateTime?)> updateData,
      List<WorkItem> addedWorkItems)
    {
      IQueryable<WorkItemEntity> entities = dbContext.WorkItemEntities
        .Where(i => i.Category != Category.Archived.ToString());

      foreach (var e in entities)
      {
        if (updateData.TryGetValue(e.WorkItemId, out var data))
        {
          var (sortOrder, category, nextTime) = data;
          e.SortOrder = sortOrder;
          e.Category = category.ToString();
          e.NextTime = nextTime;
        }
      }
      dbContext.UpdateRange(entities);
      foreach(var workItem in addedWorkItems)
      {
        await dbContext.AddAsync(workItemEntityMapper.CreateEntity(workItem));
      }
      try
      {
        await dbContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to update work items.");

        throw new DataAccessException();
      }
    }
      

    public async Task DeleteWorkItemAsync(Guid workItemId, Dictionary<Guid, SortData> sortData)
    {
      var entities = dbContext.WorkItemEntities.Include(i => i.Durations)
        .Where(i => i.Category != Category.Archived.ToString());
      WorkItemEntity? entity = entities.FirstOrDefault(e => e.WorkItemId == workItemId);
      if (entity == null)
      {
        throw new EntityMissingException();
      }

      foreach (var e in entities.Where(e => e.WorkItemId != workItemId))
      {
        e.SortOrder = sortData[e.WorkItemId].SortOrder;
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
