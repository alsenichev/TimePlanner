using TimePlanner.Domain.Core.WorkItemsTracking;
using TimePlanner.WebApi.Models.Responses;

namespace TimePlanner.WebApi.Mappers
{
  public interface IStatusMapper
  {
    StatusResponse Map(Status status);
  }
}
