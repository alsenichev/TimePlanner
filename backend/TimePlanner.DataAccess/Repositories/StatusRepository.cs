using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimePlanner.DataAccess.Entities;
using TimePlanner.DataAccess.Mappers;
using TimePlanner.Domain.Exceptions;
using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Models;

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
          .AsNoTracking()
          .FirstOrDefaultAsync(s => s.StatusId.Equals(statusId));
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
  }
}
