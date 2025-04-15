using System;
using Microsoft.Data.SqlClient;
using System.IO;

namespace ProiectPAOO
{
    public class DatabaseConnection
    {
        private static readonly string DatabaseName = "population_db";
        private static readonly string ConnectionString = $"Server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={GetDatabasePath()};Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            EnsureDatabaseExists();
            return new SqlConnection(ConnectionString);
        }

        private static string GetDatabasePath()
        {
            string projectRootPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string dbPath = Path.Combine(projectRootPath, $"{DatabaseName}.mdf");

            return dbPath;
        }

        private static void EnsureDatabaseExists()
        {
            string dbPath = GetDatabasePath();
            if (!File.Exists(dbPath))
            {
                CreateDatabase();
                CreateTables();
            }
        }

        private static void CreateDatabase()
        {
            string connectionString = $"Server=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string dbPath = GetDatabasePath();
            string directoryPath = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"CREATE DATABASE population_db ON PRIMARY (NAME = population_db, FILENAME = '{dbPath}')";
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void CreateTables()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    CREATE TABLE addresses (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        street NVARCHAR(255) NOT NULL,
                        city NVARCHAR(255) NOT NULL,
                        state NVARCHAR(255) NOT NULL,
                        postal_code NVARCHAR(20) NOT NULL
                    )";
                    command.ExecuteNonQuery();
                    command.CommandText = @"
                    CREATE TABLE persons (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        name NVARCHAR(255) NOT NULL,
                        age INT NOT NULL,
                        address_id INT,
                        birth_place NVARCHAR(255) NOT NULL,
                        cnp NCHAR(13) NOT NULL,
                        Ethnicity NVARCHAR(50) NOT NULL,
                        FOREIGN KEY (address_id) REFERENCES addresses(id) ON DELETE CASCADE
                    )";
                    command.ExecuteNonQuery();
                    command.CommandText = @"
                    CREATE TABLE usage_history (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        date_time DATETIME NOT NULL,
                        username NVARCHAR(255) NOT NULL,
                        operation NVARCHAR(255) NOT NULL,
                        sql_command NVARCHAR(MAX) NOT NULL
                    )";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
