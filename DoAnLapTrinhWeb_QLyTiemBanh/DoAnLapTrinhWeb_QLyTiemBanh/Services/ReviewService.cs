using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;

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

        // Đã thêm tham số 'bool isPositive' vào đây
        public async Task<string> SubmitReviewAsync(int productId, string userId, string comment, int rating, bool isPositive)
        {
            // 1. Kiểm tra đã mua hàng chưa (Logic nghiệp vụ giữ nguyên)
            var hasPurchased = await _context.OrderDetails
                .AnyAsync(od => od.ProductId == productId && od.Order.UserId == userId);

            if (!hasPurchased)
            {
                return "Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua hàng.";
            }

            // 2. Tạo đối tượng Review
            // Lưu ý: Chúng ta không còn lọc từ khóa thủ công ở đây nữa
            // vì Controller đã dùng AI để quyết định biến 'isPositive' rồi.
            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                Comment = comment,
                Rating = rating,
                IsPositive = isPositive, // Nhận kết quả từ AI
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