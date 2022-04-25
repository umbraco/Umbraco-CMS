namespace Umbraco.Cms.Core.Events
{
    /// <summary>
    /// A simple/default transient messages factory
    /// </summary>
    public class TransientEventMessagesFactory : IEventMessagesFactory
    {
        public EventMessages Get()
        {
            return new EventMessages();
        }

        public EventMessages? GetOrDefault()
        {
            return null;
        }
    }
}
