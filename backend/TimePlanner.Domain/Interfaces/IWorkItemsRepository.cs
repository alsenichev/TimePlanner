using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;

namespace TimePlanner.Domain.Interfaces
{
  public interface IWorkItemRepository
  {
    Task<List<WorkItem>> GetWorkItemsAsync();

    Task<WorkItem> GetWorkItemAsync(Guid workItemId);

    Task<WorkItem> CreateWorkItemAsync(string name);

    Task<WorkItem> UpdateWorkItemAsync(
      WorkItem workItem,
      Dictionary<Guid,SortData>? sortData,
      WorkItem? repeatedWorkItem);

    Task DeleteWorkItemAsync(Guid workItemId);
  }
}
