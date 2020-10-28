using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// A simple/default transient messages factory
    /// </summary>
    [UmbracoVolatile]
    public class TransientEventMessagesFactory : IEventMessagesFactory
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
