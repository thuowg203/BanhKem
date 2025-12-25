using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ApplicationDbContext _context;

        public ReviewService(IReviewRepository reviewRepository, ApplicationDbContext context)
        {
            _reviewRepository = reviewRepository;
            _context = context;
        }

        public async Task<string> SubmitReviewAsync(int productId, string userId, string comment, int rating)
        {
            var hasPurchased = await _context.OrderDetails
                .AnyAsync(od => od.ProductId == productId && od.Order.UserId == userId);

            if (!hasPurchased)
            {
                return "Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua hàng.";
            }

            string[] negativeWords = { "tệ", "dở", "kém", "không ngon", "thất vọng", "xấu" };
            bool isPositive = (rating >= 4) && !negativeWords.Any(w => comment.ToLower().Contains(w));

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                Comment = comment,
                Rating = rating,
                IsPositive = isPositive,
                CreatedDate = DateTime.Now
            };
            try
            {
                await _reviewRepository.AddAsync(review);
                return "Success";
            }
            catch (Exception)
            {
                return "Đã xảy ra lỗi khi lưu đánh giá. Vui lòng thử lại sau.";
            }
        }
    }
}
