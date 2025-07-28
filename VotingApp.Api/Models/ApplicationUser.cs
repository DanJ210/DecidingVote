using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VotingApp.Api.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
