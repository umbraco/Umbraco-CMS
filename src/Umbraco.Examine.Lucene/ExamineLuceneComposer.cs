using System.Runtime.InteropServices;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Examine
{
    // We want to run after core composers since we are replacing some items
    [ComposeAfter(typeof(ICoreComposer))]
    public sealed class ExamineLuceneComposer : ComponentComposer<ExamineLuceneComponent>
    {
        public override void Compose(Composition composition)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if(!isWindows) return;


            base.Compose(composition);

            composition.Services.AddUnique<IBackOfficeExamineSearcher, BackOfficeExamineSearcher>();
            composition.Services.AddUnique<IUmbracoIndexesCreator, UmbracoIndexesCreator>();
            composition.Services.AddUnique<IIndexDiagnosticsFactory, LuceneIndexDiagnosticsFactory>();
            composition.Services.AddUnique<ILuceneDirectoryFactory, LuceneFileSystemDirectoryFactory>();
        }
    }
}
