using System.Data;
using Npgsql;

namespace Wmi.Api.Data;

public class SqlConnectionFactory(IConfiguration configuration): IDbConnectionFactory
{


    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connectionString = configuration.GetValue<string>("ConnectionString");
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}