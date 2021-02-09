using Umbraco.Cms.Core.Composing;

namespace Umbraco.Examine
{
    // examine's Lucene final composer composes after all user composers
    // and *also* after ICoreComposer (in case IUserComposer is disabled)
    [ComposeAfter(typeof(IUserComposer))]
    [ComposeAfter(typeof(ICoreComposer))]
    public class ExamineLuceneFinalComposer : ComponentComposer<ExamineLuceneFinalComponent>
    { }
}
