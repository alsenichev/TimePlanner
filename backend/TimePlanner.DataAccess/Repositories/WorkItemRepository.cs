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

    private async Task UpdateArchivedAndRepeating()
    {
      var entities = dbContext.WorkItemEntities.Where(i => i.Category != Category.Archived.ToString());
      
      var archived = UpdateArchived(entities);

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
          if (!entity.Recurrence.IsAfterPreviousCompleted)
          {
            entity.Recurrence = null;
            var newEntity = new WorkItemEntity
            {
              Category = Category.Scheduled.ToString(),
              CreatedAt = DateTime.Now,
              Name = entity.Name,
              SortOrder = int.MaxValue,
              Recurrence = entity.Recurrence,
              NextTime = CalculateNextTime(entity.Recurrence!)
            };
            dbContext.Add(newEntity);
          }
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

    private static List<WorkItemEntity> UpdateArchived(IQueryable<WorkItemEntity> entities)
    {
      DateTime archiveThreshold = DateTime.Now.AddDays(-30);
      var archive = entities.Where(e => e.CompletedAt.HasValue && e.CompletedAt.Value.Date < archiveThreshold.Date)
        .ToList();
      foreach (var entity in archive)
      {
        entity.Category = Category.Archived.ToString();
      }

      return archive;
    }

    private DateTime? CalculateNextTime(RecurrenceEntity recurrence)
    {
      DateTime baseDate = DateTime.Now;
      if (recurrence.DaysEveryN.HasValue)
      {
        return baseDate.AddDays(recurrence.DaysEveryN.Value);
      }

      if (recurrence.DaysCustom != null && recurrence.DaysCustom.Count > 0)
      {
        int newDay = recurrence.DaysCustom.FirstOrDefault(d => d > baseDate.Day);
        if (newDay == 0)
        {
          return new DateTime(baseDate.Year, baseDate.Month, 1).AddMonths(1).AddDays(recurrence.DaysCustom[0]);
        }
      }

      throw new ApplicationException("The recurrence value can not be correctly processed.");
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
      string recurrence,
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

      List<SortData> models = new List<SortData>();
      if (entity.Recurrence != null)
      {
        if (targetCategory == Category.Completed)
        {
          entity.CompletedAt = DateTime.Now;
          if (entity.Recurrence != null)
          {
            entity.Recurrence = null;
            var newEntity = new WorkItemEntity
            {
              Category = Category.Scheduled.ToString(),
              CreatedAt = DateTime.Now,
              Name = entity.Name,
              SortOrder = int.MaxValue,
              Recurrence = entity.Recurrence,
              NextTime = CalculateNextTime(entity.Recurrence!)
            };
            dbContext.Add(newEntity);
          }
        }
        if (string.IsNullOrEmpty(recurrence))
        {
          entity.NextTime = null;
          targetCategory = Category.Today;
        }
      }
      {
        entity.Recurrence = workItemEntityMapper.ParseRecurrence(entity.WorkItemId, recurrence);
        entity.NextTime = CalculateNextTime(entity.Recurrence!);
        // TODO restore in Today if recurrence is set to null
      }
      if (entity.Category != targetCategory.ToString())
      {
        models = entities.Select(e => workItemEntityMapper.MapSortData(e)).ToList();
        Dictionary<Guid, SortData> ordered = Sorting.ChangeCategory(
          models, workItemId, targetCategory).ToDictionary(i => i.Id);
        foreach (var e in entities)
        {
          e.SortOrder = ordered[e.WorkItemId].SortOrder;
        }

        if (entity.Category.Equals(Category.Completed.ToString()))
        {
          entity.CompletedAt = null;
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
      // update sorting
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
