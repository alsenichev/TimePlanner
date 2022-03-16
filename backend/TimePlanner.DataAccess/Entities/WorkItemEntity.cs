using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimePlanner.DataAccess.Entities
{
  [Table("WorkItems")]
  public class WorkItemEntity
  {
    [Column("WorkItemId")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid WorkItemEntityId { get; set; }

    public string Name { get; set; }

    public TimeSpan Duration { get; set; }
  }
}
