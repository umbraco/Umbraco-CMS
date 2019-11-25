using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;

namespace Umbraco.Web.Runtime
{
    // web's final composer composes after all user composers
    // and *also* after ICoreComposer (in case IUserComposer is disabled)
    [ComposeAfter(typeof(IUserComposer))]
    [ComposeAfter(typeof(ICoreComposer))]
    public class WebFinalComposer : ComponentComposer<WebFinalComponent>
    {
    }
}
