using Microsoft.EntityFrameworkCore;

namespace MidTerm8897.Models
{
    public class EFDoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;
        public EFDoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Doctors>> GetAllDoctorsAsync()
        {
            return await _context.Doctors.ToListAsync();
        }
        public async Task<Doctors> GetDoctorByIdAsync(int id)
        {
            return await _context.Doctors.FindAsync(id);
        }
        public async Task AddDoctorAsync(Doctors doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateDoctorAsync(Doctors doctor)
        {
            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteDoctorAsync(int id)
        {
            var doctor = await GetDoctorByIdAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
        }
    }
}
