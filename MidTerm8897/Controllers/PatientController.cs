using Microsoft.AspNetCore.Mvc;
using MidTerm8897.Models;
using System.Threading.Tasks;

namespace MidTerm8897.Controllers
{
    public class PatientController : Controller
    {
        private readonly IPatientRepository _patientRepository;

        public PatientController(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var patients = await _patientRepository.GetAllPatientsAsync();
                return View(patients);
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine($"InvalidCastException in Index: {ex.Message}");
                return View("Error");
            }
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Patients patient)
        {
            if (ModelState.IsValid)
            {
                await _patientRepository.AddPatientAsync(patient);
                return RedirectToAction("Index");
            }
            return View(patient);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        public async Task<IActionResult> Update(int id)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Patients patient)
        {
            if (id != patient.id_patient)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _patientRepository.UpdatePatientAsync(patient);
                return RedirectToAction("Index");
            }
            return View(patient);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _patientRepository.DeletePatientAsync(id);
                return RedirectToAction("Index");
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine($"InvalidCastException in DeleteConfirmed: {ex.Message}");
                return View("Error");
            }
        }
    }
}