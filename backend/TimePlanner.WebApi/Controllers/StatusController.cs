using Microsoft.AspNetCore.Mvc;
using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Mappers;
using TimePlanner.WebApi.Services;

namespace TimePlanner.WebApi.Controllers
{
  [ApiController]
  [Route("statuses")]
  public class StatusController : ControllerBase
  {
    private readonly IStatusService statusService;
    private readonly IStatusResponseMapper statusMapper;

    public StatusController(
      IStatusService statusService,
      IStatusResponseMapper statusMapper)
    {
      this.statusService = statusService;
      this.statusMapper = statusMapper;
    }

    [HttpGet("{count}")]
    public async Task<ActionResult<List<Status>>> GetStatus(
      [FromRoute] int count)
    {
      var statuses = await statusService.GetStatuses(count);
      return Ok(statuses.Select(s => statusMapper.Map(s)));
    }

    [HttpGet("current")]
    public async Task<ActionResult<Status>> GetCurrentStatus()
    {
      var status = await statusService.GetCurrentStatusAsync();
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/pause/{duration}")]
    public async Task<ActionResult<Status>> SetPause(
      [FromRoute] Guid statusId,
      [FromRoute] TimeSpan duration)
    {
      var status = await statusService.SetPause(statusId, duration);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/break/start")]
    public async Task<ActionResult<Status>> StartBreak([FromRoute] Guid statusId)
    {
      var status = await statusService.StartBreak(statusId);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/break/end")]
    public async Task<ActionResult<Status>> EndBreak([FromRoute] Guid statusId)
    {
      var status = await statusService.EndBreak(statusId);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/break/cancel")]
    public async Task<ActionResult<Status>> CancelBreak([FromRoute] Guid statusId)
    {
      var status = await statusService.CancelBreak(statusId);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("{statusId}/startTime/{startTime}")]
    public async Task<ActionResult<Status>> FixStartTime(
      [FromRoute] Guid statusId,
      [FromRoute] TimeSpan startTime)
    {
      var status = await statusService.FixStartTime(statusId, TimeOnly.FromTimeSpan(startTime));
      return Ok(statusMapper.Map(status));
    }
  }
}
