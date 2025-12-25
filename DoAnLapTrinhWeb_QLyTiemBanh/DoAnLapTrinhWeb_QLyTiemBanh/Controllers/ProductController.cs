
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using DoAnLapTrinhWeb_QLyTiemBanh.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers
{

    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IReviewService _reviewService;
        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IReviewRepository reviewRepository,
            IReviewService reviewService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
            _reviewService = reviewService;
        }
        // Hiển thị danh sách sản phẩm 
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
        // Hiển thị thông tin chi tiết sản phẩm 
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            // Lấy danh sách review để hiển thị ra View
            var reviews = await _reviewRepository.GetByProductIdAsync(id);
            ViewBag.Reviews = reviews;
            return View(product);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostReview(int productId, string comment, int rating)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return RedirectToAction("Details", new { id = productId });
            }

            // Lấy ID của người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Gọi Service và nhận kết quả trả về dạng chuỗi
            var result = await _reviewService.SubmitReviewAsync(productId, userId, comment, rating);

            if (result == "Success")
            {
                TempData["ReviewSuccess"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            }
            else
            {
                // Gửi thông báo lỗi (ví dụ: "Bạn chưa mua hàng") sang View
                TempData["ReviewError"] = result;
            }

            return RedirectToAction("Details", new { id = productId });
        }

    }
}
