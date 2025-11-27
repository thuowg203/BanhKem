using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidTerm8897.Migrations
{
    /// <inheritdoc />
    public partial class FixPatientsPhoneDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dob",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "gender",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "appointment_time",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "record_date",
                table: "MedicalRecords",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "prestription",
                table: "MedicalRecords",
                newName: "treatment");

            migrationBuilder.RenameColumn(
                name: "id_record",
                table: "MedicalRecords",
                newName: "id_medical_record");

            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_id_appointment",
                table: "MedicalRecords",
                column: "id_appointment",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_id_doctor",
                table: "MedicalRecords",
                column: "id_doctor");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_id_patient",
                table: "MedicalRecords",
                column: "id_patient");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_id_doctor",
                table: "Appointments",
                column: "id_doctor");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_id_patient",
                table: "Appointments",
                column: "id_patient");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_id_doctor",
                table: "Appointments",
                column: "id_doctor",
                principalTable: "Doctors",
                principalColumn: "id_doctor",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_id_patient",
                table: "Appointments",
                column: "id_patient",
                principalTable: "Patients",
                principalColumn: "id_patient",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Appointments_id_appointment",
                table: "MedicalRecords",
                column: "id_appointment",
                principalTable: "Appointments",
                principalColumn: "id_appointment",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Doctors_id_doctor",
                table: "MedicalRecords",
                column: "id_doctor",
                principalTable: "Doctors",
                principalColumn: "id_doctor",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Patients_id_patient",
                table: "MedicalRecords",
                column: "id_patient",
                principalTable: "Patients",
                principalColumn: "id_patient",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_id_doctor",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_id_patient",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Appointments_id_appointment",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Doctors_id_doctor",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Patients_id_patient",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_id_appointment",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_id_doctor",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_id_patient",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_id_doctor",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_id_patient",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "treatment",
                table: "MedicalRecords",
                newName: "prestription");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "MedicalRecords",
                newName: "record_date");

            migrationBuilder.RenameColumn(
                name: "id_medical_record",
                table: "MedicalRecords",
                newName: "id_record");

            migrationBuilder.AlterColumn<int>(
                name: "phone",
                table: "Patients",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateOnly>(
                name: "dob",
                table: "Patients",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "gender",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "appointment_time",
                table: "Appointments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
