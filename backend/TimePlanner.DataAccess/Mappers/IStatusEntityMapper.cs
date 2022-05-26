using TimePlanner.DataAccess.Entities;
using TimePlanner.Domain.Models;

namespace TimePlanner.DataAccess.Mappers
{
  public interface IStatusEntityMapper
  {
    StatusEntity Map(Status status);

    Status Map(StatusEntity entity);
  }
}
