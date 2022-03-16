using System.Net;
using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.Domain.Core.WorkItemsTracking.Segments;
using TimePlanner.Domain.Exceptions;
using TimePlanner.Domain.Services.Interfaces;
using TimePlanner.WebApi.Exceptions;

namespace TimePlanner.WebApi.Services
{
  public class StatusService : IStatusService
  {
    private readonly IStatusRepository statusRepository;
    private readonly ILogger<StatusService> logger;

    private Status CreateStatus(Status? previous)
    {
      var deposit = previous?.Deposit ?? TimeSpan.Zero;
      StatusBuilder statusBuilder;
      try
      {
        statusBuilder = StatusBuilder.Of(deposit);
      }
      catch (SegmentOverflowException)
      {
        throw new ApiException(
          $"The deposit value {deposit.Duration} is out of the acceptable range.",
          HttpStatusCode.InternalServerError);
      }

      return statusBuilder.Build();
    }

    private async Task<Status> SaveStatusAsync(Status status)
    {
      try
      {
        return await statusRepository.SaveStatusAsync(status);
      }
      catch (DataAccessException e)
      {
        logger.LogError(e, e.Message);

        throw new ApiException("Failed to save changes.", HttpStatusCode.InternalServerError);
      }
      catch (EntityMissingException)
      {
        logger.LogError("Failed to save status {status}", status);

        throw new ApiException("Failed to save changes.", HttpStatusCode.InternalServerError);
      }
    }

    private async Task<Status> GetExistingStatusAsync(Guid statusId)
    {
      try
      {
        return await statusRepository.GetStatusAsync(statusId);
      }
      catch (DataAccessException e)
      {
        logger.LogError(e, e.Message);

        throw new ApiException(
          "Failed to read the existing status from the database.",
          HttpStatusCode.InternalServerError);
      }
      catch (EntityMissingException)
      {
        throw new ApiException(
          $"Status with id {statusId} does not exist.",
          HttpStatusCode.BadRequest);
      }
    }

    private StatusBuilder CreateStatusBuilder(Status status)
    {
      try
      {
        return StatusBuilder.CreateStatusBuilder(status);
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

    public async Task<Status> GetCurrentStatusAsync()
    {
      Status? latestStatus;

      try
      {
        var list = await statusRepository.GetStatusesAsync(1);
        latestStatus = list.Count == 1 ? list[0] : null;
      }
      catch (DataAccessException e)
      {
        logger.LogError(e, e.Message);

        throw new ApiException(
          "Failed to read the existing status from the database.",
          HttpStatusCode.InternalServerError);
      }

      Status status;
      if (latestStatus.HasValue && latestStatus.Value.StartedAt.Date.Equals(DateTime.Now.Date))
      {
        status = CreateStatusBuilder(latestStatus.Value).Build();
      }
      else
      {
        status = CreateStatus(latestStatus);
      }

      return await SaveStatusAsync(status);
    }

    public Task<List<Status>> GetStatuses(int count)
    {
      return statusRepository.GetStatusesAsync(count);
    }

    public async Task<Status> AddWorkItemAsync(Guid statusId, string workItemName)
    {
      Status existingStatus = await GetExistingStatusAsync(statusId);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus)
          .CreateWorkItem(workItemName)
          .Build();
      }
      catch (SegmentOverflowException)
      {
        throw new ApiException(
          "Too many work items this day. Can't add more.",
          HttpStatusCode.BadRequest);
      }

      return await SaveStatusAsync(updatedStatus);
    }

    public async Task<Status> DistributeWorkingTimeAsync(Guid statusId, Guid workItemId, TimeSpan duration)
    {
      Status existingStatus = await GetExistingStatusAsync(statusId);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus)
          .DistributeWorkingTime(workItemId, duration)
          .Build();
      }
      catch (MissingSegmentException)
      {
        throw new ApiException(
          $"WorkItem with the id {workItemId} doesn't exist.",
          HttpStatusCode.BadRequest);
      }
      catch (SegmentOverflowException e)
      {
        throw new ApiException(
          $"The maximum possible value is {e.AcceptableValue.Duration}.",
          HttpStatusCode.BadRequest);
      }

      return await SaveStatusAsync(updatedStatus);
    }

    public async Task<Status> SetPause(Guid statusId, TimeSpan duration)
    {
      Status existingStatus = await GetExistingStatusAsync(statusId);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus)
          .RegisterPause(duration)
          .Build();
      }
      catch (SegmentOverflowException e)
      {
        throw new ApiException(
          $"The maximum possible value is {e.AcceptableValue.Duration}.",
          HttpStatusCode.BadRequest);
      }

      return await SaveStatusAsync(updatedStatus);
    }

    public async Task<Status> StartBreak(Guid statusId)
    {
      Status existingStatus = await GetExistingStatusAsync(statusId);
      Status updatedStatus;
      updatedStatus = CreateStatusBuilder(existingStatus)
        .StartBreak()
        .Build();
      return await SaveStatusAsync(updatedStatus);
    }

    public async Task<Status> EndBreak(Guid statusId)
    {
      Status existingStatus = await GetExistingStatusAsync(statusId);
      Status updatedStatus;
      try
      {
        updatedStatus = CreateStatusBuilder(existingStatus)
          .EndBreak()
          .Build();
      }
      catch (StatusBuilderException e)
      {
        throw new ApiException(e.Message, HttpStatusCode.BadRequest);
      }

      return await SaveStatusAsync(updatedStatus);
    }

    public async Task<Status> CancelBreak(Guid statusId)
    {
      Status existingStatus = await GetExistingStatusAsync(statusId);
      Status updatedStatus = CreateStatusBuilder(existingStatus)
        .CancelBreak()
        .Build();
      return await SaveStatusAsync(updatedStatus);
    }

    public async Task<Status> DeleteWorkItemAsync(Guid statusId, Guid workItemId)
    {
      Status beforeDeletion = await GetExistingStatusAsync(statusId);
      if (!beforeDeletion.WorkItems.Any(i => i.Id.Value.Equals(workItemId)))
      {
        throw new ApiException($"Work item with id {workItemId} does not exist.", HttpStatusCode.BadRequest);
      }
      try
      {
        await statusRepository.DeleteWorkItemAsync(statusId, workItemId);
      }
      catch (DataAccessException)
      {
        throw new ApiException($"Failed to delete workItem with id {workItemId}", HttpStatusCode.InternalServerError);
      }
      Status afterDeletion = await GetExistingStatusAsync(statusId);
      Status updatedStatus = CreateStatusBuilder(afterDeletion).Build();
      return await SaveStatusAsync(updatedStatus);
    }

    public async Task<Status> FixStartTime(Guid statusId, TimeOnly startTime)
    {
      Status existingStatus = await GetExistingStatusAsync(statusId);
      var builder = CreateStatusBuilder(existingStatus);
      try
      {
        builder.FixStartTime(startTime);
      }
      catch (SegmentOverflowException e)
      {
        throw new ApiException(
          $"Some of the time values of the status are out of the acceptable range: {e.AcceptableValue.Duration}.",
          HttpStatusCode.BadRequest);
      }
      Status updatedStatus = builder.Build();
      return await SaveStatusAsync(updatedStatus);
    }
  }
}
