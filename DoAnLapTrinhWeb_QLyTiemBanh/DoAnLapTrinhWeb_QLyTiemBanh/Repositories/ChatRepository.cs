using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Repositories
{
    public interface IChatRepository
    {
        Task SaveMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetMessagesAsync(string userId);
    }

    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(string userId)
        {
            const string adminId = "admin@tiembanh.local";
            return await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == userId && m.ReceiverId == adminId) ||
                    (m.SenderId == adminId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}
