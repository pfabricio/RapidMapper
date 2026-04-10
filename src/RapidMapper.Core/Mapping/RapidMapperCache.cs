using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace RapidMapper.Mapping;

public static class RapidMapperCache
{
    private static readonly ConcurrentDictionary<Type, object> _cache = new();

    public static Func<IDataReader, T> GetMapper<T>(IDataReader reader)
    {
        var type = typeof(T);

        if (_cache.TryGetValue(type, out var existing))
            return (Func<IDataReader, T>)existing;

        var mapper = CreateMapper<T>(reader);
        _cache[type] = mapper;
        return mapper;
    }

    private static Func<IDataReader, T> CreateMapper<T>(
        IDataReader reader)
    {
        var readerParam =
            Expression.Parameter(
                typeof(IDataReader),
                "reader");

        var bindings =
            new List<MemberBinding>();

        var properties =
            typeof(T)
            .GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        var isDbNullMethod =
            typeof(IDataRecord)
            .GetMethod(nameof(IDataRecord.IsDBNull))!;

        var getValueMethod =
            typeof(IDataRecord)
            .GetMethod(nameof(IDataRecord.GetValue))!;

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName =
                reader.GetName(i);

            var property =
                properties.FirstOrDefault(p =>
                    string.Equals(
                        p.Name,
                        columnName,
                        StringComparison.OrdinalIgnoreCase));

            if (property == null)
                continue;

            var indexExpr =
                Expression.Constant(i);

            var isDbNullExpr =
                Expression.Call(
                    readerParam,
                    isDbNullMethod,
                    indexExpr);

            var getValueExpr =
                Expression.Call(
                    readerParam,
                    getValueMethod,
                    indexExpr);

            var targetType =
                Nullable.GetUnderlyingType(
                    property.PropertyType)
                ?? property.PropertyType;

            Expression convertExpr;

            if (targetType.IsEnum)
            {
                convertExpr =
                    Expression.Convert(
                        Expression.Call(
                            typeof(Enum),
                            nameof(Enum.ToObject),
                            null,
                            Expression.Constant(targetType),
                            getValueExpr),
                        property.PropertyType);
            }
            else
            {
                convertExpr =
                    Expression.Convert(
                        getValueExpr,
                        targetType);

                if (targetType != property.PropertyType)
                    convertExpr =
                        Expression.Convert(
                            convertExpr,
                            property.PropertyType);
            }

            var defaultValue =
                Expression.Default(
                    property.PropertyType);

            var safeValue =
                Expression.Condition(
                    isDbNullExpr,
                    defaultValue,
                    convertExpr);

            var bind =
                Expression.Bind(
                    property,
                    safeValue);

            bindings.Add(bind);
        }

        var newExpr =
            Expression.New(typeof(T));

        var memberInit =
            Expression.MemberInit(
                newExpr,
                bindings);

        var lambda =
            Expression.Lambda<Func<IDataReader, T>>(
                memberInit,
                readerParam);

        return lambda.Compile();
    }

    public static Func<IDataReader, T?> CreatePartialMapper<T>(
        IDataReader reader,
        int startIndex,
        int endIndex)
    {
        var props =
            typeof(T)
                .GetProperties()
                .Where(p => p.CanWrite)
                .ToDictionary(
                    p => ColumnNameNormalizer.Normalize(p.Name),
                    p => p);

        return reader =>
        {
            bool allNull = true;

            var obj =
                Activator.CreateInstance<T>();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (reader.IsDBNull(i))
                    continue;

                allNull = false;

                var column =
                    reader.GetName(i);

                var prop =
                    FindProperty(
                        props,
                        column);

                if (prop == null)
                    continue;

                var value =
                    reader.GetValue(i);

                prop.SetValue(
                    obj,
                    value);
            }

            if (allNull)
                return default;

            return obj;
        };
    }

    private static PropertyInfo? FindProperty(
        Dictionary<string, PropertyInfo> props,
        string columnName)
    {
        var normalizedColumn =
            ColumnNameNormalizer.Normalize(columnName);

        // match exato
        if (props.TryGetValue(
                normalizedColumn,
                out var exact))
            return exact;

        // match por sufixo
        foreach (var kv in props)
        {
            if (normalizedColumn.EndsWith(kv.Key))
                return kv.Value;
        }

        return null;
    }
}