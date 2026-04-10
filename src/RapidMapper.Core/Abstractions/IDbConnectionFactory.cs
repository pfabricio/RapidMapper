using System.Data.Common;

namespace RapidMapper.Abstractions;

public interface IDbConnectionFactory
{
    DbConnection Create();
}