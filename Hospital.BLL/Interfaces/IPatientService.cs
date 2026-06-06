using Hospital.BLL.DTO;
using System.Collections.Generic;

namespace Hospital.BLL.Interfaces
{
    public interface IPatientService
    {
        IEnumerable<PatientDTO> GetAllPatients();
        void AddPatient(PatientDTO patientDto);
        void UpdatePatient(PatientDTO patientDto);
        void Dispose();
    }
}