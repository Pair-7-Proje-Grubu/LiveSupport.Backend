using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Hubs;
using ChatAppServer.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatAppServer.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public sealed class ChatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatsController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(Guid currentUserId)
        {
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null)
                return NotFound("Current user not found");

            List<User> users;
            if (currentUser.Role == 1) // Moderatör
            {
                users = await _context.Users.Where(u => u.Id != currentUserId).OrderBy(p => p.Name).ToListAsync();
            }
            else // Normal kullanıcı
            {
                users = await _context.Users.Where(u => u.Role == 1 && u.Id != currentUserId).OrderBy(p => p.Name).ToListAsync();
            }

            return Ok(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetChats(Guid userId, Guid toUserId, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(userId);
            var toUser = await _context.Users.FindAsync(toUserId);

            if (user == null || toUser == null)
                return NotFound("One or both users not found");

            if (user.Role != 1 && toUser.Role != 1)
                return Forbid("Chat is only allowed between users and moderators");

            List<Chat> chats = await _context.Chats
                .Where(p => (p.UserId == userId && p.ToUserId == toUserId) || (p.ToUserId == userId && p.UserId == toUserId))
                .OrderBy(p => p.Date)
                .ToListAsync(cancellationToken);

            return Ok(chats);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            var toUser = await _context.Users.FindAsync(request.ToUserId);

            if (user == null || toUser == null)
                return NotFound("One or both users not found");

            if (user.Role != 1 && toUser.Role != 1)
                return Forbid("Message sending is only allowed between users and moderators");

            Chat chat = new()
            {
                UserId = request.UserId,
                ToUserId = request.ToUserId,
                Message = request.Message,
                Date = DateTime.Now
            };

            await _context.AddAsync(chat, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            string connectionId = ChatHub.Users.FirstOrDefault(p => p.Value == chat.ToUserId).Key;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("Messages", chat);
            }

            return Ok(chat);
        }
    }
}