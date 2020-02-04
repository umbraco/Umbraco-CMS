using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Examine
{
    // We want to run after core composers since we are replacing some items
    [ComposeAfter(typeof(ICoreComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ExamineLuceneComposer : ComponentComposer<ExamineLuceneComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.RegisterUnique<IBackOfficeExamineSearcher, BackOfficeExamineSearcher>();
            composition.RegisterUnique<IUmbracoIndexesCreator, UmbracoIndexesCreator>();
            composition.RegisterUnique<IIndexDiagnosticsFactory, LuceneIndexDiagnosticsFactory>();
        }
    }
}
