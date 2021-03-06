using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimePlanner.DataAccess.Entities
{
  [Table("Statuses")]
  public class StatusEntity
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid StatusId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? BreakStartedAt { get; set; }

    public TimeSpan Deposit { get; set; }

    public TimeSpan Pause { get; set; }

    public TimeSpan UndistributedTime { get; set; }
  }
}
