using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Core.WorkItemsTracking;

namespace TimePlanner.DataAccess.Mappers
{
  public interface IStatusEntityMapper
  {
    StatusEntity Map(Status status);

    Status Map(StatusEntity entity);
  }
}
