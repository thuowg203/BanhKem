
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers
{

    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
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
            return View(product);
        }
        public async Task<IActionResult> ComboGift()
        {
            // Lấy tất cả sản phẩm và bao gồm cả thông tin Category
            var allProducts = await _productRepository.GetAllAsync();

            // Lọc ra những sản phẩm thuộc danh mục "Combo/Gift"
            // (Hãy chắc chắn rằng bạn có một Category với tên chính xác là "Combo/Gift" trong database)
            var comboProducts = allProducts.Where(p => p.Category?.TenLoai == "Combo/Gift").ToList();

            // Nếu không có danh mục nào khớp, bạn có thể hiển thị tất cả sản phẩm
            // hoặc trả về một view trống tùy theo yêu cầu
            if (!comboProducts.Any())
            {
                // Tùy chọn: nếu không tìm thấy, có thể hiển thị tất cả sản phẩm nổi bật
                // return View(allProducts); 
            }

            // Trả về view "ComboGift.cshtml" cùng với danh sách sản phẩm đã lọc
            return View(comboProducts);
        }

    }
}
