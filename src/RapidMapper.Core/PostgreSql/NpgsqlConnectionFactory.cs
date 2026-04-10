using System.Data.Common;
using Npgsql;
using RapidMapper.Abstractions;

namespace RapidMapper.PostgreSql;

public class NpgsqlConnectionFactory
    : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(
        string connectionString)
    {
        _connectionString =
            connectionString;
    }

    public DbConnection Create()
        => new NpgsqlConnection(_connectionString);
}