
using Microsoft.Data.SqlClient;

namespace CafePOS
{
    public static class Database
    {
        private static readonly string connectionString =
            @"Server=DESKTOP-TSRKH98\SQLEXPRESS;Database=CafePOSDB;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
