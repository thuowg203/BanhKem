using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly IChatRepository _chatRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatApiController(IChatRepository chatRepo, UserManager<ApplicationUser> userManager)
        {
            _chatRepo = chatRepo;
            _userManager = userManager;
        }


        // Lấy lịch sử tin nhắn theo userId
        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Thiếu userId.");

            var messages = await _chatRepo.GetMessagesAsync(userId);
            var user = await _userManager.FindByIdAsync(userId);
            var userName = user?.FullName ?? user?.Email ?? userId;
            // Trả ra dạng dễ đọc cho Flutter
            var result = messages.Select(m => new
            {
                from = m.IsFromAdmin ? "Quản trị viên" : userName,
                to = m.ReceiverId,
                text = m.Message,
                isFromAdmin = m.IsFromAdmin,
                sentAt = m.SentAt
            });

            return Ok(result);
        }
    }
}
