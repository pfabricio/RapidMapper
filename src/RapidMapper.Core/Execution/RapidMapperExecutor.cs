using RapidMapper.Abstractions;
using RapidMapper.Configuration;
using RapidMapper.Mapping;
using RapidMapper.Parameters;
using System.Data;
using System.Data.Common;

namespace RapidMapper.Execution;

public class RapidMapperExecutor : IRapidMapper
{
    private readonly IDbConnectionFactory _factory;
    private readonly IDbTransaction? _transaction;

    public RapidMapperExecutor(
        IDbConnectionFactory factory,
        IDbTransaction? transaction = null)
    {
        _factory = factory;
        _transaction = transaction;
    }

    private async Task<DbConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_transaction?.Connection is DbConnection existing)
            return existing;

        var conn =
            (DbConnection)_factory.Create();

        await conn.OpenAsync(ct);

        return conn;
    }

    private async Task<DbCommand> CreateCommandAsync(string sql, object? parameters, CancellationToken ct)
    {
        var conn =
            await GetConnectionAsync(ct);

        var cmd =
            conn.CreateCommand();

        cmd.CommandText = sql;

        if (_transaction != null)
            cmd.Transaction =
                (DbTransaction)_transaction;

        ParameterBinder.Bind(
            cmd,
            parameters);

        return cmd;
    }

    public async IAsyncEnumerable<T> QueryStreamAsync<T>(
        string sql,
        object? parameters = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var conn =
            _transaction?.Connection as DbConnection
            ?? (DbConnection)_factory.Create();

        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        await using var cmd =
            conn.CreateCommand();

        cmd.CommandText = sql;

        if (_transaction != null)
            cmd.Transaction =
                (DbTransaction)_transaction;

        ParameterBinder.Bind(
            cmd,
            parameters);

        await using var reader =
            await cmd.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess,
                cancellationToken);

        var mapper =
            RapidMapperCache
                .GetMapper<T>(reader);

        while (await reader.ReadAsync(
                   cancellationToken))
        {
            yield return mapper(reader);
        }
    }

    public async Task<IRapidGridReader> QueryMultipleAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var conn =
            _transaction?.Connection as DbConnection
            ?? (DbConnection)_factory.Create();

        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(
                cancellationToken);

        var cmd =
            conn.CreateCommand();

        cmd.CommandText = sql;

        if (_transaction != null)
            cmd.Transaction =
                (DbTransaction)_transaction;

        ParameterBinder.Bind(
            cmd,
            parameters);

        var reader =
            await cmd.ExecuteReaderAsync(
                cancellationToken);

        return new RapidGridReader(reader);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var cmd =
            await CreateCommandAsync(sql, parameters, cancellationToken);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var list = new List<T>();

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(RapidAutoMapper.Map<T>(reader));
        }

        return list;
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        return (await QueryAsync<T>(sql, parameters, cancellationToken)).FirstOrDefault();
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var list = (await QueryAsync<T>(sql, parameters, cancellationToken)).ToList();

        if (list.Count > 1)
            throw new InvalidOperationException("Sequence contains more than one element");

        return list.SingleOrDefault();
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var list = (await QueryAsync<T>(sql, parameters, cancellationToken)).ToList();

        if (list.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements");

        if (list.Count > 1)
            throw new InvalidOperationException("Sequence contains more than one element");

        return list[0];
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var cmd = await CreateCommandAsync(sql, parameters, cancellationToken);
        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var cmd = await CreateCommandAsync(sql, parameters, cancellationToken);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);

        if (result == null || result == DBNull.Value)
            return default!;

        return (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<IRapidTransaction> BeginTransactionAsync(DatabaseType? databaseType = null, CancellationToken cancellationToken = default)
    {
        var conn = (DbConnection)_factory.Create();
        await conn.OpenAsync(cancellationToken);
        return new RapidTransaction(conn, _factory);
    }

    public IRapidMapper WithTransaction(IRapidTransaction transaction)
    {
        return new RapidMapperExecutor(_factory, transaction.DbTransaction);
    }

    public async Task<IEnumerable<TResult>> QueryAsync<T1, T2, TResult>(
        string sql,
        Func<T1, T2?, TResult> map,
        object? parameters = null,
        string splitOn = "Id",
        CancellationToken cancellationToken = default)
    {
        await using var cmd =
            await CreateCommandAsync(
                sql,
                parameters,
                cancellationToken);

        await using var reader =
            await cmd.ExecuteReaderAsync(
                cancellationToken);

        var results =
            new List<TResult>();

        var splitIndex =
            GetSplitIndex(
                reader,
                splitOn);

        var mapper1 =
            RapidMapperCache.CreatePartialMapper<T1>(
                reader,
                0,
                splitIndex);

        var mapper2 =
            RapidMapperCache.CreatePartialMapper<T2>(
                reader,
                splitIndex,
                reader.FieldCount);

        while (await reader.ReadAsync(
                   cancellationToken))
        {
            var obj1 =
                mapper1(reader);

            var obj2 =
                mapper2(reader);

            var result =
                map(obj1!, obj2);

            results.Add(result);
        }

        return results;
    }

    public async Task<IEnumerable<TResult>> QueryAsync<T1, T2, T3, TResult>(
        string sql,
        Func<T1, T2?, T3?, TResult> map,
        object? parameters = null,
        string splitOn = "Id,Id",
        CancellationToken cancellationToken = default)
    {
        await using var cmd =
            await CreateCommandAsync(
                sql,
                parameters,
                cancellationToken);

        await using var reader =
            await cmd.ExecuteReaderAsync(
                cancellationToken);

        var splits =
            GetSplitIndexes(
                reader,
                splitOn);

        var mapper1 =
            RapidMapperCache.CreatePartialMapper<T1>(
                reader,
                0,
                splits[0]);

        var mapper2 =
            RapidMapperCache.CreatePartialMapper<T2>(
                reader,
                splits[0],
                splits[1]);

        var mapper3 =
            RapidMapperCache.CreatePartialMapper<T3>(
                reader,
                splits[1],
                reader.FieldCount);

        var results =
            new List<TResult>();

        while (await reader.ReadAsync(
                   cancellationToken))
        {
            var obj1 =
                mapper1(reader);

            var obj2 =
                mapper2(reader);

            var obj3 =
                mapper3(reader);

            var result =
                map(obj1!, obj2, obj3);

            results.Add(result);
        }

        return results;
    }

    #region Metodos Privados
    private static int GetSplitIndex(
        IDataReader reader,
        string splitOn)
    {
        var matches =
            new List<int>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(
                    reader.GetName(i),
                    splitOn,
                    StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(i);
            }
        }

        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                $"splitOn column '{splitOn}' was not found in result set.");
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException(
                $"splitOn column '{splitOn}' is duplicated. Use alias (ex: AddressId).");
        }

        var index = matches[0];

        if (index == 0)
        {
            throw new InvalidOperationException(
                $"splitOn column '{splitOn}' cannot be the first column.");
        }

        return index;
    }
    private static int[] GetSplitIndexes(
        IDataReader reader,
        string splitOn)
    {
        var splitColumns =
            splitOn.Split(',');

        var indexes =
            new List<int>();

        foreach (var col in splitColumns)
        {
            var index =
                GetSplitIndex(
                    reader,
                    col.Trim());

            indexes.Add(index);
        }

        ValidateSplitOrder(indexes);

        return indexes.ToArray();
    }
    private static void ValidateSplitOrder(
        List<int> indexes)
    {
        for (int i = 1; i < indexes.Count; i++)
        {
            if (indexes[i] <= indexes[i - 1])
            {
                throw new InvalidOperationException(
                    "splitOn columns must appear in order in SELECT statement.");
            }
        }
    }
    #endregion
}