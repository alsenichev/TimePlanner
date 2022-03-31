using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Models.Requests;

namespace TimePlanner.WebApi.Services;

public class WorkItemService : IWorkItemService
{
  private readonly IWorkItemRepository workItemRepository;

  public WorkItemService(IWorkItemRepository workItemRepository)
  {
    this.workItemRepository = workItemRepository;
  }

  public Task<List<WorkItem>> GetWorkItemsAsync()
  {
    return workItemRepository.GetWorkItemsAsync();
  }

  public Task<WorkItem> GetWorkItemAsync(Guid workItemId)
  {
    return workItemRepository.GetWorkItemAsync(workItemId);
  }

  public Task<WorkItem> CreateWorkItemAsync(CreateWorkItemRequest request)
  {
    return workItemRepository.CreateWorkItemAsync(request.Name);
  }

  public Task<WorkItem> UpdateWorkItemAsync(UpdateWorkItemRequest workItemRequest)
  {
    if (workItemRequest.UpdateRecurrence)
    {
      return workItemRepository.UpdateRecurrence(
        workItemRequest.Id,
        workItemRequest.CronExpression,
        workItemRequest.IsAfterPreviousCompleted,
        workItemRequest.RecurrenceStartsFrom,
        workItemRequest.MaxRepetitionsCount);
    }

    return workItemRepository.UpdateWorkItemAsync(
      workItemRequest.Id,
      workItemRequest.Name,
      Enum.Parse<Category>(workItemRequest.Category),
      workItemRequest.SortOrder);
  }

  public Task DeleteWorkItemAsync(Guid workItemId)
  {
    return workItemRepository.DeleteWorkItemAsync(workItemId);
  }
}
