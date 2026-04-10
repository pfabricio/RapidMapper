using MySqlConnector;
using RapidMapper.Abstractions;
using System.Data.Common;

namespace RapidMapper.MySql;

public class MySqlConnectionFactory
    : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(
        string connectionString)
    {
        _connectionString =
            connectionString;
    }

    public DbConnection Create()
        => new MySqlConnection(
            _connectionString);
}