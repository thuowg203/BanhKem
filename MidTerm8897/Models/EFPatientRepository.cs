using Microsoft.EntityFrameworkCore;
namespace MidTerm8897.Models
{
    public class EFPatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;
        public EFPatientRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Patients>> GetAllPatientsAsync()
        {
            return await _context.Patients.ToListAsync();
        }
        public async Task<Patients> GetPatientByIdAsync(int id)
        {
            return await _context.Patients.FindAsync(id);
        }
        public async Task AddPatientAsync(Patients patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatePatientAsync(Patients patient)
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }
        public async Task DeletePatientAsync(int id)
        {
            var patient = await GetPatientByIdAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
        }
    }
    
    
}
