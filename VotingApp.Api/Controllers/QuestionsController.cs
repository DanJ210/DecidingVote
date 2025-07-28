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
public class QuestionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public QuestionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestions()
    {
        var questions = await _context.Questions
            .Include(q => q.User)
            .Include(q => q.Votes)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                Author = q.User.Username,
                CreatedAt = q.CreatedAt,
                YesVotes = q.Votes.Count(v => v.IsYes),
                NoVotes = q.Votes.Count(v => !v.IsYes)
            })
            .ToListAsync();

        return Ok(questions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionDto>> GetQuestion(int id)
    {
        var question = await _context.Questions
            .Include(q => q.User)
            .Include(q => q.Votes)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return NotFound();

        var questionDto = new QuestionDto
        {
            Id = question.Id,
            Title = question.Title,
            Description = question.Description,
            Author = question.User.Username,
            CreatedAt = question.CreatedAt,
            YesVotes = question.Votes.Count(v => v.IsYes),
            NoVotes = question.Votes.Count(v => !v.IsYes)
        };

        return Ok(questionDto);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var question = new Question
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Load the user to get the username
        await _context.Entry(question)
            .Reference(q => q.User)
            .LoadAsync();

        var questionDto = new QuestionDto
        {
            Id = question.Id,
            Title = question.Title,
            Description = question.Description,
            Author = question.User.Username,
            CreatedAt = question.CreatedAt,
            YesVotes = 0,
            NoVotes = 0
        };

        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, questionDto);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetUserQuestions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var questions = await _context.Questions
            .Include(q => q.User)
            .Include(q => q.Votes)
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                Author = q.User.Username,
                CreatedAt = q.CreatedAt,
                YesVotes = q.Votes.Count(v => v.IsYes),
                NoVotes = q.Votes.Count(v => !v.IsYes)
            })
            .ToListAsync();

        return Ok(questions);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id);
        if (question == null)
            return NotFound();

        if (question.UserId != userId)
            return Forbid();

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
