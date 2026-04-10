using RapidMapper.Abstractions;

namespace RapidMapper.Configuration;

public interface IConnectionRegistry
{
    void Add(
        DatabaseType databaseType,
        IDbConnectionFactory factory);

    IDbConnectionFactory GetFactory(
        DatabaseType databaseType);

    IDbConnectionFactory GetDefault();

    bool HasMultipleConnections();
}