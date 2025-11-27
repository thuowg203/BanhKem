using System.ComponentModel.DataAnnotations;

namespace MidTerm8897.Models
{
    public class Patients
    {
        [Key]
        public int id_patient { get; set; }
        //public DateTime dob { get; set; } // Ngày sinh
        public string name { get; set; }
        public string phone { get; set; }
        //public string email { get; set; }
        public string address { get; set; }

        // Thuộc tính điều hướng: Một bệnh nhân có nhiều cuộc hẹn
        public ICollection<Appointments> Appointments { get; set; } = new List<Appointments>();

        // Thuộc tính điều hướng: Một bệnh nhân có nhiều hồ sơ y tế
        public ICollection<MedicalRecords> MedicalRecords { get; set; } = new List<MedicalRecords>();
    }
}