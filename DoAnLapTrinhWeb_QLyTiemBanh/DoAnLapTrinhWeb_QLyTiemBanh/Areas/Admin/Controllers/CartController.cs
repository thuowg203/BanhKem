using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;

        public CartController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        // ✅ Danh sách giỏ hàng người dùng
        public async Task<IActionResult> UserCarts()
        {
            // Lấy toàn bộ giỏ hàng cùng CartItems và ApplicationUser
            var carts = await _cartRepo.GetAllCartsAsync();

            // Gộp thông tin theo từng người dùng
            var model = carts.Select(c => new
            {
                UserId = c.ApplicationUser?.Id ?? "",
                UserName = c.ApplicationUser?.FullName ?? "(Không rõ tên)",
                UserEmail = c.ApplicationUser?.Email ?? "(Không có email)",
                TotalItems = c.CartItems?.Sum(i => i.Quantity) ?? 0,
                TotalValue = c.CartItems?.Sum(i => i.Quantity * i.Price) ?? 0m,
                Items = c.CartItems?.ToList() ?? new List<CartItem>()
            }).ToList();

            return View(model);
        }

        // ✅ Xem chi tiết giỏ hàng từng người
        public async Task<IActionResult> Details(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var cart = await _cartRepo.GetCartAsync(userId);
            if (cart == null)
                return NotFound();

            // Tính tổng tiền của giỏ
            ViewBag.TotalValue = cart.CartItems?.Sum(i => i.Quantity * i.Price) ?? 0m;

            return View(cart);
        }
    }
}
