using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class IdKeyMapRepository : IIdKeyMapRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public IdKeyMapRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    public int? GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
    {
        // if it's unknown don't include the nodeObjectType in the query
        if (umbracoObjectType == UmbracoObjectTypes.Unknown)
        {
            return _scopeAccessor.AmbientScope?.Database.ExecuteScalar<int?>(
                "SELECT id FROM umbracoNode WHERE uniqueId=@id", new { id = key });
        }

        return _scopeAccessor.AmbientScope?.Database.ExecuteScalar<int?>(
            "SELECT id FROM umbracoNode WHERE uniqueId=@id AND (nodeObjectType=@type OR nodeObjectType=@reservation)",
            new
            {
                id = key,
                type = GetNodeObjectTypeGuid(umbracoObjectType),
                reservation = Constants.ObjectTypes.IdReservation,
            });
    }

    public Guid? GetIdForKey(int id, UmbracoObjectTypes umbracoObjectType)
    {
        // if it's unknown don't include the nodeObjectType in the query
        if (umbracoObjectType == UmbracoObjectTypes.Unknown)
        {
            return _scopeAccessor.AmbientScope?.Database.ExecuteScalar<Guid?>(
                "SELECT uniqueId FROM umbracoNode WHERE id=@id", new { id });
        }

        return _scopeAccessor.AmbientScope?.Database.ExecuteScalar<Guid?>(
            "SELECT uniqueId FROM umbracoNode WHERE id=@id AND (nodeObjectType=@type OR nodeObjectType=@reservation)",
            new
            {
                id,
                type = GetNodeObjectTypeGuid(umbracoObjectType),
                reservation = Constants.ObjectTypes.IdReservation,
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
