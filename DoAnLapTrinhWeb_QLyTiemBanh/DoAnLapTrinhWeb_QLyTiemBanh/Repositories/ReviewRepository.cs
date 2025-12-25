using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.EntityFrameworkCore;
namespace DoAnLapTrinhWeb_QLyTiemBanh.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;
        public ReviewRepository(ApplicationDbContext context) => _context = context;

        public async Task AddAsync(ProductReview review)
        {
            await _context.ProductReviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductReview>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductReviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }
    }
}
