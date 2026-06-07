using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.DAL.Entities;

namespace Hospital.BLL.Infrastructure
{
    public class HospitalMapperProfile : Profile
    {
        public HospitalMapperProfile()
        {
            CreateMap<Doctor, DoctorDTO>().ReverseMap();
            CreateMap<Patient, PatientDTO>().ReverseMap();
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<Appointment, AppointmentDTO>()
                .ForMember(dest => dest.DoctorFullName,
                    opt => opt.MapFrom(src => src.Doctor == null ? null : $"{src.Doctor.FirstName} {src.Doctor.LastName}"))
                .ForMember(dest => dest.PatientFullName,
                    opt => opt.MapFrom(src => src.Patient == null ? null : $"{src.Patient.FirstName} {src.Patient.LastName}"));

            CreateMap<AppointmentDTO, Appointment>()
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());
        }
    }
}
