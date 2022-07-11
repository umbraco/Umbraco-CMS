using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IIdKeyMapRepository
{
    int? GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType);

    Guid? GetIdForKey(int id, UmbracoObjectTypes umbracoObjectType);
}
