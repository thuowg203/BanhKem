using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductApiController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        private string BuildAbsoluteUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            // Chuẩn hóa: loại ~, ./ và đảm bảo bắt đầu bằng '/'
            path = path.Replace("\\", "/");
            if (!path.StartsWith("/")) path = "/" + path;

            // Ví dụ ảnh lưu ở /uploads/xxx.jpg trong wwwroot
            // Nếu DB đã lưu đầy đủ /uploads/xxx.jpg thì giữ nguyên
            // Nếu DB chỉ lưu tên file (xxx.jpg), bạn có thể prefix thủ công:
            // if (!path.StartsWith("/uploads/")) path = "/uploads" + path;

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            return baseUrl + path;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productRepository.GetAllAsync();

            var result = products.Select(p => new
            {
                p.Id,
                name = p.Name,
                description = p.Description,
                price = p.Price,
                quantity = p.Quantity,
                // TRẢ VỀ URL ẢNH ĐẦY ĐỦ
                imageUrl = BuildAbsoluteUrl(p.Image),
                badge = p.Badge,
                categoryName = p.Category?.TenLoai
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            return Ok(new
            {
                product.Id,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                quantity = product.Quantity,
                imageUrl = BuildAbsoluteUrl(product.Image),
                badge = product.Badge,
                categoryName = product.Category?.TenLoai
            });
        }

        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _productRepository.GetAllAsync();
            var filtered = products
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new
                {
                    p.Id,
                    name = p.Name,
                    description = p.Description,
                    price = p.Price,
                    imageUrl = BuildAbsoluteUrl(p.Image),
                    badge = p.Badge,
                    categoryName = p.Category?.TenLoai
                }).ToList();

            if (!filtered.Any())
                return NotFound(new { message = "Không có sản phẩm trong danh mục này" });

            return Ok(filtered);
        }
    }
}
