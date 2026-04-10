using Microsoft.Extensions.DependencyInjection;
using RapidMapper.Configuration;
using RapidMapper.SqlServer;
using RapidMapper.PostgreSql;
using RapidMapper.MySql;

namespace RapidMapper.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRapidMapper(
        this IServiceCollection services)
    {
        services.AddSingleton<ConnectionRegistry>();

        services.AddScoped<Execution.RapidMapper>();

        return services;
    }

    public static IServiceCollection AddSqlServer(
        this IServiceCollection services,
        string connectionString,
        DatabaseType type =
            DatabaseType.Default)
    {
        services.ConfigureRegistry(registry =>
        {
            registry.Add(
                type,
                new SqlConnectionFactory(
                    connectionString));
        });

        return services;
    }

    public static IServiceCollection AddPostgreSql(
        this IServiceCollection services,
        string connectionString,
        DatabaseType type)
    {
        services.ConfigureRegistry(registry =>
        {
            registry.Add(
                type,
                new NpgsqlConnectionFactory(
                    connectionString));
        });

        return services;
    }

    public static IServiceCollection AddMySql(
        this IServiceCollection services,
        string connectionString,
        DatabaseType type)
    {
        services.ConfigureRegistry(registry =>
        {
            registry.Add(
                type,
                new MySqlConnectionFactory(
                    connectionString));
        });

        return services;
    }

    private static void ConfigureRegistry(
        this IServiceCollection services,
        Action<ConnectionRegistry> configure)
    {
        services.AddSingleton(sp =>
        {
            var registry =
                sp.GetRequiredService<
                    ConnectionRegistry>();

            configure(registry);

            return registry;
        });
    }
}