using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.WebApi.Mappers;
using TimePlanner.WebApi.Models.Requests;
using TimePlanner.WebApi.Services;

namespace TimePlanner.WebApi.Controllers
{
  [ApiController]
  [Route("statuses")]
  public class StatusController : ControllerBase
  {
    private readonly IStatusService statusService;
    private readonly IStatusMapper statusMapper;

    public StatusController(
      IStatusService statusService,
      IStatusMapper statusMapper)
    {
      this.statusService = statusService;
      this.statusMapper = statusMapper;
    }

    [HttpGet("{count}")]
    public async Task<ActionResult<List<Status>>> GetStatus(
      [FromRoute] int count)
    {
      List<Status> statuses = await statusService.GetStatuses(count);
      return Ok(statuses.Select(s => statusMapper.Map(s)));
    }

    [HttpGet("current")]
    public async Task<ActionResult<Status>> GetCurrentStatus()
    {
      Status status = await statusService.GetCurrentStatusAsync();
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/workItems")]
    public async Task<ActionResult<Status>> AddWorkItem(
      [FromRoute] Guid statusId,
      [FromBody] WorkItemRequest request,
      [FromServices] IValidator<WorkItemRequest> validator)
    {
      var validationResult = await validator.ValidateAsync(request);
      if (!validationResult.IsValid)
      {
        return BadRequest(string.Join(Environment.NewLine, validationResult.Errors));
      }

      Status status = await statusService.AddWorkItemAsync(statusId, request.Name);
      return Ok(statusMapper.Map(status));
    }

    [HttpDelete("{statusId}/workItems/{workItemId}")]
    public async Task<ActionResult> DeleteWorkItem(
      [FromRoute] Guid statusId,
      [FromRoute] Guid workItemId)
    {
      Status status = await statusService.DeleteWorkItemAsync(statusId, workItemId);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/workItems/{workItemId}/time/{duration}")]
    public async Task<ActionResult<Status>> DistributeWorkingTime(
      [FromRoute] Guid statusId,
      [FromRoute] Guid workItemId,
      [FromRoute] TimeSpan duration)
    {
      Status status = await statusService.DistributeWorkingTimeAsync(
        statusId, workItemId, duration);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/pause/{duration}")]
    public async Task<ActionResult<Status>> SetPause(
      [FromRoute] Guid statusId,
      [FromRoute] TimeSpan duration)
    {
      Status status = await statusService.SetPause(statusId, duration);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/break/start")]
    public async Task<ActionResult<Status>> StartBreak([FromRoute] Guid statusId)
    {
      Status status = await statusService.StartBreak(statusId);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/break/end")]
    public async Task<ActionResult<Status>> EndBreak([FromRoute] Guid statusId)
    {
      Status status = await statusService.EndBreak(statusId);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/break/cancel")]
    public async Task<ActionResult<Status>> CancelBreak([FromRoute] Guid statusId)
    {
      Status status = await statusService.CancelBreak(statusId);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/startTime/{startTime}")]
    public async Task<ActionResult<Status>> FixStartTime(
      [FromRoute] Guid statusId,
      [FromRoute] TimeSpan startTime)
    {
      Status status = await statusService.FixStartTime(statusId, TimeOnly.FromTimeSpan(startTime));
      return Ok(statusMapper.Map(status));
    }
  }
}
