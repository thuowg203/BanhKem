namespace MidTerm8897.Models
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patients>> GetAllPatientsAsync();
        Task<Patients> GetPatientByIdAsync(int id);
        Task AddPatientAsync(Patients patient);
        Task UpdatePatientAsync(Patients patient);
        Task DeletePatientAsync(int id);

    }
}
