using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IIdKeyMap
    {
        Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType);
        Attempt<int> GetIdForUdi(Udi udi);
        Attempt<Udi> GetUdiForId(int id, UmbracoObjectTypes umbracoObjectType);
        Attempt<Guid> GetKeyForId(int id, UmbracoObjectTypes umbracoObjectType);
        void ClearCache();
        void ClearCache(int id);
        void ClearCache(Guid key);
    }
}
