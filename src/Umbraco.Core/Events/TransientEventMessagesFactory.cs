namespace Umbraco.Core.Events
{
    /// <summary>
    /// A simple/default transient messages factory
    /// </summary>
    internal class TransientEventMessagesFactory : IEventMessagesFactory
    {
        public EventMessages Get()
        {
            return new EventMessages();
        }

        public EventMessages GetOrDefault()
        {
            return null;
        }
    }
}
