using Hospital.DAL.EF;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using System;

namespace Hospital.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HospitalDbContext _db;

        private IRepository<User> _userRepository;
        private IRepository<Doctor> _doctorRepository;
        private IRepository<Patient> _patientRepository;
        private IRepository<Appointment> _appointmentRepository;

        public UnitOfWork(HospitalDbContext context)
        {
            _db = context;
        }

        public IRepository<User> Users => _userRepository ??= new Repository<User>(_db);
        public IRepository<Doctor> Doctors => _doctorRepository ??= new Repository<Doctor>(_db);
        public IRepository<Patient> Patients => _patientRepository ??= new Repository<Patient>(_db);
        public IRepository<Appointment> Appointments => _appointmentRepository ??= new Repository<Appointment>(_db);

        public void Save()
        {
            _db.SaveChanges();
        }

        private bool _disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}