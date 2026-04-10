using RapidMapper.Abstractions;

namespace RapidMapper.Configuration;

public class ConnectionRegistry : IConnectionRegistry
{
    private readonly Dictionary<
        DatabaseType,
        IDbConnectionFactory> _factories =
        new();

    public void Add(
        DatabaseType databaseType,
        IDbConnectionFactory factory)
    {
        _factories[databaseType] = factory;
    }

    public IDbConnectionFactory GetFactory(
        DatabaseType databaseType)
    {
        if (_factories.TryGetValue(
                databaseType,
                out var factory))
        {
            return factory;
        }

        throw new InvalidOperationException(
            $"Database '{databaseType}' não registrado.");
    }

    public IDbConnectionFactory GetDefault()
    {
        if (_factories.Count == 1)
            return _factories.First().Value;

        if (_factories.TryGetValue(
                DatabaseType.Default,
                out var factory))
        {
            return factory;
        }

        throw new InvalidOperationException("Existe mais de um banco configurado. Use .For(DatabaseType).");
    }

    public bool HasMultipleConnections()
        => _factories.Count > 1;
}