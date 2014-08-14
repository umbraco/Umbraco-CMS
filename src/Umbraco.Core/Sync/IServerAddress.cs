using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An interface exposing a server address to use for server syncing
    /// </summary>
    public interface IServerAddress
    {
        string ServerAddress { get; }

        //TODO : Should probably add things like port, protocol, server name, app id
    }
}