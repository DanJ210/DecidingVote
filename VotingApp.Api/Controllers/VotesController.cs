using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VotingApp.Api.Data;
using VotingApp.Api.DTOs;
using VotingApp.Api.Models;

namespace VotingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VotesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<VoteDto>> CreateVote(CreateVoteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        // Check if question exists
        var questionExists = await _context.Questions.AnyAsync(q => q.Id == dto.QuestionId);
        if (!questionExists)
            return BadRequest("Question not found");

        // Check if user has already voted on this question
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.QuestionId == dto.QuestionId);

        if (existingVote != null)
            return BadRequest("You have already voted on this question");

        var vote = new Vote
        {
            UserId = userId,
            QuestionId = dto.QuestionId,
            IsYes = dto.IsYes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        var voteDto = new VoteDto
        {
            Id = vote.Id,
            QuestionId = vote.QuestionId,
            IsYes = vote.IsYes,
            CreatedAt = vote.CreatedAt
        };

        return CreatedAtAction(nameof(GetVote), new { id = vote.Id }, voteDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VoteDto>> GetVote(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var vote = await _context.Votes
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

        if (vote == null)
            return NotFound();

        var voteDto = new VoteDto
        {
            Id = vote.Id,
            QuestionId = vote.QuestionId,
            IsYes = vote.IsYes,
            CreatedAt = vote.CreatedAt
        };

        return Ok(voteDto);
    }

    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<VoteDto>>> GetUserVotes()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var votes = await _context.Votes
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VoteDto
            {
                Id = v.Id,
                QuestionId = v.QuestionId,
                IsYes = v.IsYes,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();

        return Ok(votes);
    }

    [HttpGet("question/{questionId}")]
    public async Task<ActionResult<object>> GetQuestionVotes(int questionId)
    {
        var questionExists = await _context.Questions.AnyAsync(q => q.Id == questionId);
        if (!questionExists)
            return NotFound("Question not found");

        var votes = await _context.Votes
            .Where(v => v.QuestionId == questionId)
            .ToListAsync();

        var result = new
        {
            QuestionId = questionId,
            TotalVotes = votes.Count,
            YesVotes = votes.Count(v => v.IsYes),
            NoVotes = votes.Count(v => !v.IsYes),
            YesPercentage = votes.Count > 0 ? (double)votes.Count(v => v.IsYes) / votes.Count * 100 : 0,
            NoPercentage = votes.Count > 0 ? (double)votes.Count(v => !v.IsYes) / votes.Count * 100 : 0
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVote(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var vote = await _context.Votes
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

        if (vote == null)
            return NotFound();

        _context.Votes.Remove(vote);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
