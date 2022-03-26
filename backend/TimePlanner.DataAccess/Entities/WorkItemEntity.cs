using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimePlanner.Domain.Models;

namespace TimePlanner.DataAccess.Entities
{
  [Table("WorkItems")]
  public class WorkItemEntity
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid WorkItemId { get; set; }

    public string Name { get; set; }

    public List<DurationEntity> Durations { get; set; }

    public string Category { get; set; }

    public int SortOrder { get; set; }
    
    public DateTime? WakingUpWhen { get; set; }

    public string? WakingUpWhere { get; set; }

    public int? RecurrenceDays { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
  }
}
