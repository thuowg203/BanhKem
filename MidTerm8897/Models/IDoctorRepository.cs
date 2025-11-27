namespace MidTerm8897.Models
{
    public interface IDoctorRepository 
    {
        Task<IEnumerable<Doctors>> GetAllDoctorsAsync();
        Task<Doctors> GetDoctorByIdAsync(int id);
        Task AddDoctorAsync(Doctors doctor);
        Task UpdateDoctorAsync(Doctors doctor);
        Task DeleteDoctorAsync(int id);

    }
}
