using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Action để hiển thị danh sách người dùng
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UserRolesViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return View(userRolesViewModel);
        }

        // GET: Hiển thị form tạo người dùng mới
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roles = await _context.Roles.Select(r => r.Name).ToListAsync();

            if (roles == null || !roles.Any())
            {
                ModelState.AddModelError(string.Empty, "Không có vai trò nào trong hệ thống.");
                return View();
            }

            ViewData["Roles"] = new SelectList(roles);

            return View();
        }

        // POST: Tạo người dùng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký rồi.");
            }
            // Tạo người dùng mới mà không cần kiểm tra ModelState
            var user = new ApplicationUser
            {
                UserName = model.Email,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Nếu không có vai trò nào được chọn, gán vai trò mặc định là "Customer"
                if (model.Roles == null || !model.Roles.Any())
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }
                else
                {
                    // Nếu có vai trò được chọn, gán các vai trò đó
                    foreach (var role in model.Roles)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            // Nếu có lỗi, thêm các thông báo lỗi vào ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Nếu có lỗi, trả lại view cùng với các lỗi
            var rolesList = await _context.Roles.Select(r => r.Name).ToListAsync();
            ViewData["Roles"] = new SelectList(rolesList);
            return View(model);
        }
        // GET: Hiển thị xác nhận xóa người dùng
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user); // Trả về view để xác nhận xóa
        }

        // Action xử lý xóa người dùng
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index)); // Quay lại danh sách người dùng
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(); // Trả lại view nếu có lỗi
        }



    }
    // ViewModel để chứa thông tin người dùng và vai trò
    public class UserRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string UserName { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
        [Display(Name = "Vai trò")]
        public List<string> Roles { get; set; } = new List<string>();
    }
}