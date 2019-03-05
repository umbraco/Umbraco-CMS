using Umbraco.Core.Composing;

namespace Umbraco.Web.Runtime
{
    // web's final composer composes after all user composers
    [ComposeAfter(typeof(IUserComposer))]
    public class WebFinalComposer : ComponentComposer<WebFinalComponent>
    { }
}
