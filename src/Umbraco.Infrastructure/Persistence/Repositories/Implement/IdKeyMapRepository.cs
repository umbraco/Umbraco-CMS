using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Repository for mapping between integer IDs and GUID keys for Umbraco objects.
///     Provides methods to retrieve an ID for a given key and object type, and vice versa.
/// </summary>
public class IdKeyMapRepository : IIdKeyMapRepository
{
    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _scopeAccessor;

    public IdKeyMapRepository(IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor) =>
        _scopeAccessor = scopeAccessor;

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
    public async Task<int?> GetIdForKeyAsync(Guid key, UmbracoObjectTypes umbracoObjectType)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return null;
        }

        return await _scopeAccessor.AmbientScope.ExecuteWithContextAsync(async db =>
        {
            if (umbracoObjectType == UmbracoObjectTypes.Unknown)
            {
                return await db.Nodes
                    .Where(x => x.UniqueId == key)
                    .Select(node => (int?)node.NodeId) // Cast needed to ensure null is returned if not found.
                    .FirstOrDefaultAsync();
            }

            Guid type = GetNodeObjectTypeGuid(umbracoObjectType);
            return await db.Nodes
                .Where(x => x.UniqueId == key
                            && (x.NodeObjectType == type
                                || x.NodeObjectType == Constants.ObjectTypes.IdReservation))
                .Select(node => (int?)node.NodeId) // Cast needed to ensure null is returned if not found.
                .FirstOrDefaultAsync();
        });
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
    public async Task<Guid?> GetIdForKeyAsync(int id, UmbracoObjectTypes umbracoObjectType)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return null;
        }

        return await _scopeAccessor.AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // if it's unknown don't include the nodeObjectType in the query
            if (umbracoObjectType == UmbracoObjectTypes.Unknown)
            {
                return await db.Nodes
                    .Where(x => x.NodeId == id)
                    .Select(node => (Guid?)node.UniqueId) // Cast needed to ensure null is returned if not found.
                    .FirstOrDefaultAsync();
            }

            Guid type = GetNodeObjectTypeGuid(umbracoObjectType);
            return await db.Nodes
                .Where(x => x.NodeId == id
                            && (x.NodeObjectType == type
                                || x.NodeObjectType == Constants.ObjectTypes.IdReservation))
                .Select(node => (Guid?)node.UniqueId) // Cast needed to ensure null is returned if not found.
                .FirstOrDefaultAsync();
        });
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
