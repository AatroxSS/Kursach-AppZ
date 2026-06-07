using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.DAL.Entities;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hospital.BLL.Infrastructure
{
    public class HospitalMapperProfile : Profile
    {
        public HospitalMapperProfile()
        {
            CreateMap<Doctor, DoctorDTO>()
    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName));
            CreateMap<Patient, PatientDTO>().ReverseMap();

            CreateMap<Appointment, AppointmentDTO>()
                .ForMember(dest => dest.DoctorFullName, opt => opt.MapFrom(src => $"{src.Doctor.FirstName} {src.Doctor.LastName}"))
                .ForMember(dest => dest.PatientFullName, opt => opt.MapFrom(src => $"{src.Patient.FirstName} {src.Patient.LastName}"));

            CreateMap<AppointmentDTO, Appointment>();
        }
    }
}