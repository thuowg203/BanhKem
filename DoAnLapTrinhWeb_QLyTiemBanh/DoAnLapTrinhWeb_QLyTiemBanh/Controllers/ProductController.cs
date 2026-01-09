
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
        private readonly SentimentService _sentimentService;
        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IReviewRepository reviewRepository,
            IReviewService reviewService,
            SentimentService sentimentService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
            _reviewService = reviewService;
            _sentimentService = sentimentService;
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
                TempData["ReviewError"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction("Details", new { id = productId });
            }

            // --- KIỂM TRA BẰNG AI ---
            var prediction = _sentimentService.Predict(comment);

           
            // Nếu AI đoán là Tiêu cực (Prediction == false) => Chặn luôn, không cần quan tâm độ tin cậy bao nhiêu
            if (!prediction.Prediction)
            {
                TempData["ReviewError"] = "Bình luận của bạn chứa nội dung không phù hợp hoặc tiêu cực. Vui lòng kiểm tra lại!";
                // Dừng luôn, không lưu vào DB nữa
                return RedirectToAction("Details", new { id = productId });
            }
            // -------------------------

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Vì đã chặn ở trên, nên nếu chạy xuống đây thì mặc định là Tích cực (true)
            var result = await _reviewService.SubmitReviewAsync(productId, userId, comment, rating, true);

            if (result == "Success")
            {
                TempData["ReviewSuccess"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            }
            else
            {
                TempData["ReviewError"] = result;
            }

            return RedirectToAction("Details", new { id = productId });
        }

    }
}
