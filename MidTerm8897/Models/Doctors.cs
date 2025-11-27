using System.ComponentModel.DataAnnotations;

namespace MidTerm8897.Models
{
    public class Doctors
    {
        [Key]
        public int id_doctor { get; set; }
        public string name { get; set; }
        public string specialization { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string image { get; set; }
        public int work_experience { get; set; }

        // Thuộc tính điều hướng: Một bác sĩ có nhiều cuộc hẹn
        public ICollection<Appointments> Appointments { get; set; } = new List<Appointments>();

        // Thuộc tính điều hướng: Một bác sĩ có nhiều hồ sơ y tế
        public ICollection<MedicalRecords> MedicalRecords { get; set; } = new List<MedicalRecords>();
    }
}