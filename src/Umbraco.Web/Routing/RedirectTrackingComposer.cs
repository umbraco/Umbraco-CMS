using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Implements an Application Event Handler for managing redirect URLs tracking.
    /// </summary>
    /// <remarks>
    /// <para>when content is renamed or moved, we want to create a permanent 301 redirect from it's old URL</para>
    /// <para>not managing domains because we don't know how to do it - changing domains => must create a higher level strategy using rewriting rules probably</para>
    /// <para>recycle bin = moving to and from does nothing: to = the node is gone, where would we redirect? from = same</para>
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class RedirectTrackingComposer : ComponentComposer<RedirectTrackingComponent>, ICoreComposer
    { }
}
