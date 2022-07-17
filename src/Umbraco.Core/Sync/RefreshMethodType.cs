namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     Describes <see cref="RefreshInstruction" /> refresh action type.
/// </summary>
[Serializable]
public enum RefreshMethodType
{
    // NOTE
    // that enum should get merged somehow with MessageType and renamed somehow
    // but at the moment it is exposed in CacheRefresher webservice through RefreshInstruction
    // so for the time being we keep it as-is for backward compatibility reasons
    RefreshAll,
    RefreshByGuid,
    RefreshById,
    RefreshByIds,
    RefreshByJson,
    RemoveById,

    // would adding values break backward compatibility?
    // RemoveByIds

    // these are MessageType values
    // note that AnythingByInstance are local messages and cannot be distributed
    /*
    RefreshAll,
    RefreshById,
    RefreshByJson,
    RemoveById,
    RefreshByInstance,
    RemoveByInstance
    */

    // NOTE
    // in the future we want
    // RefreshAll
    // RefreshById / ByInstance (support enumeration of int or guid)
    // RemoveById / ByInstance (support enumeration of int or guid)
    // Notify (for everything JSON)
}
