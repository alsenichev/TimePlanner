using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.WebApi.Mappers;
using TimePlanner.WebApi.Models.Requests;
using TimePlanner.WebApi.Services;

namespace TimePlanner.WebApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
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

    [HttpGet]
    public async Task<ActionResult<Status>> GetStatus()
    {
      var status = await statusService.GetStatusAsync(DateTime.Now);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("workItems")]
    public async Task<ActionResult<Status>> AddWorkItem(
      [FromBody] WorkItemRequest request,
      [FromServices] IValidator<WorkItemRequest> validator)
    {
      var validationResult = await validator.ValidateAsync(request);
      if (!validationResult.IsValid)
      {
        return BadRequest(string.Join(Environment.NewLine, validationResult.Errors));
      }

      Status status = await statusService.AddWorkItemAsync(DateTime.Now, request.Name);
      return Ok(statusMapper.Map(status));
    }

    [HttpPost("workItems/{workItemIndex}/time/{duration}")]
    public async Task<ActionResult<Status>> DistributeWorkingTime(
      [FromRoute] int workItemIndex,
      [FromRoute] TimeSpan duration)
    {
      Status status = await statusService.DistributeWorkingTimeAsync(
        DateTime.Now, workItemIndex, duration);
      return Ok(statusMapper.Map(status));
    }
  }
}
