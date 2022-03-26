using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Mappers;
using TimePlanner.WebApi.Models.Requests;
using TimePlanner.WebApi.Models.Responses;
using TimePlanner.WebApi.Services;

namespace TimePlanner.WebApi.Controllers
{
  [ApiController]
  [Route("workItems")]
  public class WorkItemsController : Controller
  {
    private readonly IWorkItemService workItemService;
    private readonly IWorkItemResponseMapper workItemResponseMapper;

    public WorkItemsController(
      IWorkItemService workItemService,
      IWorkItemResponseMapper workItemResponseMapper)
    {
      this.workItemService = workItemService;
      this.workItemResponseMapper = workItemResponseMapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkItemResponse>>> GetWorkItems()
    {
      var workItems = await workItemService.GetWorkItemsAsync();
      return Ok(workItems.Select(workItemResponseMapper.Map).ToList());
    }

    [HttpGet("{workItemId}")]
    public async Task<ActionResult<WorkItemResponse>> GetWorkItem(
      [FromRoute] Guid workItemId)
    {
      var workItem = await workItemService.GetWorkItemAsync(workItemId);
      return Ok(workItemResponseMapper.Map(workItem));
    }

    [HttpPost()]
    public async Task<ActionResult<WorkItemResponse>> CreateWorkItem(
      [FromBody] CreateWorkItemRequest request,
      [FromServices] IValidator<CreateWorkItemRequest> validator)
    {
      var validationResult = await validator.ValidateAsync(request);
      if (!validationResult.IsValid)
      {
        return BadRequest(string.Join(Environment.NewLine, validationResult.Errors));
      }

      var result = await workItemService.CreateWorkItemAsync(request);
      return Ok(workItemResponseMapper.Map(result));
    }

    [HttpPut("{workItemId}")]
    public async Task<ActionResult<WorkItemResponse>> UpdateWorkItem(
      [FromRoute] Guid workItemId,
      [FromBody] UpdateWorkItemRequest request,
      [FromServices] IValidator<UpdateWorkItemRequest> validator)
    {
      var validationResult = await validator.ValidateAsync(request);
      if (!validationResult.IsValid)
      {
        return BadRequest(string.Join(Environment.NewLine, validationResult.Errors));
      }

      var result = await workItemService.UpdateWorkItemAsync(request);
      return Ok(workItemResponseMapper.Map(result));
    }

    [HttpDelete("{workItemId}")]
    public async Task<ActionResult> DeleteWorkItemAsync(
      [FromRoute] Guid workItemId)
    {
      await workItemService.DeleteWorkItemAsync(workItemId);
      return Ok();
    }
  }
}
