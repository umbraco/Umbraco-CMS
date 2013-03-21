namespace Umbraco.Core.Sync
{
    /// <summary>
    /// The message type to be used for syncing across servers
    /// </summary>
    public enum MessageType
    {
        RefreshAll,
        RefreshById,
        RefreshByJson,
        RemoveById,
        RemoveByJson,
        RefreshByInstance,
        RemoveByInstance
    }
}