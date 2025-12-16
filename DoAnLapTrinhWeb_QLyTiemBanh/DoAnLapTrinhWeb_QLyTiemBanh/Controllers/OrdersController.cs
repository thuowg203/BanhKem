using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers
{

    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult History(string? filter)
        {
            var userEmail = User.Identity?.Name;

            if (userEmail == null)
                return RedirectToAction("Login", "Account");

            // Lưu filter để hiển thị nút đang chọn trong view
            ViewBag.Filter = filter;

            var query = _context.Orders
                .Include(o => o.ApplicationUser)
                .Where(o => o.ApplicationUser.Email == userEmail)
                .AsQueryable();

            
            if (!string.IsNullOrEmpty(filter) && Enum.TryParse<OrderStatus>(filter, out var selectedStatus))
            {
                query = query.Where(o => o.OrderStatus == selectedStatus);
            }

            var orders = query
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userEmail = User.Identity?.Name;
            if (userEmail == null)
                return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == id && o.ApplicationUser.Email == userEmail);

            if (order == null)
                return NotFound();

            //Chỉ cho phép hủy khi đơn hàng vẫn đang chờ xác nhận
            if (order.OrderStatus != OrderStatus.ChoXacNhan)
            {
                TempData["Error"] = "Chỉ có thể hủy đơn hàng khi đang ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction(nameof(History));
            }

            order.OrderStatus = OrderStatus.DaHuy;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"🗑️ Đã hủy đơn hàng #{order.Id} thành công.";
            return RedirectToAction(nameof(History));
        }
        // Trả về chi tiết sản phẩm trong đơn hàng (hiển thị trong modal)
        public IActionResult GetOrderDetails(int id)
        {
            var userEmail = User.Identity?.Name;
            if (userEmail == null)
                return Unauthorized();

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.ApplicationUser)
                .FirstOrDefault(o => o.Id == id && o.ApplicationUser.Email == userEmail);

            if (order == null)
            {
                return Content("<p class='text-danger text-center'>Không tìm thấy đơn hàng.</p>", "text/html");
            }

            // Trả về PartialView hiển thị chi tiết đơn hàng
            return PartialView("_OrderDetailsPartial", order);
        }



    }
}
