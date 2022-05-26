using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Models.Responses;

namespace TimePlanner.WebApi.Mappers
{
  public interface IWorkItemResponseMapper
  {
    WorkItemResponse Map(WorkItem workItem);
  }
}
