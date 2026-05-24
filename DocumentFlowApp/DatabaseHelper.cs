using System.Data.SqlClient;

namespace DocumentFlowApp
{
    public class DatabaseHelper
    {
        private static string connectionString = @"Server=MSI\SQLEXPRESS;Database=DocumentFlow;Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}