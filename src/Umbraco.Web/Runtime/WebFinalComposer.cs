using Umbraco.Core.Composing;

namespace Umbraco.Web.Runtime
{
    // composes after absolutely everything else = last
    [ComposeAfter(typeof(IUserComposer))] // after IUserComposer, which comes after ICoreComposer, which comes after IRuntimeComposer
    [ComposeAfter(typeof(IComposer))] // after plain IComposer
    public class WebFinalComposer : ComponentComposer<WebFinalComponent>
    { }
}
