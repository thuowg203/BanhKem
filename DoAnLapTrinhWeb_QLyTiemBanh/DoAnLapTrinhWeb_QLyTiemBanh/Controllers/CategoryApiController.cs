using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers.Api
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryApiController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // 🧾 GET: /api/CategoryApi
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();

            if (categories == null || !categories.Any())
                return Ok(new { message = "Không có danh mục nào." });

            var result = categories.Select(c => new
            {
                id = c.Id,
                tenLoai = c.TenLoai
            });

            return Ok(result);
        }

        // 🔍 GET: /api/CategoryApi/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return NotFound(new { message = "Không tìm thấy danh mục." });

            return Ok(new
            {
                id = category.Id,
                tenLoai = category.TenLoai
            });
        }
    }
}
