using System.Data;
using System.Reflection;

namespace RapidMapper.Mapping;

public static class RapidAutoMapper
{
    public static T Map<T>(IDataReader reader)
    {
        var mapper = RapidMapperCache.GetMapper<T>(reader);
        return mapper(reader);
    }
}