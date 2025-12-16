using DoAnLapTrinhWeb_QLyTiemBanh.Models;
namespace DoAnLapTrinhWeb_QLyTiemBanh.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameExceptIdAsync(string name, int excludeId);
    }
}
