using System;

namespace Hospital.BLL.DTO
{
    public class PatientDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }

       
        public int UserId { get; set; }
    }
}