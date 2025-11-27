using System.Data;

namespace DataStorage;

/// <summary>
/// Factory interface for creating database connections
/// Follows Factory pattern and enables dependency injection
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new database connection
    /// </summary>
    /// <returns>An open database connection</returns>
    IDbConnection CreateConnection();
}
