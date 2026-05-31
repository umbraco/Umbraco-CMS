using NPoco;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Repository responsible for querying and managing information about where data types are used within the Umbraco persistence layer.
/// </summary>
public class DataTypeUsageRepository : IDataTypeUsageRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeUsageRepository"/> class, which is responsible for managing data type usage information in the persistence layer.
    /// </summary>
    /// <param name="scopeAccessor">An accessor for the current database scope, used to manage database transactions and context.</param>
    public DataTypeUsageRepository(IScopeAccessor scopeAccessor)
    {
        _scopeAccessor = scopeAccessor;
    }

    /// <summary>
    /// Determines whether there are any saved values associated with the specified data type key.
    /// </summary>
    /// <param name="dataTypeKey">The unique identifier of the data type to check for saved values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if saved values exist; otherwise, false.</returns>
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
