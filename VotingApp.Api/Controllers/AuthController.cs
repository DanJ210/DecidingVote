using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VotingApp.Api.Data;
using VotingApp.Api.DTOs;
using VotingApp.Api.Models;

namespace VotingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        var token = await GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = await GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty
        });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Questions)
            .Include(u => u.Votes)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            CreatedAt = user.CreatedAt,
            QuestionsCount = user.Questions.Count,
            VotesCount = user.Votes.Count
        });
    }

    [HttpGet("test")]
    [Authorize]
    public IActionResult TestAuth()
    {
        Console.WriteLine("=== TestAuth endpoint hit ===");
        Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
        
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"UserId from claims: {userId}");
        
        return Ok(new { 
            message = "Authentication successful!", 
            userId = userId,
            userName = User.Identity?.Name,
            isAuthenticated = User.Identity?.IsAuthenticated
        });
    }

    private Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-256-bits-long";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "VotingApp",
            audience: _configuration["Jwt:Audience"] ?? "VotingApp",
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        // Debug logging
        Console.WriteLine($"Generated JWT token for user {user.UserName}");
        Console.WriteLine($"Token length: {tokenString.Length}");
        Console.WriteLine($"JWT Key: {jwtKey.Substring(0, 10)}...");
        
        return Task.FromResult(tokenString);
    }
}
