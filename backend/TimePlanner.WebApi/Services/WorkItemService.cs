using System.Collections.Immutable;
using TimePlanner.Domain.Exceptions;
using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;
using TimePlanner.WebApi.Models.Requests;

namespace TimePlanner.WebApi.Services;

public class WorkItemService : IWorkItemService
{
  private readonly IWorkItemRepository workItemRepository;
  private readonly IRecurrenceService recurrenceService;

  private async Task UpdateArchivedAndRepeating()
  {
    List<WorkItem> workItems = await workItemRepository.GetWorkItemsAsync();

    DateTime archiveThreshold = DateTime.UtcNow.AddDays(-7);

    // these items will have the modified category
    List<WorkItem> archived = workItems
      .Where(e => e.CompletedAt.HasValue && e.CompletedAt.Value.Date < archiveThreshold.Date)
      .Select(i => i with { Category = Category.Archived })
      .ToList();

    // these will have category and NextTime updated
    List<WorkItem> awaken = workItems.Where(e =>
        e.Category == Category.Scheduled &&
        e.NextTime.HasValue &&
        e.NextTime.Value <= DateTime.UtcNow &&
        (e.IsOnPause == null || e.IsOnPause.Value == false))
      .Select(i => i with { NextTime = null, Category = Category.Today })
      .ToList();

    ImmutableList<SortData> sortData = workItems.Select(e => CreateSortData(e)).ToImmutableList();
    List<WorkItem> createdItems = new List<WorkItem>();
    foreach (WorkItem workItem in awaken)
    {
      sortData = SortingService.ChangeCategory(sortData, workItem.Id.Value, Category.Today);

      if (!(workItem.IsIfPreviousCompleted.HasValue && workItem.IsIfPreviousCompleted.Value))
      {
        var createdItem = CreateNextRecurrentWorkItemInstance(workItem);
        if(createdItem != null)
        {
          createdItems.Add(createdItem.Value);
        }
      }
    }
  }
  private WorkItem UpdateCategory(
    List<WorkItem> workItems,
    WorkItem workItem,
    Category targetCategory,
    out Dictionary<Guid, SortData> sorted,
    out WorkItem? repeatedWorkItem)
  {
    WorkItem result = workItem with { Category = targetCategory };
    sorted = SortForCategoryChange(workItems, workItem.Id.Value, targetCategory);
    
    repeatedWorkItem = null;

    if (targetCategory == Category.Completed)
    {
      workItem.CompletedAt = DateTime.UtcNow;
      if (!string.IsNullOrEmpty(workItem.CronExpression))
      {
        repeatedWorkItem = CreateNextRecurrentWorkItemInstance(workItem);
        result = CleanUpRecurrence(result);
      }
    }
    return result;
  }

  private Dictionary<Guid, SortData> SortForCategoryChange(
    List<WorkItem> workItems, Guid workItemId, Category targetCategory)
  {
    List<SortData> models = workItems.Select(e => CreateSortData(e)).ToList();
    return SortingService.ChangeCategory(
      models, workItemId, targetCategory).ToDictionary(i => i.Id);
  }

  private WorkItem? CreateNextRecurrentWorkItemInstance(WorkItem workItem)
  {
    if ((workItem.MaxRepetitionCount.HasValue && workItem.RepetitionCount.HasValue &&
         workItem.RepetitionCount.Value == workItem.MaxRepetitionCount.Value) ||
         workItem.RecurrenceEndsOn.HasValue && workItem.RecurrenceEndsOn.Value >= DateTime.UtcNow)
    {
      return null;
    }

    // create new item
    return new WorkItem(
      Id: null,
      Name: workItem.Name,
      Category: Category.Scheduled,
      CreatedAt: DateTime.UtcNow,
      CompletedAt: null,
      NextTime: recurrenceService.CalculateNextTime(workItem.CronExpression!, DateTime.UtcNow, DateTime.UtcNow),
      CronExpression: workItem.CronExpression,
      RecurrenceStartsOn: workItem.RecurrenceStartsOn,
      RecurrenceEndsOn: workItem.RecurrenceEndsOn,
      IsIfPreviousCompleted: workItem.IsIfPreviousCompleted,
      MaxRepetitionCount: workItem.MaxRepetitionCount,
      RepetitionCount: workItem.MaxRepetitionCount.HasValue ? ++workItem.RepetitionCount: null,
      IsOnPause: workItem.IsOnPause,
      SortOrder: int.MaxValue,
      Durations: new List<Duration>());
  }

  private WorkItem CleanUpRecurrence(WorkItem workItem)
  {
    return workItem with
    {
      CronExpression = null,
      RecurrenceStartsOn = null,
      RecurrenceEndsOn = null,
      RepetitionCount = null,
      MaxRepetitionCount = null,
      IsIfPreviousCompleted = null,
      IsOnPause = null
    };
  }

  private SortData CreateSortData(WorkItem workItem)
  {
    return new SortData(workItem.Id.Value, workItem.Category, workItem.SortOrder);
  }

  private Dictionary<Guid, SortData> UpdateSortOrder(
    List<WorkItem> workItems, WorkItem workItem, int sortOrder)
  {
    List<SortData> sortData = workItems.Select(e => CreateSortData(e)).ToList();
    return SortingService.ChangeSortOrder(
      sortData,
      CreateSortData(workItem),
      sortOrder - workItem.SortOrder).ToDictionary(i => i.Id);
  }

  public WorkItemService(
    IWorkItemRepository workItemRepository,
    IRecurrenceService recurrenceService)
  {
    this.workItemRepository = workItemRepository;
    this.recurrenceService = recurrenceService;
  }

  public async Task<List<WorkItem>> GetWorkItemsAsync()
  {
    await UpdateArchivedAndRepeating();
    return await workItemRepository.GetWorkItemsAsync();
  }

  public Task<WorkItem> GetWorkItemAsync(Guid workItemId)
  {
    return workItemRepository.GetWorkItemAsync(workItemId);
  }

  public async Task<WorkItem> CreateWorkItemAsync(CreateWorkItemRequest request)
  {
    List<WorkItem> workItems = await workItemRepository.GetWorkItemsAsync();
    List<SortData> sortModels = workItems.Select(e => CreateSortData(e)).ToList();
    var tempGuid = Guid.NewGuid();
    var sortModel = new SortData(tempGuid, Category.Today, 0);
    Dictionary<Guid, SortData> ordered = SortingService.AddItem(sortModels, sortModel).ToDictionary(i => i.Id);
    int sortOrder = ordered[tempGuid].SortOrder;
    return await workItemRepository.CreateWorkItemAsync(request.Name, sortOrder, ordered);
  }

  public async Task<WorkItem> UpdateWorkItemAsync(UpdateWorkItemRequest workItemRequest)
  {
    List<WorkItem> workItems = await workItemRepository.GetWorkItemsAsync();
    WorkItem workItem = workItems.SingleOrDefault(i => i.Id == workItemRequest.Id);
    if (workItem.Id == null)
    {
      throw new EntityMissingException();
    }

    if (workItemRequest.SortOrder != workItem.SortOrder)
    {
      workItem = workItem with { SortOrder = workItemRequest.SortOrder };
      Dictionary<Guid, SortData>? sorted =
        UpdateSortOrder(workItems, workItem, workItemRequest.SortOrder);
      return await workItemRepository.UpdateWorkItemAsync(workItem, sorted, null);
    }

    if (workItemRequest.Category != workItem.Category)
    {
      workItem = UpdateCategory(
        workItems,
        workItem,
        workItemRequest.Category,
        out Dictionary<Guid, SortData> sorted, out WorkItem? repeatedWorkItem);
      return await workItemRepository.UpdateWorkItemAsync(workItem, sorted, repeatedWorkItem);
    }
    
    workItem = workItem with { Name = workItemRequest.Name };

    if (!string.IsNullOrEmpty(workItem.CronExpression) && string.IsNullOrEmpty(workItemRequest.CronExpression))
    {
      // reset existing recurrence
      workItem = CleanUpRecurrence(workItem);

      // promote to Today
      workItem = workItem with { Category = Category.Today };
      Dictionary<Guid, SortData>? sorted = SortForCategoryChange(workItems, workItem.Id.Value, Category.Today);

      return await workItemRepository.UpdateWorkItemAsync(workItem, sorted, null);
    }

    if (string.IsNullOrEmpty(workItemRequest.CronExpression))
    {
      // both source and target have no recurrence - just update name
      return await workItemRepository.UpdateWorkItemAsync(workItem, null , null);
    }

    // assigning or changing recurrence
    Dictionary<Guid, SortData>? sortData = null;
    bool isCreating = string.IsNullOrEmpty(workItem.CronExpression);

    if (isCreating){
      sortData = SortForCategoryChange(workItems, workItem.Id.Value, Category.Scheduled);
    }

    DateTime startsOn = workItemRequest.RecurrenceStartsOn.HasValue
      ? workItemRequest.RecurrenceStartsOn.Value : DateTime.UtcNow;

    workItem = workItem with 
    {
      Category = isCreating ? Category.Scheduled : workItem.Category,
      CronExpression = workItemRequest.CronExpression,
      RecurrenceStartsOn = startsOn,
      RecurrenceEndsOn = workItemRequest.RecurrenceEndsOn,
      IsIfPreviousCompleted = workItemRequest.IsAfterPreviousCompleted,
      MaxRepetitionCount = workItemRequest.MaxRepetitionsCount,
      IsOnPause = workItemRequest.IsOnPause,
      NextTime = recurrenceService.CalculateNextTime(workItemRequest.CronExpression, startsOn, DateTime.UtcNow)
    };

    return await workItemRepository.UpdateWorkItemAsync(workItem, sortData, null);

  }

  public async Task DeleteWorkItemAsync(Guid workItemId)
  {
    List<WorkItem> workItems = await workItemRepository.GetWorkItemsAsync();
    WorkItem workItem = workItems.SingleOrDefault(i => i.Id == workItemId);
    if (workItem.Id == null)
    {
      throw new EntityMissingException();
    }

    List<SortData> sortModels = workItems.Select(e => CreateSortData(e)).ToList();
    Dictionary<Guid, SortData> ordered = SortingService.DeleteItem(sortModels, workItemId).ToDictionary(i => i.Id);
    await workItemRepository.DeleteWorkItemAsync(workItemId, ordered);
  }
}
