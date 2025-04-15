using System;

namespace ProiectPAOO
{
    public class UsageHistory
    {
        public int Id { get; }
        public DateTime DateTime { get; }
        public string User { get; }
        public string Operation { get; }
        public string SqlCommand { get; }

        public UsageHistory(int id, DateTime dateTime, string user, string operation, string sqlCommand)
        {
            Id = id;
            DateTime = dateTime;
            User = user;
            Operation = operation;
            SqlCommand = sqlCommand;
        }
    }
}