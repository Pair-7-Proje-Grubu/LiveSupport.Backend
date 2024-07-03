using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatAppServer.WebAPI.Hubs
{
    public sealed class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        public static Dictionary<string, Guid> Users = new();

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Connect(Guid userId)
        {
            Users.Add(Context.ConnectionId, userId);
            User? user = await _context.Users.FindAsync(userId);

            if (user is not null)
            {
                user.Status = "online";
                await _context.SaveChangesAsync();

                // Moderatörlere tüm kullanıcıları bildir
                var moderators = await _context.Users.Where(u => u.Role == 1).ToListAsync();
                await Clients.Clients(GetConnectionIds(moderators)).SendAsync("Users", user);

                // Normal kullanıcılara sadece moderatörleri bildir
                if (user.Role == 1)
                {
                    var normalUsers = await _context.Users.Where(u => u.Role == 0).ToListAsync();
                    await Clients.Clients(GetConnectionIds(normalUsers)).SendAsync("Users", user);
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Users.TryGetValue(Context.ConnectionId, out Guid userId))
            {
                Users.Remove(Context.ConnectionId);
                User? user = await _context.Users.FindAsync(userId);

                if (user is not null)
                {
                    user.Status = "offline";
                    await _context.SaveChangesAsync();

                    // Moderatörlere tüm kullanıcıları bildir
                    var moderators = await _context.Users.Where(u => u.Role == 1).ToListAsync();
                    await Clients.Clients(GetConnectionIds(moderators)).SendAsync("Users", user);

                    // Normal kullanıcılara sadece moderatörleri bildir
                    if (user.Role == 1)
                    {
                        var normalUsers = await _context.Users.Where(u => u.Role == 0).ToListAsync();
                        await Clients.Clients(GetConnectionIds(normalUsers)).SendAsync("Users", user);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private List<string> GetConnectionIds(List<User> users)
        {
            return Users
                .Where(u => users.Any(user => user.Id == u.Value))
                .Select(u => u.Key)
                .ToList();
        }
    }
}