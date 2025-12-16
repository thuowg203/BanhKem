using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepo;
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();

        public ChatHub(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                _onlineUsers[userId] = Context.ConnectionId;

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

        // 🟢 Người dùng gửi tin đến admin
        public async Task SendMessageToAdmin(string fromUserId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            // Lưu vào DB
            await _chatRepo.SaveMessageAsync(new ChatMessage
            {
                SenderId = fromUserId,
                ReceiverId = "admin@tiembanh.local",
                Message = message,
                IsFromAdmin = false,
                SentAt = DateTime.UtcNow
            });

            // Gửi cho admin (nếu đang online)
            if (_onlineUsers.TryGetValue("admin@tiembanh.local", out string? adminConn))
            {
                await Clients.Client(adminConn).SendAsync("ReceiveMessage", fromUserId, message);
            }

            // Gửi phản hồi cho user
            await Clients.Caller.SendAsync("ReceiveMessage", "Bạn", message);
        }

        // 🟠 Admin gửi tin đến người dùng
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

        // 🟣 Admin có thể load lịch sử khi chọn user
        public async Task LoadHistory(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var messages = await _chatRepo.GetMessagesAsync(userId);

            Console.WriteLine($"📜 LoadHistory cho {userId}: {messages.Count} tin nhắn");

            await Clients.Caller.SendAsync("LoadChatHistory", messages);
        }


    }
}
