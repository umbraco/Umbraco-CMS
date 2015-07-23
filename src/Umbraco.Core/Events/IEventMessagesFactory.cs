using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public interface IEventMessagesFactory
    {
        EventMessages CreateMessages();
    }

    /// <summary>
    /// A simple/default transient messages factory
    /// </summary>
    internal class TransientMessagesFactory : IEventMessagesFactory
    {
        public EventMessages CreateMessages()
        {
            return new EventMessages();
        }
    }
}