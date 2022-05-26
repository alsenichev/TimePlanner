using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Models.Requests;

namespace TimePlanner.WebApi.Services;

public interface IWorkItemService
{
  Task<List<WorkItem>> GetWorkItemsAsync();
  Task<WorkItem> GetWorkItemAsync(Guid workItemId);
  Task<WorkItem> CreateWorkItemAsync(CreateWorkItemRequest request);
  Task<WorkItem> UpdateWorkItemAsync(UpdateWorkItemRequest workItem);
  Task DeleteWorkItemAsync(Guid workItemId);
}