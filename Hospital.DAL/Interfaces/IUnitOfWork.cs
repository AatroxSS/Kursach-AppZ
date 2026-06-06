using Hospital.DAL.Entities;
using System;

namespace Hospital.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Doctor> Doctors { get; }
        IRepository<Patient> Patients { get; }
        IRepository<Appointment> Appointments { get; }

        void Save();
    }
}