using System.ComponentModel.DataAnnotations;

namespace MidTerm8897.Models
{
    public class Appointments
    {
        [Key]
        public int id_appointment { get; set; }
        public int id_patient { get; set; }
        public int id_doctor { get; set; }
        public DateTime appointment_date { get; set; }
        public string status { get; set; }

        // Thuộc tính điều hướng: Một cuộc hẹn thuộc về một bệnh nhân
        public Patients Patient { get; set; }

        // Thuộc tính điều hướng: Một cuộc hẹn thuộc về một bác sĩ
        public Doctors Doctor { get; set; }

        // Thuộc tính điều hướng: Một cuộc hẹn có một hồ sơ y tế
        public MedicalRecords MedicalRecord { get; set; }
    }
}