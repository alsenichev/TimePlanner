using TimePlanner.Domain.Models;

namespace TimePlanner.Domain.Interfaces
{
  public interface IWorkItemRepository
  {
    Task<List<WorkItem>> GetWorkItemsAsync();
    Task<WorkItem> GetWorkItemAsync(Guid workItemId);
    Task<WorkItem> CreateWorkItemAsync(string name);
    Task<WorkItem> UpdateWorkItemAsync(WorkItem workItem);
    Task DeleteWorkItemAsync(Guid workItemId);
  }
}
