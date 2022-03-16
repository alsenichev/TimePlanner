using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimePlanner.DataAccess.Entities;
using TimePlanner.DataAccess.Mappers;
using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.Domain.Exceptions;
using TimePlanner.Domain.Services.Interfaces;

namespace TimePlanner.DataAccess.Repositories
{
  public class StatusRepository : IStatusRepository
  {
    private readonly TimePlannerDbContext dbContext;
    private readonly IStatusEntityMapper statusEntityMapper;
    private readonly ILogger<StatusRepository> logger;

    public StatusRepository(
      TimePlannerDbContext dbContext,
      IStatusEntityMapper statusEntityMapper,
      ILogger<StatusRepository> logger)
    {
      this.dbContext = dbContext;
      this.statusEntityMapper = statusEntityMapper;
      this.logger = logger;
    }

    public async Task<Status> GetStatusAsync(Guid statusId)
    {
      StatusEntity? entity;
      try
      {
        entity = await dbContext
          .StatusEntities
          .Include(s => s.WorkItems)
          .AsNoTracking()
          .FirstOrDefaultAsync(s => s.StatusEntityId.Equals(statusId));
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to get status by id {statusId}.", statusId);

        throw new DataAccessException();
      }

      if (entity is null)
      {
        throw new EntityMissingException();
      }

      return statusEntityMapper.Map(entity);
    }

    public Task<List<Status>> GetStatusesAsync(int count)
    {
      return dbContext
        .StatusEntities
        .Include(s => s.WorkItems)
        .AsNoTracking()
        .OrderByDescending(s => s.StartedAt)
        .Take(count)
        .Select(e => statusEntityMapper.Map(e))
        .ToListAsync();
    }

    public async Task<Status> SaveStatusAsync(Status status)
    {
      var entity = statusEntityMapper.Map(status);
      try
      {
        if (status.Id.HasValue)
        {
          dbContext.Update(statusEntityMapper.Map(status));
        }
        else
        {
          await dbContext.AddAsync(entity);
        }

        await dbContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to update the status.");

        throw new DataAccessException();
      }

      if (status.Id.HasValue)
      {
        return await GetStatusAsync(status.Id.Value);
      }
      List<Status> result = await GetStatusesAsync(1);
      if (result.Count == 0 || result[0].StartedAt != status.StartedAt)
      {
        throw new EntityMissingException();
      }

      return result[0];
    }

    public async Task DeleteWorkItemAsync(Guid statusId, Guid workItemId)
    {
      var entity = new WorkItemEntity { WorkItemEntityId = workItemId };
      try
      {
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
