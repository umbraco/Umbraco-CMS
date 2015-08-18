using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Event messages factory
    /// </summary>
    public interface IEventMessagesFactory
    {
        EventMessages Get();
    }
}