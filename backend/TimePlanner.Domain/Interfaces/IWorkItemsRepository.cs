using TimePlanner.Domain.Models;

namespace TimePlanner.Domain.Interfaces
{
  public interface IWorkItemRepository
  {
    Task<List<WorkItem>> GetWorkItemsAsync();

    Task<WorkItem> GetWorkItemAsync(Guid workItemId);

    Task<WorkItem> CreateWorkItemAsync(string name);

    Task<WorkItem> UpdateWorkItemAsync(
      Guid workItemId,
      string name,
      Category targetCategory,
      int sortOrder);

    Task<WorkItem> UpdateRecurrence(
      Guid workItemId,
      string? cronExpression,
      bool? isAfterPreviousCompleted,
      DateTime? recurrenceStartsFrom,
      int? maxRepetitionsCount);

    Task DeleteWorkItemAsync(Guid workItemId);
  }
}
