using Umbraco.Cms.Core.Composing;

namespace Umbraco.Web.Runtime
{
    // web's final composer composes after all user composers
    // and *also* after ICoreComposer (in case IUserComposer is disabled)
    [ComposeAfter(typeof(IUserComposer))]
    public class WebFinalComposer : ComponentComposer<WebFinalComponent>
    {
    }
}
