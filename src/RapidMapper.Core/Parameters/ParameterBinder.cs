using System.Data.Common;
using System.Reflection;

namespace RapidMapper.Parameters;

public static class ParameterBinder
{
    public static void Bind(
        DbCommand command,
        object? parameters)
    {
        if (parameters == null)
            return;

        foreach (var prop in parameters
                     .GetType()
                     .GetProperties(
                         BindingFlags.Public |
                         BindingFlags.Instance))
        {
            var parameter =
                command.CreateParameter();

            parameter.ParameterName =
                "@" + prop.Name;

            parameter.Value =
                prop.GetValue(parameters)
                ?? DBNull.Value;

            command.Parameters.Add(parameter);
        }
    }
}