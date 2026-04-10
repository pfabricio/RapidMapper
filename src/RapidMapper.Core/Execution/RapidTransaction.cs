using RapidMapper.Abstractions;
using System.Data;
using System.Data.Common;

namespace RapidMapper.Execution;

public class RapidTransaction: IRapidTransaction
{
    private readonly IDbConnection _connection;

    public IDbTransaction DbTransaction { get; }
    public IDbConnectionFactory Factory { get; }

    public RapidTransaction(IDbConnection connection, IDbConnectionFactory factory)
    {
        _connection = connection;
        DbTransaction = connection.BeginTransaction();
        Factory = factory;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        DbTransaction.Commit();

        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        DbTransaction.Rollback();

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        DbTransaction.Dispose();

        if (_connection is DbConnection dbConnection)
            await dbConnection.DisposeAsync();
        else
            _connection.Dispose();
    }
}