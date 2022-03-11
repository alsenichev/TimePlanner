using System.Net;
using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Services.Interfaces;
using TimePlanner.WebApi.Exceptions;

namespace TimePlanner.WebApi.Services
{
  public class StatusService : IStatusService
  {
    private readonly IStatusRepository statusRepository;
    private readonly ILogger<StatusService> logger;

    private async Task<Status> CreateStatusIfNotExists(Status? status, DateTime dateTime)
    {
      if (status.HasValue)
      {
        return status.Value;
      }

      Status? prev;
      try
      {
        prev = await statusRepository.GetPreviousStatusAsync(DateOnly.FromDateTime(dateTime));
      }
      catch (DataAccessException)
      {
        throw new ApiException(
          "Failed to read previous status.",
          HttpStatusCode.InternalServerError);
      }

      var deposit = prev.HasValue ? prev.Value.Deposit : TimeSpan.Zero;
      StatusBuilder statusBuilder;
      try
      {
        statusBuilder = StatusBuilder.Of(deposit, dateTime);
      }
      catch (SegmentOverflowException)
      {
        throw new ApiException(
          $"The deposit value {deposit.Duration} is out of the acceptable range.",
          HttpStatusCode.InternalServerError);
      }

      return statusBuilder.Build();
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

        throw new ApiException("Failed to save changes.", HttpStatusCode.InternalServerError);
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

        throw new ApiException(
          "Failed to read the existing status from the database.",
          HttpStatusCode.InternalServerError);
      }

      if (!maybeStatus.HasValue)
      {
        throw new ApiException(
          $"Status for {dateOnly} does not exist. Use GET/status endpoint to create one.",
          HttpStatusCode.BadRequest);
      }

      return maybeStatus.Value;
    }

    private StatusBuilder CreateStatusBuilder(Status status, DateTime dateTime)
    {
      try
      {
        return StatusBuilder.CreateStatusBuilder(status, dateTime);
      }
      catch (SegmentOverflowException)
      {
        throw new ApiException(
          $"Some of the time values of the status are out of the acceptable range: {status}.",
          HttpStatusCode.InternalServerError);
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

        throw new ApiException(
          "Failed to read the existing status from the database.",
          HttpStatusCode.InternalServerError);
      }

      Status status;
      if (maybeStatus.HasValue)
      {
        status = CreateStatusBuilder(maybeStatus.Value, dateTime).Build();
      }
      else
      {
        status = await CreateStatusIfNotExists(maybeStatus, dateTime);
      }

      await SaveStatusAsync(status);
      return status;
    }

    public async Task<Status> AddWorkItemAsync(DateTime dateTime, string workItemName)
    {
      var existingStatus = await GetExistingStatusAsync(dateTime);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus, dateTime)
          .AddWorkItem(workItemName)
          .Build();
      }
      catch (SegmentOverflowException)
      {
        throw new ApiException(
          "Too many work items this day.Can't add more.",
          HttpStatusCode.BadRequest);
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
        updatedStatus = CreateStatusBuilder(existingStatus, dateTime)
          .DistributeWorkingTime(workItemIndex, duration)
          .Build();
      }
      catch (MissingSegmentException)
      {
        throw new ApiException(
          $"WorkItem with the index {workItemIndex} doesn't exist.",
          HttpStatusCode.BadRequest);
      }
      catch (SegmentOverflowException e)
      {
        throw new ApiException(
          $"The maximum possible value is {e.AcceptableValue.Duration}.",
          HttpStatusCode.BadRequest);
      }

      await SaveStatusAsync(updatedStatus);
      return updatedStatus;
    }

    public async Task<Status> SetPause(DateTime dateTime, TimeSpan duration)
    {
      var existingStatus = await GetExistingStatusAsync(dateTime);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus, dateTime)
          .RegisterPause(duration)
          .Build();
      }
      catch (SegmentOverflowException e)
      {
        throw new ApiException(
          $"The maximum possible value is {e.AcceptableValue.Duration}.",
          HttpStatusCode.BadRequest);
      }
      await SaveStatusAsync(updatedStatus);
      return updatedStatus;
    }

    public async Task<Status> StartBreak(DateTime dateTime)
    {
      var existingStatus = await GetExistingStatusAsync(dateTime);
      Status updatedStatus;
      updatedStatus = CreateStatusBuilder(existingStatus, dateTime)
        .StartBreak(dateTime)
        .Build();
      await SaveStatusAsync(updatedStatus);
      return updatedStatus;
    }

    public async Task<Status> EndBreak(DateTime dateTime)
    {
      var existingStatus = await GetExistingStatusAsync(dateTime);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus, dateTime)
          .EndBreak(dateTime)
          .Build();
      }
      catch (StatusBuilderException e)
      {
        throw new ApiException(e.Message, HttpStatusCode.BadRequest);
      }
      await SaveStatusAsync(updatedStatus);
      return updatedStatus;
    }

    public async Task<Status> CancelBreak(DateTime dateTime)
    {
      var existingStatus = await GetExistingStatusAsync(dateTime);
      Status updatedStatus;
      updatedStatus = CreateStatusBuilder(existingStatus, dateTime)
        .CancelBreak()
        .Build();
      await SaveStatusAsync(updatedStatus);
      return updatedStatus;
    }
  }
}
