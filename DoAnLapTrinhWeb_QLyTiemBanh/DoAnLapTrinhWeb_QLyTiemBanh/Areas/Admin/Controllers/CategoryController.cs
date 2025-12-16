using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        // Hiển thị danh sách loại sản phẩm 
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            bool exists = await _categoryRepository.ExistsByNameAsync(category.TenLoai);
            if (exists)
            {
                ModelState.AddModelError("TenLoai", "Tên loại sản phẩm đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            await _categoryRepository.CreateAsync(category);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            bool exists = await _categoryRepository.ExistsByNameExceptIdAsync(category.TenLoai, id);
            if (exists)
            {
                ModelState.AddModelError("TenLoai", "Tên loại sản phẩm đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            await _categoryRepository.UpdateAsync(category);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasProducts = await _categoryRepository.HasProductsAsync(id);
            if (hasProducts)
            {
                TempData["Error"] = "Không thể xóa loại sản phẩm vì đang có sản phẩm thuộc loại này.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
