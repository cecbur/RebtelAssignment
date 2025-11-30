using System.Data;
using Microsoft.Data.SqlClient;

namespace DataStorage;

public class SqlServerConnectionFactory(string connectionString) : IDbConnectionFactory
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString), "Connection string missing");

    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
