using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace BlogModule.Domain;

[Index("Slug", IsUnique = true)]
[Table("Post", Schema = "dbo")]
public class Post : BaseEntity
{
    public string Description { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(80)]
    public string OwnerName { get; set; }
    [MaxLength(80)]
    public string Title { get; set; }
    [MaxLength(80)]
    public string Slug { get; set; }
    public Guid CategoryId { get; set; }
    public long Visit { get; set; }
    public string ImageName { get; set; }

}