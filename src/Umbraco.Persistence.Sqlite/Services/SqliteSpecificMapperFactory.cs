using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Persistence.Sqlite.Services;

/// <summary>
/// Implements <see cref="IProviderSpecificMapperFactory"/> for SQLite.
/// </summary>
public class SqliteSpecificMapperFactory : IProviderSpecificMapperFactory
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    public NPocoMapperCollection Mappers => new NPocoMapperCollection(Array.Empty<IMapper>);
}
