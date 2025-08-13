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
                Side1Text = q.Side1Text,
                Side2Text = q.Side2Text,
                Author = q.User.UserName ?? string.Empty,
                CreatedAt = q.CreatedAt,
                Side1Votes = q.Votes.Count(v => v.Choice == VoteChoice.Side1),
                Side2Votes = q.Votes.Count(v => v.Choice == VoteChoice.Side2)
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
            Side1Text = question.Side1Text,
            Side2Text = question.Side2Text,
            Author = question.User.UserName ?? string.Empty,
            CreatedAt = question.CreatedAt,
            Side1Votes = question.Votes.Count(v => v.Choice == VoteChoice.Side1),
            Side2Votes = question.Votes.Count(v => v.Choice == VoteChoice.Side2)
        };

        return Ok(questionDto);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionDto dto)
    {
        // Debug logging
        Console.WriteLine("=== CreateQuestion endpoint hit ===");
        Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
        Console.WriteLine($"Claims count: {User.Claims.Count()}");
        
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
        }
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        Console.WriteLine($"Extracted userId: {userId}");
        
        if (userId == null)
        {
            Console.WriteLine("UserId is null - returning Unauthorized");
            return Unauthorized();
        }

        var question = new Question
        {
            Title = dto.Title,
            Description = dto.Description,
            Side1Text = dto.Side1Text,
            Side2Text = dto.Side2Text,
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
            Side1Text = question.Side1Text,
            Side2Text = question.Side2Text,
            Author = question.User.UserName ?? string.Empty,
            CreatedAt = question.CreatedAt,
            Side1Votes = 0,
            Side2Votes = 0
        };

        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, questionDto);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetUserQuestions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
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
                Side1Text = q.Side1Text,
                Side2Text = q.Side2Text,
                Author = q.User.UserName ?? string.Empty,
                CreatedAt = q.CreatedAt,
                Side1Votes = q.Votes.Count(v => v.Choice == VoteChoice.Side1),
                Side2Votes = q.Votes.Count(v => v.Choice == VoteChoice.Side2)
            })
            .ToListAsync();

        return Ok(questions);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
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
