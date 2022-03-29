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
    Duration MapDuration(DurationRequest dr)
    {
      return new Duration(dr.id, DateOnly.Parse(dr.date), TimeSpan.Parse(dr.value));
    }

    return workItemRepository.UpdateWorkItemAsync(
      workItemRequest.Id,
      workItemRequest.Name,
      Enum.Parse<Category>(workItemRequest.Category),
      workItemRequest.SortOrder,
      workItemRequest.Recurrence,
      workItemRequest.durations.Select(d => MapDuration(d)).ToList());
  }

  public Task DeleteWorkItemAsync(Guid workItemId)
  {
    return workItemRepository.DeleteWorkItemAsync(workItemId);
  }
}
