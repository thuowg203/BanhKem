using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        // ✅ Trả toàn bộ người dùng (trừ admin)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users
                .Where(u => u.Email != "admin@tiembanh.local")
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email
                })
                .ToList();

            return Ok(users);
        }
    }
}
