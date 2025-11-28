using System.Data;

namespace DataStorage;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
