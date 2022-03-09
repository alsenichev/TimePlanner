using System.Net;
using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Services.Interfaces;

namespace TimePlanner.WebApi.Services
{
  public class StatusService : IStatusService
  {
    private readonly IStatusRepository statusRepository;
    private readonly ILogger<StatusService> logger;
    private static readonly TimeSpan workingDayNormalDuration = TimeSpan.FromHours(8);

    private async Task<Status> CreateStatusIfNotExists(Status? status, DateTime dateTime)
    {
      if (status.HasValue)
      {
        return status.Value;
      }

      var prev = await statusRepository.GetPreviousStatusAsync(DateOnly.FromDateTime(dateTime));
      var deposit = prev.HasValue ? prev.Value.Deposit : TimeSpan.Zero;
      StatusBuilder statusBuilder;
      try
      {
        statusBuilder = StatusBuilder.Of(deposit, dateTime);
      }
      catch (SegmentOverflowException)
      {
        throw new StatusException($"The deposit value {deposit.Duration} is out of the acceptable range.");
      }

      var newStatus = statusBuilder.Build();
      await SaveStatusAsync(newStatus);
      return newStatus;
    }

    private async Task SaveStatusAsync(Status newStatus)
    {
      try
      {
        await statusRepository.SaveStatusAsync(newStatus);
      }
      catch (DataAccessException e)
      {
        logger.LogError(e, e.Message);

        throw new StatusException("Failed to save changes.");
      }
    }

    private async Task<Status> GetExistingStatusAsync(DateTime dateTime)
    {
      var dateOnly = DateOnly.FromDateTime(dateTime);
      Status? maybeStatus;

      try
      {
        maybeStatus = await statusRepository.GetStatusAsync(dateOnly);
      }
      catch (DataAccessException e)
      {
        logger.LogError(e, e.Message);

        throw new StatusException("Failed to read the existing status from the database.");
      }

      if (!maybeStatus.HasValue)
      {
        throw new StatusException(
          $"Status for {dateOnly} does not exist. Use GET/status endpoint to create one.",
          HttpStatusCode.BadRequest);
      }

      return maybeStatus.Value;
    }

    private StatusBuilder CreateStatusBuilder(Status status)
    {
      try
      {
        return StatusBuilder.CreateStatusBuilder(status);
      }
      catch (SegmentOverflowException)
      {
        throw new StatusException(
          $"Some of the time values of the status are out of the acceptable range: {status}.");
      }
    }

    public StatusService(IStatusRepository statusRepository, ILogger<StatusService> logger)
    {
      this.statusRepository = statusRepository;
      this.logger = logger;
    }

    public async Task<Status> GetStatusAsync(DateTime dateTime)
    {
      var dateOnly = DateOnly.FromDateTime(dateTime);
      Status? maybeStatus;

      try
      {
        maybeStatus = await statusRepository.GetStatusAsync(dateOnly);
      }
      catch (DataAccessException e)
      {
        logger.LogError(e, e.Message);

        throw new StatusException("Failed to read the existing status from the database.");
      }

      return await CreateStatusIfNotExists(maybeStatus, dateTime);
    }

    public async Task<Status> AddWorkItemAsync(DateTime dateTime, string workItemName)
    {
      var existingStatus = await GetExistingStatusAsync(dateTime);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus).AddWorkItem(workItemName).Build();
      }
      catch (SegmentOverflowException)
      {
        throw new StatusException("Too many work items this day.Can't add more.", HttpStatusCode.BadRequest);
      }

      await SaveStatusAsync(updatedStatus);
      return updatedStatus;
    }

    public async Task<Status> DistributeWorkingTimeAsync(DateTime dateTime, int workItemIndex, TimeSpan duration)
    {
      var existingStatus = await GetExistingStatusAsync(dateTime);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus).DistributeWorkingTime(workItemIndex, duration).Build();
      }
      catch (MissingSegmentException)
      {
        throw new StatusException($"WorkItem with the index {workItemIndex} doesn't exist.", HttpStatusCode.BadRequest);
      }
      catch (SegmentOverflowException e)
      {
        throw new StatusException($"The maximum possible value is {e.AcceptableValue.Duration}.", HttpStatusCode.BadRequest);
      }

      await SaveStatusAsync(updatedStatus);
      return updatedStatus;
    }
  }
}
