using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Tạo hoặc lấy giỏ hàng theo UserId
        public async Task<UserCart> GetOrCreateUserCartAsync(string userId)
        {
            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new UserCart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _context.UserCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // ✅ Thêm sản phẩm vào giỏ (dùng UserId, KHÔNG dùng email)
        public async Task<bool> AddToCartAsync(string userId, CartItem item)
        {
            var cart = await GetOrCreateUserCartAsync(userId);

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == item.ProductId && ci.UserCartId == cart.Id);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
                existing.Price = item.Price;
                existing.Name = item.Name;
                existing.ImageUrl = item.ImageUrl;
            }
            else
            {
                item.UserCartId = cart.Id;
                _context.CartItems.Add(item);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Lấy danh sách item theo UserId
        public async Task<List<CartItem>> GetCartItemsByUserAsync(string userId)
        {
            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.CartItems?.ToList() ?? new List<CartItem>();
        }

        // ✅ Xóa 1 item trong giỏ
        public async Task RemoveItemAsync(string userId, int itemId)
        {
            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return;

            var item = cart.CartItems.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Lấy toàn bộ UserCart (bao gồm ApplicationUser)
        public async Task<UserCart?> GetCartAsync(string userId)
        {
            return await _context.UserCarts
                .Include(c => c.ApplicationUser) // ⚡ Load email + fullname
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // ✅ Admin: Lấy tất cả giỏ hàng (có user + items)
        public async Task<List<UserCart>> GetAllCartsAsync()
        {
            return await _context.UserCarts
                .Include(c => c.ApplicationUser)
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Product)
                .ToListAsync();
        }

        // ✅ Xóa toàn bộ giỏ hàng của 1 user
        public async Task ClearCartAsync(string userId)
        {
            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return;

            if (cart.CartItems != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
            }

            await _context.SaveChangesAsync();
        }
    }
}
