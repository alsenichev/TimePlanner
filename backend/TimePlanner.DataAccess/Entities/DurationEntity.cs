using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimePlanner.DataAccess.Entities
{
  [Table("Durations")]
  public class DurationEntity
  {
    public int DurationEntityId { get; set; }
    public Guid WorkItemId { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Value { get; set; }
  }
}
