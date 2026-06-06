using Hospital.BLL.DTO;
using System.Collections.Generic;

namespace Hospital.BLL.Interfaces
{
    public interface IDoctorService
    {
        IEnumerable<DoctorDTO> GetAllDoctors();
        IEnumerable<DoctorDTO> FindDoctorsBySpecialization(string specialization);
        IEnumerable<DoctorDTO> GetDoctorsSortedByExperience();
        DoctorDTO GetDoctor(int id);
        void AddDoctor(DoctorDTO doctorDTO);
        void UpdateDoctor(DoctorDTO doctorDTO);
        void Dispose();
    }
}