using System;

namespace MultiStepFormWPF
{
    public sealed class FormData
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;

        public string HouseNumber { get; set; } = string.Empty;

        public string ApartmentNumber { get; set; } = string.Empty;
    }
}
