using System.Data;

namespace RapidMapper.Abstractions;

public interface IRapidTransaction: IAsyncDisposable
{
    IDbTransaction DbTransaction { get; }
    IDbConnectionFactory Factory { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}