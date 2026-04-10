using System.Text;

namespace RapidMapper.Mapping;

internal static class ColumnNameNormalizer
{
    public static string Normalize(
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        name = name.ToLowerInvariant();

        var sb =
            new StringBuilder();

        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c))
                sb.Append(c);
        }

        return sb.ToString();
    }
}