using System;
using MySql.Data.MySqlClient;

namespace LoginServer.DB
{
    public class AppDbContext : IDisposable
    {
        public MySqlConnection Connection { get; }

        public AppDbContext(string connectionString)
        {
            Connection = new MySqlConnection(connectionString);
        }

        public void Dispose() => Connection.Dispose();
    }
}
