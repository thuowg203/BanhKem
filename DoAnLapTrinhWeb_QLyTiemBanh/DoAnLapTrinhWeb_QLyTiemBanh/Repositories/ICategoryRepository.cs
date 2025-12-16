using DoAnLapTrinhWeb_QLyTiemBanh.Models;
namespace DoAnLapTrinhWeb_QLyTiemBanh.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameExceptIdAsync(string name, int excludeId);
        Task<bool> HasProductsAsync(int categoryId);
    }
}
