
namespace ProiectPAOO
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int AddressId { get; set; }
        public string BirthPlace { get; set; }
        public string CNP { get; set; }
        public string Ethnicity { get; set; }

        public Person(int id, string name, int age, int addressId, string birthPlace, string cnp, string ethnicity)
        {
            Id = id;
            Name = name;
            Age = age;
            AddressId = addressId;
            BirthPlace = birthPlace;
            CNP = cnp;
            Ethnicity = ethnicity;
        }
    }
}