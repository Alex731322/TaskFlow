using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using TaskFlow.Core.Entities;
using TaskFlow.Infrastructure.Data;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PostsController(AppDbContext context)
    {
        _context = context;
    }

    // Создание поста
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            UserId = dto.UserId
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return Ok(post);
    }

    // Получение постов пользователя
    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUserPosts(string username)
    {
        var posts = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.User.Username == username)
            .ToListAsync();
        
        if (posts == null) return NotFound();

        return Ok(posts);
    }

    [HttpGet("feed")]
    [Authorize]
    public async Task<IActionResult> GetFeed()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Получаем ID пользователей, на которых подписан текущий пользователь
        var followedUsers = await _context.Follows
            .Where(f => f.FollowerId == Guid.Parse(userId))
            .Select(f => f.FollowedId)
            .ToListAsync();

        // Получаем посты этих пользователей
        var posts = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => followedUsers.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(posts);
    }
}