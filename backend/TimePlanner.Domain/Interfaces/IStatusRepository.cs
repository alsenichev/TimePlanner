using TimePlanner.Domain.Models;

namespace TimePlanner.Domain.Interfaces
{
  public interface IStatusRepository
  {
    Task<Status> GetStatusAsync(Guid statusId);

    Task<List<Status>> GetStatusesAsync(int count);

    Task<Status> SaveStatusAsync(Status status);
  }
}
