using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using System.Collections.Generic;
using System.Numerics;

namespace Hospital.BLL.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DoctorService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public IEnumerable<DoctorDTO> GetAllDoctors()
        {
            var doctors = _uow.Doctors.GetAll();
            return _mapper.Map<IEnumerable<Doctor>, List<DoctorDTO>>(doctors);
        }

        public IEnumerable<DoctorDTO> FindDoctorsBySpecialization(string specialization)
        {
            var doctors = _uow.Doctors.Find(d => d.Specialization.Contains(specialization));
            return _mapper.Map<IEnumerable<Doctor>, List<DoctorDTO>>(doctors);
        }

        public IEnumerable<DoctorDTO> GetDoctorsSortedByExperience()
        {
            var doctors = _uow.Doctors.GetAll().OrderByDescending(d => d.ExperienceYears);
            return _mapper.Map<IEnumerable<Doctor>, List<DoctorDTO>>(doctors);
        }
        public DoctorDTO GetDoctor(int id)
        {
            var doctor = _uow.Doctors.GetById(id);
            if (doctor == null)
                throw new ValidationException("Лікаря не знайдено", "");

            return _mapper.Map<Doctor, DoctorDTO>(doctor);
        }

        public void AddDoctor(DoctorDTO doctorDTO)
        {
            if (string.IsNullOrWhiteSpace(doctorDTO.FirstName) || string.IsNullOrWhiteSpace(doctorDTO.LastName))
                throw new ValidationException("Ім'я та прізвище лікаря є обов'язковими", "Name");

            var doctor = _mapper.Map<DoctorDTO, Doctor>(doctorDTO);
            _uow.Doctors.Create(doctor);
            _uow.Save();
        }

        public void UpdateDoctor(DoctorDTO doctorDTO)
        {
            var doctor = _mapper.Map<DoctorDTO, Doctor>(doctorDTO);
            _uow.Doctors.Update(doctor);
            _uow.Save();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}