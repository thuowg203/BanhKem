using System.ComponentModel.DataAnnotations;

namespace MidTerm8897.Models
{
    public class MedicalRecords
    {
        [Key]
        public int id_medical_record { get; set; }
        public int id_appointment { get; set; }
        public int id_patient { get; set; }
        public int id_doctor { get; set; }
        public string diagnosis { get; set; }
        public string treatment { get; set; }
        public DateTime date { get; set; }

        // Thuộc tính điều hướng: Một hồ sơ y tế thuộc về một cuộc hẹn
        public Appointments Appointment { get; set; }

        // Thuộc tính điều hướng: Một hồ sơ y tế thuộc về một bệnh nhân
        public Patients Patient { get; set; }

        // Thuộc tính điều hướng: Một hồ sơ y tế thuộc về một bác sĩ
        public Doctors Doctor { get; set; }
    }
}