using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.Sqlite.Mappers;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

/// <summary>
///     Implements <see cref="IProviderSpecificMapperFactory" /> for SQLite.
/// </summary>
public class SqliteSpecificMapperFactory : IProviderSpecificMapperFactory
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    public NPocoMapperCollection Mappers => new(() => new[] { new SqlitePocoGuidMapper() });
}
