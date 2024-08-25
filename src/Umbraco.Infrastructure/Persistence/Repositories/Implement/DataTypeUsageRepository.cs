using NPoco;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class DataTypeUsageRepository : IDataTypeUsageRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public DataTypeUsageRepository(IScopeAccessor scopeAccessor)
    {
        _scopeAccessor = scopeAccessor;
    }

    [Obsolete("Please use HasSavedValuesAsync. Scheduled for removable in Umbraco 15.")]
    public bool HasSavedValues(int dataTypeId)
    {
        IUmbracoDatabase? database = _scopeAccessor.AmbientScope?.Database;

        if (database is null)
        {
            throw new InvalidOperationException("A scope is required to query the database");
        }

        Sql<ISqlContext> selectQuery = database.SqlContext.Sql()
            .SelectAll()
            .From<PropertyTypeDto>("pt")
            .InnerJoin<PropertyDataDto>("pd")
            .On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id, "pd", "pt")
            .Where<PropertyTypeDto>(pt => pt.DataTypeId == dataTypeId, "pt");

        Sql<ISqlContext> hasValueQuery = database.SqlContext.Sql()
            .SelectAnyIfExists(selectQuery);

        return database.ExecuteScalar<bool>(hasValueQuery);
    }

    public async Task<bool> HasSavedValuesAsync(Guid dataTypeKey)
    {
        IUmbracoDatabase? database = _scopeAccessor.AmbientScope?.Database;

        if (database is null)
        {
            throw new InvalidOperationException("A scope is required to query the database");
        }

        Sql<ISqlContext> selectQuery = database.SqlContext.Sql()
            .SelectAll()
            .From<PropertyTypeDto>("pt")
            .InnerJoin<PropertyDataDto>("pd")
            .On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id, "pd", "pt")
            .InnerJoin<NodeDto>("n")
            .On<PropertyTypeDto, NodeDto>((pt, n) => pt.DataTypeId == n.NodeId, "pt", "n")
            .Where<NodeDto>(n => n.UniqueId == dataTypeKey, "n");

        Sql<ISqlContext> hasValueQuery = database.SqlContext.Sql()
            .SelectAnyIfExists(selectQuery);

        return await Task.FromResult(database.ExecuteScalar<bool>(hasValueQuery));
    }
}
