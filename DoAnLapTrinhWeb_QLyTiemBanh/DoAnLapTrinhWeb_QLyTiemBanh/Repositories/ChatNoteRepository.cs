using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Repositories
{
    public interface IChatNoteRepository
    {
        Task<List<ChatNote>> GetNotesByUserIdAsync(string userId);
        Task AddNoteAsync(ChatNote note);
    }

    public class ChatNoteRepository : IChatNoteRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatNoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatNote>> GetNotesByUserIdAsync(string userId)
        {
            return await _context.ChatNotes
                .Where(n => n.UserId == userId)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task AddNoteAsync(ChatNote note)
        {
            _context.ChatNotes.Add(note);
            await _context.SaveChangesAsync();
        }
    }
}
