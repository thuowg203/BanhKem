using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Identity;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepo;
        private readonly IChatNoteRepository _chatNoteRepo;
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatHub(IChatRepository chatRepo, IChatNoteRepository chatNoteRepo, UserManager<ApplicationUser> userManager)
        {
            _chatRepo = chatRepo;
            _chatNoteRepo = chatNoteRepo;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                _onlineUsers[userId] = Context.ConnectionId;
                // Lấy user từ DB
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    // Nếu user thuộc role Admin → cho vào group
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                        Console.WriteLine($"Admin {user.Email} đã vào nhóm Admins");
                    }
                }

                // Gửi danh sách người dùng online cho admin
                await Clients.All.SendAsync("OnlineUsersUpdated", _onlineUsers.Keys);

                // Gửi tin nhắn hệ thống
                await Clients.Caller.SendAsync("ReceiveMessage", "Hệ thống", "✅ Kết nối trò chuyện thành công!");
                Console.WriteLine($"🟢 {userId} đã kết nối ({Context.ConnectionId})");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _onlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(user.Key))
            {
                _onlineUsers.TryRemove(user.Key, out _);
                await Clients.All.SendAsync("OnlineUsersUpdated", _onlineUsers.Keys);
                Console.WriteLine($"🔴 {user.Key} đã ngắt kết nối.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Người dùng gửi tin đến admin
        public async Task SendMessageToAdmin(string fromUserId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            // Lưu vào DB
            await _chatRepo.SaveMessageAsync(new ChatMessage
            {
                SenderId = fromUserId,
                ReceiverId = "Admins",
                Message = message,
                IsFromAdmin = false,
                SentAt = DateTime.UtcNow
            });

            await Clients.Group("Admins").SendAsync("ReceiveMessage", fromUserId, message);


            // Gửi phản hồi cho user
            await Clients.Caller.SendAsync("ReceiveMessage", "Bạn", message);
        }

        // Admin gửi tin đến người dùng
        public async Task SendMessageToUser(string toUserId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            // Lưu vào DB
            await _chatRepo.SaveMessageAsync(new ChatMessage
            {
                SenderId = "admin@tiembanh.local",
                ReceiverId = toUserId,
                Message = message,
                IsFromAdmin = true,
                SentAt = DateTime.UtcNow
            });

            // Gửi cho user (nếu online)
            if (_onlineUsers.TryGetValue(toUserId, out string? userConn))
            {
                await Clients.Client(userConn).SendAsync("ReceiveMessage", "Quản trị viên", message);
            }
        }

        // Admin có thể load lịch sử khi chọn user
        public async Task LoadHistory(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var messages = await _chatRepo.GetMessagesAsync(userId);

            Console.WriteLine($"LoadHistory cho {userId}: {messages.Count} tin nhắn");

            await Clients.Caller.SendAsync("LoadChatHistory", messages);
        }
        // Admin gửi note
        public async Task SendCustomerNote(string adminId, string userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrEmpty(userId)) return;

            var note = new ChatNote
            {
                UserId = userId,
                AdminId = adminId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            await _chatNoteRepo.AddNoteAsync(note);

            await Clients.Group("Admins").SendAsync("ReceiveCustomerNote", note);
        }


        // Admin load tất cả note liên quan của từng khách
        public async Task<List<ChatNote>> LoadCustomerNotes(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<ChatNote>();

            var notes = await _chatNoteRepo.GetNotesByUserIdAsync(userId);
            return notes;
        }




    }
}
