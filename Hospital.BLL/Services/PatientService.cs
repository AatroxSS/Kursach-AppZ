using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using System.Collections.Generic;

namespace Hospital.BLL.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PatientService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public IEnumerable<PatientDTO> GetAllPatients()
        {
            return _mapper.Map<IEnumerable<Patient>, List<PatientDTO>>(_uow.Patients.GetAll());
        }

        public void AddPatient(PatientDTO patientDto)
        {
            if (string.IsNullOrWhiteSpace(patientDto.FirstName))
                throw new ValidationException("Ім'я обов'язкове", "FirstName");

            _uow.Patients.Create(_mapper.Map<PatientDTO, Patient>(patientDto));
            _uow.Save();
        }

        public void UpdatePatient(PatientDTO patientDto)
        {
            _uow.Patients.Update(_mapper.Map<PatientDTO, Patient>(patientDto));
            _uow.Save();
        }

        public void Dispose() => _uow.Dispose();
    }
}