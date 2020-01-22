using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IIdKeyMap
    {
        void SetMapper(UmbracoObjectTypes umbracoObjectType, Func<int, Guid> id2key, Func<Guid, int> key2id);
        Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType);
        Attempt<int> GetIdForUdi(Udi udi);
        Attempt<Udi> GetUdiForId(int id, UmbracoObjectTypes umbracoObjectType);
        Attempt<Guid> GetKeyForId(int id, UmbracoObjectTypes umbracoObjectType);
        void ClearCache();
        void ClearCache(int id);
        void ClearCache(Guid key);
    }
}
