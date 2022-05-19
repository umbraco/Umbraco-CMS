namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     The message type to be used for syncing across servers.
/// </summary>
public enum MessageType
{
    RefreshAll,
    RefreshById,
    RefreshByJson,
    RemoveById,
    RefreshByInstance,
    RemoveByInstance,
    RefreshByPayload,
}
