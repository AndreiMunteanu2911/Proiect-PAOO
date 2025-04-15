using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace ProiectPAOO
{
    public class ImportUtility
    {
        public void ImportFromCSV(string filePath, bool overwrite)
        {
            if (overwrite)
            {
                ClearDatabase();
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(',');
                    if (values.Length < 10)
                    {
                        Console.Error.WriteLine($"Error: Not enough columns in line: {line}");
                        continue;
                    }

                    try
                    {
                        int id = int.Parse(values[0]);
                        string name = values[1];
                        int age = int.Parse(values[2]);
                        string street = values[3];
                        string city = values[4];
                        string state = values[5];
                        string postalCode = values[6];
                        string birthPlace = values[7];
                        string cnp = values[8];
                        string ethnicity = values[9];
                        Address address = new Address(0, street, city, state, postalCode);
                        Person person = new Person(id, name, age, 0, birthPlace, cnp, ethnicity);
                        if (!overwrite)
                        {
                            if (!PersonExists(cnp))
                            {
                                AddPersonToDatabase(person, address);
                            }
                        }
                        else
                        {
                            AddPersonToDatabase(person, address);
                        }
                    }
                    catch (FormatException e)
                    {
                        Console.Error.WriteLine($"Error parsing line: {line}");
                        Console.Error.WriteLine($"Error: {e.Message}");
                    }
                }
            }
        }

        private void AddPersonToDatabase(Person person, Address address)
        {
            PopulationManager manager = new PopulationManager("admin");
            manager.AddPerson(person, address);
        }

        private void ClearDatabase()
        {
            string sql = "DELETE FROM persons";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private bool PersonExists(string cnp)
        {
            string sql = "SELECT COUNT(*) FROM persons WHERE cnp = @CNP";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CNP", cnp);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}
