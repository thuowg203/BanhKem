using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers.Api
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class CartApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;

        public CartApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IProductRepository productRepository)
        {
            _context = context;
            _userManager = userManager;
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Thiếu userId.");

            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // 🔹 Nếu chưa có giỏ, tạo giỏ mới cho user
                cart = new UserCart { UserId = userId };
                _context.UserCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var items = cart.CartItems.Select(i => new
            {
                i.Id,
                i.ProductId,
                ProductName = i.Product?.Name ?? i.Name,
                i.Quantity,
                i.Price,
                i.ImageUrl,
                Category = i.Product?.Category?.TenLoai,
                SubTotal = i.Quantity * i.Price
            }).ToList();

            return Ok(new
            {
                userId = userId,
                totalItems = items.Sum(i => i.Quantity),
                totalValue = items.Sum(i => i.SubTotal),
                items
            });
        }

        // ✅ POST: /api/cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItem model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserId))
                return BadRequest("Thiếu thông tin người dùng hoặc sản phẩm.");

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            var userCart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == model.UserId);

            if (userCart == null)
            {
                userCart = new UserCart { UserId = model.UserId };
                _context.UserCarts.Add(userCart);
                await _context.SaveChangesAsync();
            }

            var product = await _productRepository.GetByIdAsync(model.ProductId);
            if (product == null)
                return NotFound("Sản phẩm không tồn tại.");

            var existingItem = userCart.CartItems.FirstOrDefault(i => i.ProductId == model.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += model.Quantity;
            }
            else
            {
                userCart.CartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    Quantity = model.Quantity,
                    Price = product.Price,
                    Name = product.Name,
                    ImageUrl = product.Image,
                    UserCartId = userCart.Id,
                    UserId = model.UserId  // ✅ đã có trường này
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "✅ Đã thêm sản phẩm vào giỏ hàng." });
        }




        // ✅ PUT: /api/cart/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartItem model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserId))
                return BadRequest("Thiếu thông tin người dùng.");

            var userId = model.UserId;

            var userCart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
                return NotFound("Không tìm thấy giỏ hàng.");

            var item = userCart.CartItems.FirstOrDefault(i => i.ProductId == model.ProductId);
            if (item == null)
                return NotFound("Sản phẩm không tồn tại trong giỏ hàng.");

            item.Quantity = model.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "✅ Đã cập nhật số lượng sản phẩm." });
        }



        // ✅ DELETE: /api/cart/remove?userId=abc123&productId=5
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveItem([FromQuery] string userId, [FromQuery] int productId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Thiếu userId.");

            var userCart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
                return NotFound("Không tìm thấy giỏ hàng.");

            var item = userCart.CartItems.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return NotFound("Sản phẩm không tồn tại trong giỏ.");

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "🗑️ Đã xóa sản phẩm khỏi giỏ hàng." });
        }
    }
}
