using System.ComponentModel.DataAnnotations;

namespace VotingApp.Api.DTOs;

public class CreateQuestionDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int YesVotes { get; set; }
    public int NoVotes { get; set; }
}

public class CreateVoteDto
{
    [Required]
    public int QuestionId { get; set; }

    [Required]
    public bool IsYes { get; set; }
}

public class VoteDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public bool IsYes { get; set; }
    public DateTime CreatedAt { get; set; }
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
