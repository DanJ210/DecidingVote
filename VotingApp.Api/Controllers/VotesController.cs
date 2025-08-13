using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
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

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
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
            Choice = dto.Choice,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        var voteDto = new VoteDto
        {
            Id = vote.Id,
            QuestionId = vote.QuestionId,
            Choice = vote.Choice,
            CreatedAt = vote.CreatedAt,
            LastModifiedAt = vote.LastModifiedAt
        };

        return CreatedAtAction(nameof(GetVote), new { id = vote.Id }, voteDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VoteDto>> GetVote(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
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
            Choice = vote.Choice,
            CreatedAt = vote.CreatedAt,
            LastModifiedAt = vote.LastModifiedAt
        };

        return Ok(voteDto);
    }

    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<VoteDto>>> GetUserVotes()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userId == null)
            return Unauthorized();

        var votes = await _context.Votes
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VoteDto
            {
                Id = v.Id,
                QuestionId = v.QuestionId,
                Choice = v.Choice,
                CreatedAt = v.CreatedAt,
                LastModifiedAt = v.LastModifiedAt
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
            Side1Votes = votes.Count(v => v.Choice == VoteChoice.Side1),
            Side2Votes = votes.Count(v => v.Choice == VoteChoice.Side2),
            Side1Percentage = votes.Count > 0 ? (double)votes.Count(v => v.Choice == VoteChoice.Side1) / votes.Count * 100 : 0,
            Side2Percentage = votes.Count > 0 ? (double)votes.Count(v => v.Choice == VoteChoice.Side2) / votes.Count * 100 : 0
        };

        return Ok(result);
    }

    [HttpGet("can-change/{questionId}")]
    public async Task<ActionResult<object>> CanChangeVote(int questionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userId == null)
            return Unauthorized();

        var vote = await _context.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.QuestionId == questionId);

        if (vote == null)
            return Ok(new { CanChange = false, HasVoted = false });

        var hoursSinceVote = (DateTime.UtcNow - vote.CreatedAt).TotalHours;
        var canChange = hoursSinceVote < 24;
        var timeRemaining = canChange ? 24 - hoursSinceVote : 0;

        return Ok(new 
        { 
            CanChange = canChange, 
            HasVoted = true,
            CurrentChoice = vote.Choice,
            HoursSinceVote = Math.Round(hoursSinceVote, 2),
            HoursRemaining = Math.Round(Math.Max(0, timeRemaining), 2),
            VotedAt = vote.CreatedAt,
            LastModified = vote.LastModifiedAt
        });
    }

    [HttpPut("change")]
    public async Task<ActionResult<VoteDto>> ChangeVote(CreateVoteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userId == null)
            return Unauthorized();

        // Check if question exists
        var questionExists = await _context.Questions.AnyAsync(q => q.Id == dto.QuestionId);
        if (!questionExists)
            return BadRequest("Question not found");

        // Find existing vote
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.QuestionId == dto.QuestionId);

        if (existingVote == null)
            return BadRequest("You have not voted on this question yet");

        // Check if vote can still be changed (within 24 hours)
        var hoursSinceVote = (DateTime.UtcNow - existingVote.CreatedAt).TotalHours;
        if (hoursSinceVote >= 24)
            return BadRequest($"Vote cannot be changed after 24 hours. Your vote was cast {Math.Round(hoursSinceVote, 1)} hours ago.");

        // Update the vote
        existingVote.Choice = dto.Choice;
        existingVote.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var voteDto = new VoteDto
        {
            Id = existingVote.Id,
            QuestionId = existingVote.QuestionId,
            Choice = existingVote.Choice,
            CreatedAt = existingVote.CreatedAt,
            LastModifiedAt = existingVote.LastModifiedAt
        };

        return Ok(voteDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVote(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
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
