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

    public Task<bool> HasSavedValuesAsync(Guid dataTypeKey)
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

        return Task.FromResult(database.ExecuteScalar<bool>(hasValueQuery));
    }
}
