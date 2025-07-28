using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotingApp.Api.Models;

public class Question
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

    // Computed properties
    [NotMapped]
    public int YesVotes => Votes?.Count(v => v.IsYes) ?? 0;

    [NotMapped]
    public int NoVotes => Votes?.Count(v => !v.IsYes) ?? 0;
}
