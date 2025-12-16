using System.Text.Encodings.Web;
using System.Text;
using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;

        public AccountApiController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            _emailSender = emailSender;
        }

        // POST: /api/accountapi/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, [FromQuery] string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            // Email đã tồn tại?
            var existed = await _userManager.FindByEmailAsync(req.Email);
            if (existed != null)
                return Conflict(new { message = "Email này đã được đăng ký rồi, vui lòng dùng email khác." });

            var user = new ApplicationUser
            {
                FullName = req.FullName,
                Address = req.Address,
                PhoneNumber = req.PhoneNumber
            };

            await _userStore.SetUserNameAsync(user, req.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, req.Email, CancellationToken.None);

            var createResult = await _userManager.CreateAsync(user, req.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Tạo tài khoản thất bại",
                    errors = createResult.Errors.Select(e => e.Description)
                });
            }

            const string defaultRole = SD.Role_Customer;
            if (await _roleManager.RoleExistsAsync(defaultRole))
            {
                await _userManager.AddToRoleAsync(user, defaultRole);
            }
            else
            {
                await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                await _userManager.AddToRoleAsync(user, defaultRole);
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var callbackUrl = $"{baseUrl}/Identity/Account/ConfirmEmail?userId={userId}&code={codeEncoded}";

            await _emailSender.SendEmailAsync(req.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return CreatedAtAction(nameof(GetProfile), new { id = userId }, new
            {
                id = userId,
                fullName = user.FullName,
                email = req.Email,
                phoneNumber = user.PhoneNumber,
                address = user.Address,
                confirmEmail = new { userId, code = codeEncoded, callbackUrl }
            });
        }

        // GET: /api/accountapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                address = user.Address,
                roles
            });
        }

        // POST: /api/accountapi/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null) return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            var ok = await _userManager.CheckPasswordAsync(user, req.Password);
            if (!ok) return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                accessToken = token,
                tokenType = "Bearer",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    phoneNumber = user.PhoneNumber,
                    address = user.Address,
                    roles
                }
            });
        }

        // PUT: /api/accountapi/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateProfileRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            user.FullName = req.FullName;
            user.Email = req.Email;
            user.PhoneNumber = req.PhoneNumber;
            user.Address = req.Address;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Cập nhật thất bại",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new
            {
                message = "Cập nhật hồ sơ thành công",
                profile = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    address = user.Address
                }
            });
        }

        // Giữ nguyên các phần resend, forgot-password, reset-password, assign-role, remove-role, change-password như gốc của bạn ↓↓↓

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ForgotPasswordRequest req, [FromQuery] string returnUrl = "/")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null) return Ok(new { message = "Nếu email tồn tại, hệ thống đã gửi lại xác nhận." });

            if (user.EmailConfirmed) return Ok(new { message = "Email đã xác nhận rồi." });

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page("/Account/ConfirmEmail", null,
                new { area = "Identity", userId = user.Id, code = codeEncoded, returnUrl }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return Ok(new { message = "Đã gửi lại email xác nhận" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return Ok(new { message = "Nếu email hợp lệ, hệ thống đã gửi hướng dẫn đặt lại mật khẩu." });

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            return Ok(new { message = "Đã tạo token đặt lại mật khẩu", userId = user.Id, code = codeEncoded });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

            var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(req.Code));
            var result = await _userManager.ResetPasswordAsync(user, decoded, req.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { message = "Đặt lại mật khẩu thất bại", errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Đặt lại mật khẩu thành công" });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { message = "Đổi mật khẩu thất bại", errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] RoleChangeRequest req)
        {
            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

            if (!await _roleManager.RoleExistsAsync(req.Role))
                return BadRequest(new { message = "Role không tồn tại" });

            var result = await _userManager.AddToRoleAsync(user, req.Role);
            if (!result.Succeeded)
                return BadRequest(new { message = "Gán role thất bại", errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Đã gán role", userId = req.UserId, role = req.Role });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] RoleChangeRequest req)
        {
            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

            if (!await _roleManager.RoleExistsAsync(req.Role))
                return BadRequest(new { message = "Role không tồn tại" });

            var result = await _userManager.RemoveFromRoleAsync(user, req.Role);
            if (!result.Succeeded)
                return BadRequest(new { message = "Gỡ role thất bại", errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Đã gỡ role", userId = req.UserId, role = req.Role });
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email ?? "")
            };

            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var token = new JwtSecurityToken(
                issuer: config["JWT:Issuer"],
                audience: config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Giữ nguyên DTOs của bạn ↓↓↓

    public class RegisterRequest
    {
        [Required] public string FullName { get; set; }
        [Required, Phone] public string PhoneNumber { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        public string Address { get; set; }
        [Required, MinLength(6)] public string Password { get; set; }
    }

    public class LoginRequest
    {
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required] public string FullName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Phone] public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required] public string CurrentPassword { get; set; }
        [Required, MinLength(6)] public string NewPassword { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [Required, EmailAddress] public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required] public string UserId { get; set; }
        [Required] public string Code { get; set; }
        [Required, MinLength(6)] public string NewPassword { get; set; }
    }

    public class RoleChangeRequest
    {
        [Required] public string UserId { get; set; }
        [Required] public string Role { get; set; }
    }
}
