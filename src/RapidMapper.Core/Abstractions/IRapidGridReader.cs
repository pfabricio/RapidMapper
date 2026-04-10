namespace RapidMapper.Abstractions;

public interface IRapidGridReader : IAsyncDisposable
{
    Task<IEnumerable<T>> ReadAsync<T>(CancellationToken cancellationToken = default);

    IAsyncEnumerable<T> ReadStreamAsync<T>(CancellationToken cancellationToken = default);
}