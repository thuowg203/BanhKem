using Microsoft.EntityFrameworkCore;

namespace MidTerm8897.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Doctors> Doctors { get; set; }
        public DbSet<Patients> Patients { get; set; }
        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<MedicalRecords> MedicalRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quan hệ một-một giữa Appointment và MedicalRecord
            modelBuilder.Entity<Appointments>()
                .HasOne(a => a.MedicalRecord) // Chỉ định thuộc tính điều hướng
                .WithOne(m => m.Appointment)  // Chỉ định thuộc tính điều hướng ngược lại
                .HasForeignKey<MedicalRecords>(m => m.id_appointment); // Khóa ngoại

            // Quan hệ một-nhiều giữa Patient và Appointment
            modelBuilder.Entity<Appointments>()
                .HasOne(a => a.Patient) // Chỉ định thuộc tính điều hướng
                .WithMany(p => p.Appointments) // Chỉ định danh sách trong Patient
                .HasForeignKey(a => a.id_patient)
                .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa cascade

            // Quan hệ một-nhiều giữa Doctor và Appointment
            modelBuilder.Entity<Appointments>()
                .HasOne(a => a.Doctor) // Chỉ định thuộc tính điều hướng
                .WithMany(d => d.Appointments) // Chỉ định danh sách trong Doctor
                .HasForeignKey(a => a.id_doctor)
                .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa cascade

            // Quan hệ một-nhiều giữa Patient và MedicalRecord
            modelBuilder.Entity<MedicalRecords>()
                .HasOne(m => m.Patient) // Chỉ định thuộc tính điều hướng
                .WithMany(p => p.MedicalRecords) // Chỉ định danh sách trong Patient
                .HasForeignKey(m => m.id_patient)
                .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa cascade

            // Quan hệ một-nhiều giữa Doctor và MedicalRecord
            modelBuilder.Entity<MedicalRecords>()
                .HasOne(m => m.Doctor) // Chỉ định thuộc tính điều hướng
                .WithMany(d => d.MedicalRecords) // Chỉ định danh sách trong Doctor
                .HasForeignKey(m => m.id_doctor)
                .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa cascade
        }
    }
}