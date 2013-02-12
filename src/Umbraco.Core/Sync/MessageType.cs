namespace Umbraco.Core.Sync
{
    /// <summary>
    /// The message type to be used for syncing across servers
    /// </summary>
    internal enum MessageType
    {
        RefreshAll,
        RefreshById,
        RemoveById
    }
}