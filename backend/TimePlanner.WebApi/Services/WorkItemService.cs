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

    WakingUp? MapWakingUp(WakingUpRequest? wr)
    {
      if (wr is null)
      {
        return null;
      }

      return new WakingUp(DateOnly.Parse(wr.Value.when), Enum.Parse<Category>(wr.Value.where));

    }

    var workItem = new WorkItem(
      workItemRequest.id,
      workItemRequest.name,
      workItemRequest.durations?.Select(d => MapDuration(d)).ToList() ?? new List<Duration>(),
      Enum.Parse<Category>(workItemRequest.category),
      MapWakingUp(workItemRequest.wakingUp),
      workItemRequest.recurrenceDays.HasValue ? new Recurrence(workItemRequest.recurrenceDays.Value) : null,
      new History(workItemRequest.createdAt, workItemRequest.completedAt),
      workItemRequest.sortOrder);
    return workItemRepository.UpdateWorkItemAsync(workItem);
  }

  public Task DeleteWorkItemAsync(Guid workItemId)
  {
    return workItemRepository.DeleteWorkItemAsync(workItemId);
  }
}
