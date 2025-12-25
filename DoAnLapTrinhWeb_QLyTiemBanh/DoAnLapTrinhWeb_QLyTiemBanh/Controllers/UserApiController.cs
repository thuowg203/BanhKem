using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserApiController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        //Trả toàn bộ người dùng (trừ admin)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            // Lấy danh sách admin
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = admins.Select(a => a.Id).ToList();

            // Chỉ lấy user KHÔNG phải admin
            var users = await _userManager.Users
                .Where(u => !adminIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
