using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Repository for mapping between integer IDs and GUID keys for Umbraco objects.
///     Provides methods to retrieve an ID for a given key and object type, and vice versa.
/// </summary>
public class IdKeyMapRepository(IScopeAccessor scopeAccessor) : IIdKeyMapRepository
{
    /// <summary>
    /// Retrieves the unique identifier (ID) for a specified key and object type.
    /// </summary>
    /// <remarks>If <paramref name="umbracoObjectType"/> is <see cref="UmbracoObjectTypes.Unknown"/>, the
    /// query will not filter by object type. For other object types, the query will match the specified object type or
    /// reserved object type.</remarks>
    /// <param name="key">The globally unique identifier (GUID) of the object to look up.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object to filter the query. Use <see cref="UmbracoObjectTypes.Unknown"/> to ignore the
    /// object type in the query.</param>
    /// <returns>The unique identifier (ID) of the object if found; otherwise, <see langword="null"/>.</returns>
    public int? GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            return null;
        }

        ISqlContext sqlContext = scopeAccessor.AmbientScope.SqlContext;
        IUmbracoDatabase database = scopeAccessor.AmbientScope.Database;
        Sql<ISqlContext>? sql;

        // if it's unknown don't include the nodeObjectType in the query
        if (umbracoObjectType == UmbracoObjectTypes.Unknown)
        {
            sql = sqlContext.Sql()
                .Select<NodeDto>(c => c.NodeId)
                .From<NodeDto>()
                .Where<NodeDto>(n => n.UniqueId == key);
            return database?.ExecuteScalar<int?>(sql);
        }

        Guid type = GetNodeObjectTypeGuid(umbracoObjectType);
        sql = sqlContext.Sql()
            .Select<NodeDto>(c => c.NodeId)
            .From<NodeDto>()
            .Where<NodeDto>(n =>
                n.UniqueId == key
                && (n.NodeObjectType == type
                    || n.NodeObjectType == Constants.ObjectTypes.IdReservation));
        return database.ExecuteScalar<int?>(sql);
    }

    /// <summary>
    /// Retrieves the unique identifier (GUID) associated with a specified node ID and object type.
    /// </summary>
    /// <remarks>If the <paramref name="umbracoObjectType"/> is <see cref="UmbracoObjectTypes.Unknown"/>,  the
    /// query will only filter by the node ID. For other object types, the query will filter  by both the node ID and
    /// the object type, including reserved object types.</remarks>
    /// <param name="id">The ID of the node to retrieve the unique identifier for.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object to filter by. If set to <see cref="UmbracoObjectTypes.Unknown"/>,  the object
    /// type is not included in the query.</param>
    /// <returns>The unique identifier (GUID) of the node if found; otherwise, <see langword="null"/>.</returns>
    public Guid? GetIdForKey(int id, UmbracoObjectTypes umbracoObjectType)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            return null;
        }

        ISqlContext sqlContext = scopeAccessor.AmbientScope.SqlContext;
        IUmbracoDatabase database = scopeAccessor.AmbientScope.Database;
        Sql<ISqlContext> sql;

        // if it's unknown don't include the nodeObjectType in the query
        if (umbracoObjectType == UmbracoObjectTypes.Unknown)
        {
            sql = sqlContext.Sql()
                .Select<NodeDto>(c => c.UniqueId)
                .From<NodeDto>()
                .Where<NodeDto>(n => n.NodeId == id);

            // We must use FirstOrDefault over ExecuteScalar when retrieving a nullable Guid, to ensure we go through the full NPoco mapping pipeline.
            // Without that, though it will succeed on SQLite and SQLServer, it could fail on other database providers.
            return database?.FirstOrDefault<Guid?>(sql);
        }

        Guid type = GetNodeObjectTypeGuid(umbracoObjectType);
        sql = sqlContext.Sql()
            .Select<NodeDto>(c => c.UniqueId)
            .From<NodeDto>()
            .Where<NodeDto>(n =>
                n.NodeId == id
                && (n.NodeObjectType == type
                    || n.NodeObjectType == Constants.ObjectTypes.IdReservation));

        // We must use FirstOrDefault over ExecuteScalar when retrieving a nullable Guid, to ensure we go through the full NPoco mapping pipeline.
        // Without that, though it will succeed on SQLite and SQLServer, it could fail on other database providers.
        return database?.FirstOrDefault<Guid?>(sql);
    }

    private Guid GetNodeObjectTypeGuid(UmbracoObjectTypes umbracoObjectType)
    {
        Guid guid = umbracoObjectType.GetGuid();
        if (guid == Guid.Empty)
        {
            throw new NotSupportedException("Unsupported object type (" + umbracoObjectType + ").");
        }

        return guid;
    }
}
