
namespace ProiectPAOO
{
    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        public Address(int id, string street, string city, string state, string postalCode)
        {
            Id = id;
            Street = street;
            City = city;
            State = state;
            PostalCode = postalCode;
        }
    }
}