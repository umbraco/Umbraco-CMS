using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Search
{
    // examine's final composer composes after all user composers
    // and *also* after ICoreComposer (in case IUserComposer is disabled)
    [ComposeAfter(typeof(IUserComposer))]
    [ComposeAfter(typeof(ICoreComposer))]
    public class ExamineFinalComposer : ComponentComposer<ExamineFinalComponent>
    { }
}
