using RapidMapper.Abstractions;
using RapidMapper.Configuration;

namespace RapidMapper.Execution;

public class RapidMapper : IRapidMapper
{
    private readonly ConnectionRegistry _registry;

    public RapidMapper(ConnectionRegistry registry)
    {
        _registry = registry;
    }

    public IRapidMapper For(DatabaseType type)
    {
        var factory = _registry.GetFactory(type);
        return new RapidMapperExecutor(factory);
    }

    public IAsyncEnumerable<T> QueryStreamAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var factory =
            _registry.GetDefault();

        var executor =
            new RapidMapperExecutor(factory);

        return executor.QueryStreamAsync<T>(
            sql,
            parameters,
            cancellationToken);
    }

    public async Task<IRapidGridReader> QueryMultipleAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var factory = _registry.GetDefault();

        var executor = new RapidMapperExecutor(factory);

        return await executor.QueryMultipleAsync(sql, parameters, cancellationToken);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var factory =
            _registry.GetDefault();

        var executor =
            new RapidMapperExecutor(factory);

        return await executor.QueryAsync<T>(
            sql,
            parameters,
            cancellationToken);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var executor = new RapidMapperExecutor(_registry.GetDefault());
        return await executor.QuerySingleOrDefaultAsync<T>(sql, parameters, cancellationToken);
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var executor = new RapidMapperExecutor(_registry.GetDefault());
        return await executor.QuerySingleAsync<T>(sql, parameters, cancellationToken);
    }

    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var factory =
            _registry.GetDefault();

        var executor =
            new RapidMapperExecutor(factory);

        return await executor.ExecuteAsync(
            sql,
            parameters,
            cancellationToken);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var factory =
            _registry.GetDefault();

        var executor =
            new RapidMapperExecutor(factory);

        return await executor
            .QueryFirstOrDefaultAsync<T>(
                sql,
                parameters,
                cancellationToken);
    }

    public async Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var factory =
            _registry.GetDefault();

        var executor =
            new RapidMapperExecutor(factory);

        return await executor.ExecuteScalarAsync<T>(
            sql,
            parameters,
            cancellationToken);
    }

    public IRapidMapper WithTransaction(IRapidTransaction transaction)
    {
        var factory =
            _registry.GetDefault();
        return new RapidMapperExecutor(factory, transaction.DbTransaction);
    }

    public async Task<IRapidTransaction> BeginTransactionAsync(
            DatabaseType? databaseType = null,
            CancellationToken cancellationToken = default)
    {
        var factory =
            databaseType.HasValue
                ? _registry.GetFactory(
                    databaseType.Value)
                : _registry.GetDefault();

        var executor =
            new RapidMapperExecutor(factory);

        return await executor
            .BeginTransactionAsync(
                databaseType,
                cancellationToken);
    }

    public async Task<IEnumerable<TResult>> QueryAsync<T1, T2, TResult>(
        string sql,
        Func<T1, T2, TResult> map,
        object? parameters = null,
        string splitOn = "Id",
        CancellationToken cancellationToken = default)
    {
        var factory =
            _registry.GetDefault();

        var executor =
            new RapidMapperExecutor(factory);

        return await executor.QueryAsync<T1, T2, TResult>(
            sql,
            map,
            parameters,
            splitOn,
            cancellationToken);
    }

    public async Task<IEnumerable<TResult>> QueryAsync<T1, T2, T3, TResult>(
        string sql,
        Func<T1, T2?, T3?, TResult> map,
        object? parameters = null,
        string splitOn = "Id,Id",
        CancellationToken cancellationToken = default)
    {
        var factory =
            _registry.GetDefault();

        var executor =
            new RapidMapperExecutor(factory);

        return await executor.QueryAsync<T1, T2, T3, TResult>(
            sql,
            map,
            parameters,
            splitOn,
            cancellationToken);
    }
}