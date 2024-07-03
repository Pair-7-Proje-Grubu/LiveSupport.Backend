using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAppServer.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
        {
            bool isNameExists = await _context.Users.AnyAsync(p => p.Name == request.Name, cancellationToken);
            if (isNameExists)
            {
                return BadRequest(new { Message = "Bu kullanıcı adı daha önce kullanılmış." });
            }

            bool isEmailExists = await _context.Users.AnyAsync(p => p.Email == request.Email, cancellationToken);
            if (isEmailExists)
            {
                return BadRequest(new { Message = "Bu email adresi daha önce kullanılmış." });
            }

            User user = new()
            {
                Name = request.Name,
                Email = request.Email,
                Role = 0 // Varsayılan olarak normal kullanıcı
            };

            await _context.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string name, string email, CancellationToken cancellationToken)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(p => p.Name == name && p.Email == email, cancellationToken);
            if (user == null)
            {
                return BadRequest(new { Message = "Kullanıcı adı veya email hatalı." });
            }
            user.Status = "online";
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(user);
        }

    }
        
}