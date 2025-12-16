using DoAnLapTrinhWeb_QLyTiemBanh.Models;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Repositories
{
    public interface ICartRepository
    {
       
        Task<UserCart> GetOrCreateUserCartAsync(string userEmail);
        Task<bool> AddToCartAsync(string userEmail, CartItem item);

        
        Task<List<CartItem>> GetCartItemsByUserAsync(string userEmail);
        Task RemoveItemAsync(string userEmail, int itemId);
        Task<UserCart?> GetCartAsync(string userEmail);
        Task<List<UserCart>> GetAllCartsAsync();
        Task ClearCartAsync(string userEmail);
    }
}
