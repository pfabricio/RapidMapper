using RapidMapper.Configuration;

namespace RapidMapper.Abstractions;

public interface IRapidMapper
{
    IAsyncEnumerable<T> QueryStreamAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<IRapidGridReader> QueryMultipleAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<IRapidTransaction> BeginTransactionAsync(DatabaseType? databaseType = null, CancellationToken cancellationToken = default);
    IRapidMapper WithTransaction(IRapidTransaction transaction);
    Task<IEnumerable<TResult>> QueryAsync<T1, T2, TResult>(string sql, Func<T1, T2, TResult> map, object? parameters = null, string splitOn = "Id", CancellationToken cancellationToken = default);
    Task<IEnumerable<TResult>> QueryAsync<T1, T2, T3, TResult>(
        string sql,
        Func<T1, T2?, T3?, TResult> map,
        object? parameters = null,
        string splitOn = "Id,Id",
        CancellationToken cancellationToken = default);
}