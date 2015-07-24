namespace Umbraco.Core.Events
{
    /// <summary>
    /// A simple/default transient messages factory
    /// </summary>
    internal class TransientMessagesFactory : IEventMessagesFactory
    {
        public EventMessages Get()
        {
            return new EventMessages();
        }
    }
}