using System.ComponentModel.DataAnnotations;
using VotingApp.Api.Models;

namespace VotingApp.Api.DTOs;

public class CreateQuestionDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Side1Text { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Side2Text { get; set; } = string.Empty;
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Side1Text { get; set; } = string.Empty;
    public string Side2Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int Side1Votes { get; set; }
    public int Side2Votes { get; set; }
}

public class CreateVoteDto
{
    [Required]
    public int QuestionId { get; set; }

    [Required]
    public VoteChoice Choice { get; set; }
}

public class VoteDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public VoteChoice Choice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int QuestionsCount { get; set; }
    public int VotesCount { get; set; }
}
