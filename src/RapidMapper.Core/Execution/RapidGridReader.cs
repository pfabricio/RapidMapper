using RapidMapper.Mapping;
using System.Data.Common;
using RapidMapper.Abstractions;

namespace RapidMapper.Execution;

public class RapidGridReader : IRapidGridReader
{
    private readonly DbDataReader _reader;

    public RapidGridReader(
        DbDataReader reader)
    {
        _reader = reader;
    }

    public async Task<IEnumerable<T>> ReadAsync<T>(
        CancellationToken cancellationToken = default)
    {
        var list =
            new List<T>();

        var mapper =
            RapidMapperCache
                .GetMapper<T>(_reader);

        while (await _reader.ReadAsync(
                   cancellationToken))
        {
            list.Add(
                mapper(_reader));
        }

        await _reader.NextResultAsync(
            cancellationToken);

        return list;
    }

    public async IAsyncEnumerable<T> ReadStreamAsync<T>(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var mapper =
            RapidMapperCache
                .GetMapper<T>(_reader);

        while (await _reader.ReadAsync(
                   cancellationToken))
        {
            yield return mapper(_reader);
        }

        await _reader.NextResultAsync(
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _reader.DisposeAsync();
    }
}