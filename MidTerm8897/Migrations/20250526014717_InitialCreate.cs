using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidTerm8897.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    id_appointment = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_patient = table.Column<int>(type: "int", nullable: false),
                    id_doctor = table.Column<int>(type: "int", nullable: false),
                    appointment_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    appointment_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.id_appointment);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    id_doctor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    specialization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    work_experience = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.id_doctor);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    id_record = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_patient = table.Column<int>(type: "int", nullable: false),
                    id_doctor = table.Column<int>(type: "int", nullable: false),
                    id_appointment = table.Column<int>(type: "int", nullable: false),
                    diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    prestription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    record_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.id_record);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    id_patient = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dob = table.Column<DateOnly>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone = table.Column<int>(type: "int", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.id_patient);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
