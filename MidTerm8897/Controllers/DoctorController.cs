using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MidTerm8897.Models;

namespace MidTerm8897.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors.ToListAsync();
            return View(doctors);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Doctors doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(doctor);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        public async Task<IActionResult> Update(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Doctors doctor, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    doctor.image = "/images/" + fileName;
                }

                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(doctor);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}