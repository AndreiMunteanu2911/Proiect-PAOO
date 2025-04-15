using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace ProiectPAOO
{
    public class PopulationManager
    {
        private readonly string _currentUser;

        public PopulationManager(string currentUser)
        {
            _currentUser = currentUser;
        }

        public void AddPerson(Person person, Address address)
        {
            if (_currentUser != "admin")
            {
                throw new SecurityException("Only admin can add persons.");
            }

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string insertAddressQuery = "INSERT INTO addresses (street, city, state, postal_code) VALUES (@Street, @City, @State, @PostalCode); SELECT SCOPE_IDENTITY();";
                int addressId;

                using (SqlCommand addressCommand = new SqlCommand(insertAddressQuery, connection))
                {
                    addressCommand.Parameters.AddWithValue("@Street", address.Street);
                    addressCommand.Parameters.AddWithValue("@City", address.City);
                    addressCommand.Parameters.AddWithValue("@State", address.State);
                    addressCommand.Parameters.AddWithValue("@PostalCode", address.PostalCode);
                    addressId = Convert.ToInt32(addressCommand.ExecuteScalar());
                }
                string insertPersonQuery = "INSERT INTO persons (name, age, address_id, birth_place, cnp, ethnicity) VALUES (@Name, @Age, @AddressId, @BirthPlace, @CNP, @Ethnicity)";

                using (SqlCommand personCommand = new SqlCommand(insertPersonQuery, connection))
                {
                    personCommand.Parameters.AddWithValue("@Name", person.Name);
                    personCommand.Parameters.AddWithValue("@Age", person.Age);
                    personCommand.Parameters.AddWithValue("@AddressId", addressId);
                    personCommand.Parameters.AddWithValue("@BirthPlace", person.BirthPlace);
                    personCommand.Parameters.AddWithValue("@CNP", person.CNP);
                    personCommand.Parameters.AddWithValue("@Ethnicity", person.Ethnicity);
                    personCommand.ExecuteNonQuery();
                    string sqlLog = $"INSERT INTO persons (name, age, address_id, birth_place, cnp, ethnicity) VALUES ('{person.Name}', {person.Age}, {addressId}, '{person.BirthPlace}', '{person.CNP}', '{person.Ethnicity}')";
                    LogUsage(_currentUser, "Add person", sqlLog);
                }
            }
        }

        public Person GetPerson(int id)
        {
            string query = "SELECT * FROM persons WHERE id = @Id";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int addressId = (int)reader["address_id"];

                            return new Person(
                                (int)reader["id"],
                                reader["name"].ToString(),
                                (int)reader["age"],
                                addressId,
                                reader["birth_place"].ToString(),
                                reader["cnp"].ToString(),
                                reader["ethnicity"].ToString()
                            );
                        }
                    }
                }
            }

            return null;
        }

        public void UpdatePerson(Person person, Address address)
        {
            if (_currentUser.Trim() != "admin")
            {
                throw new SecurityException("Only admin can update persons.");
            }

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string updateAddressQuery = "UPDATE addresses SET street = @Street, city = @City, state = @State, postal_code = @PostalCode WHERE id = @Id";

                using (SqlCommand addressCommand = new SqlCommand(updateAddressQuery, connection))
                {
                    addressCommand.Parameters.AddWithValue("@Street", address.Street);
                    addressCommand.Parameters.AddWithValue("@City", address.City);
                    addressCommand.Parameters.AddWithValue("@State", address.State);
                    addressCommand.Parameters.AddWithValue("@PostalCode", address.PostalCode);
                    addressCommand.Parameters.AddWithValue("@Id", person.AddressId);
                    addressCommand.ExecuteNonQuery();
                }
                string updatePersonQuery = "UPDATE persons SET name = @Name, age = @Age, address_id = @AddressId, birth_place = @BirthPlace, cnp = @CNP, ethnicity = @Ethnicity WHERE id = @Id";

                using (SqlCommand personCommand = new SqlCommand(updatePersonQuery, connection))
                {
                    personCommand.Parameters.AddWithValue("@Name", person.Name);
                    personCommand.Parameters.AddWithValue("@Age", person.Age);
                    personCommand.Parameters.AddWithValue("@AddressId", person.AddressId);
                    personCommand.Parameters.AddWithValue("@BirthPlace", person.BirthPlace);
                    personCommand.Parameters.AddWithValue("@CNP", person.CNP);
                    personCommand.Parameters.AddWithValue("@Ethnicity", person.Ethnicity);
                    personCommand.Parameters.AddWithValue("@Id", person.Id);
                    personCommand.ExecuteNonQuery();
                    string sqlLog = $"UPDATE persons SET name = '{person.Name}', age = {person.Age}, address_id = {person.AddressId}, birth_place = '{person.BirthPlace}', cnp = '{person.CNP}', ethnicity = '{person.Ethnicity}' WHERE id = {person.Id}";
                    LogUsage(_currentUser, "Update person", sqlLog);
                }
            }
        }

        public void ClearDatabase()
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand("DELETE FROM Persons", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        using (SqlCommand command = new SqlCommand("DELETE FROM Addresses", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }
                        using (SqlCommand command = new SqlCommand("DBCC CHECKIDENT ('Persons', RESEED, 0)", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        using (SqlCommand command = new SqlCommand("DBCC CHECKIDENT ('Addresses', RESEED, 0)", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }





        public void DeletePerson(int id)
        {
            if (_currentUser.Trim() != "admin")
            {
                throw new SecurityException("Only admin can delete persons.");
            }

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                int addressId = 0;
                string getAddressQuery = "SELECT address_id FROM persons WHERE id = @Id";

                using (SqlCommand getAddressCommand = new SqlCommand(getAddressQuery, connection))
                {
                    getAddressCommand.Parameters.AddWithValue("@Id", id);
                    var result = getAddressCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        addressId = Convert.ToInt32(result);
                    }
                }
                string deletePersonQuery = "DELETE FROM persons WHERE id = @Id";

                using (SqlCommand personCommand = new SqlCommand(deletePersonQuery, connection))
                {
                    personCommand.Parameters.AddWithValue("@Id", id);
                    personCommand.ExecuteNonQuery();
                }
                if (addressId > 0)
                {
                    string deleteAddressQuery = "DELETE FROM addresses WHERE id = @Id";

                    using (SqlCommand addressCommand = new SqlCommand(deleteAddressQuery, connection))
                    {
                        addressCommand.Parameters.AddWithValue("@Id", addressId);
                        addressCommand.ExecuteNonQuery();
                    }
                }
                string sqlLog = $"DELETE FROM persons WHERE id = {id}";
                LogUsage(_currentUser, "Delete person", sqlLog);
            }
        }

        public List<Person> FilterAndSortPersons(string ethnicity, int? minAge, int? maxAge, string city, string sortOrder)
        {
            List<Person> persons = new List<Person>();
            List<string> filters = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(ethnicity))
            {
                filters.Add("ethnicity = @Ethnicity");
                parameters.Add(new SqlParameter("@Ethnicity", ethnicity));
            }

            if (minAge.HasValue)
            {
                filters.Add("age >= @MinAge");
                parameters.Add(new SqlParameter("@MinAge", minAge.Value));
            }

            if (maxAge.HasValue)
            {
                filters.Add("age <= @MaxAge");
                parameters.Add(new SqlParameter("@MaxAge", maxAge.Value));
            }

            if (!string.IsNullOrEmpty(city))
            {
                filters.Add("address_id IN (SELECT id FROM addresses WHERE city = @City)");
                parameters.Add(new SqlParameter("@City", city));
            }

            string filterCriteria = filters.Count > 0 ? string.Join(" AND ", filters) : "1=1";
            string query = $"SELECT * FROM persons WHERE {filterCriteria} ORDER BY {sortOrder}";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int addressId = (int)reader["address_id"];

                            persons.Add(new Person(
                                (int)reader["id"],
                                reader["name"].ToString(),
                                (int)reader["age"],
                                addressId,
                                reader["birth_place"].ToString(),
                                reader["cnp"].ToString(),
                                reader["ethnicity"].ToString()
                            ));
                        }
                    }
                }
            }

            return persons;
        }



        public static void LogUsage(string user, string operation, string sqlCommand)
        {
            string query = "INSERT INTO usage_history (date_time, username, operation, sql_command) VALUES (@DateTime, @Username, @Operation, @SqlCommand)";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DateTime", DateTime.Now);
                    command.Parameters.AddWithValue("@Username", user);
                    command.Parameters.AddWithValue("@Operation", operation);
                    command.Parameters.AddWithValue("@SqlCommand", sqlCommand);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<UsageHistory> GetUsageHistory()
        {
            List<UsageHistory> history = new List<UsageHistory>();
            string query = "SELECT * FROM usage_history ORDER BY date_time DESC";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add(new UsageHistory(
                                (int)reader["id"],
                                (DateTime)reader["date_time"],
                                reader["username"].ToString(),
                                reader["operation"].ToString(),
                                reader["sql_command"].ToString()
                            ));
                        }
                    }
                }
            }

            return history;
        }

        public Address GetAddress(int addressId)
        {
            string query = "SELECT * FROM addresses WHERE id = @Id";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", addressId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Address(
                                (int)reader["id"],
                                reader["street"].ToString(),
                                reader["city"].ToString(),
                                reader["state"].ToString(),
                                reader["postal_code"].ToString()
                            );
                        }
                    }
                }
            }

            return null;
        }
    }

    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
    }
}
