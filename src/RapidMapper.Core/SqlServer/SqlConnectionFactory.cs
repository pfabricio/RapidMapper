using Microsoft.Data.SqlClient;
using System.Data.Common;
using RapidMapper.Abstractions;

namespace RapidMapper.SqlServer;

public class SqlConnectionFactory
    : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection Create()
    {
        return new SqlConnection(_connectionString);
    }
}