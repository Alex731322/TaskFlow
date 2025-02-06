using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(IJwtService jwtService, AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _jwtService = jwtService;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("User already exists.");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(null!, dto.Password),
                Role = dto.Email.EndsWith("@admin.com") ? "Admin" : "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.RefreshToken)
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Если пользователь не найден или пароль неверный
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) != PasswordVerificationResult.Success)
                return Unauthorized("Invalid credentials.");

            // Обновляем RefreshToken
            var refreshToken = _jwtService.GenerateRefreshToken();
          
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.RefreshToken = refreshToken;
                _context.Entry(refreshToken).State = EntityState.Added; // Явное добавление
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Please try again.");
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            return Ok(new AuthResponseDto(accessToken, refreshToken.Token));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(string refreshToken)
        {
            var user = await _context.Users
                .Include(u => u.RefreshToken)
                .AsTracking()
                .FirstOrDefaultAsync(u => u.RefreshToken != null && u.RefreshToken.Token == refreshToken);

            if (user == null || user.RefreshToken!.Expires < DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();



            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.RefreshToken = newRefreshToken;
                _context.Entry(newRefreshToken).State = EntityState.Added; // Явное добавление
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Please try again.");
            }

            return Ok(new AuthResponseDto(newAccessToken, newRefreshToken.Token));
        }

        [HttpGet("admin/data")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetAdminData()
        {
            return Ok("Secret data for admins");
        }

    }
}
