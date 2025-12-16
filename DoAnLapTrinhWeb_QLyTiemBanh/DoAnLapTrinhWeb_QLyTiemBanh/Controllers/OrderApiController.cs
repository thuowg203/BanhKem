using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// 🧾 Lấy danh sách đơn hàng của người dùng đang đăng nhập (có thể lọc theo trạng thái)
        /// GET: /api/OrdersApi/history?status=ChoXacNhan
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetOrderHistory([FromQuery] string? status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var selectedStatus))
            {
                query = query.Where(o => o.OrderStatus == selectedStatus);
            }

            var orders = await query.Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.TotalPrice,
                o.PaymentMethod,
                o.PaymentStatus,
                o.OrderStatus,
                o.RecipientName,
                o.RecipientPhone,
                o.SpecificAddress,
                o.Ward,
                o.District,
                o.DeliveryDateTime,
                o.Notes,
                Details = o.OrderDetails.Select(od => new
                {
                    od.ProductId,
                    ProductName = od.Product.Name,
                    od.Quantity,
                    od.Price,
                    od.Notes,
                    Image = od.Product.Image
                })
            }).ToListAsync();

            return Ok(orders);
        }

        /// <summary>
        /// 📦 Lấy chi tiết 1 đơn hàng cụ thể
        /// GET: /api/OrdersApi/detail/{id}
        /// </summary>
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng này." });

            var result = new
            {
                order.Id,
                order.OrderDate,
                order.TotalPrice,
                order.PaymentMethod,
                order.PaymentStatus,
                order.OrderStatus,
                order.RecipientName,
                order.RecipientPhone,
                order.SpecificAddress,
                order.Ward,
                order.District,
                order.DeliveryDateTime,
                order.Notes,
                Details = order.OrderDetails.Select(od => new
                {
                    od.ProductId,
                    ProductName = od.Product.Name,
                    od.Quantity,
                    od.Price,
                    od.Notes,
                    Image = od.Product.Image
                })
            };

            return Ok(result);
        }

        /// <summary>
        /// ❌ Hủy đơn hàng (chỉ khi trạng thái = Chờ xác nhận)
        /// POST: /api/OrdersApi/cancel/{id}
        /// </summary>
        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng" });

            if (order.OrderStatus != OrderStatus.ChoXacNhan)
                return BadRequest(new { message = "Chỉ có thể hủy đơn hàng khi đang chờ xác nhận." });

            order.OrderStatus = OrderStatus.DaHuy;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"🗑️ Đã hủy đơn hàng #{order.Id} thành công." });
        }
    }
}
