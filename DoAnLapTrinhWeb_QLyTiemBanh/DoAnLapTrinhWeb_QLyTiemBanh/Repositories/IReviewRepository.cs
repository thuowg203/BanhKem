using DoAnLapTrinhWeb_QLyTiemBanh.Models;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Repositories
{
    public interface IReviewRepository
    {
        Task AddAsync(ProductReview review);
        Task<IEnumerable<ProductReview>> GetByProductIdAsync(int productId);
    }
}
