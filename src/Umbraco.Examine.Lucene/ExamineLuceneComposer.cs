//// Copyright (c) Umbraco.
//// See LICENSE for more details.

//using System.Runtime.InteropServices;
//using Umbraco.Cms.Core.Composing;
//using Umbraco.Cms.Core.DependencyInjection;
//using Umbraco.Extensions;

//namespace Umbraco.Cms.Infrastructure.Examine
//{
//    // We want to run after core composers since we are replacing some items
//    [ComposeAfter(typeof(ICoreComposer))]
//    public sealed class ExamineLuceneComposer : ComponentComposer<ExamineLuceneComponent>
//    {
//        public override void Compose(IUmbracoBuilder builder)
//        {
//            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
//            if(!isWindows) return;


//            base.Compose(builder);

//            builder.Services.AddUnique<IBackOfficeExamineSearcher, BackOfficeExamineSearcher>();
//            builder.Services.AddUnique<IUmbracoIndexesCreator, UmbracoIndexesCreator>();
//            builder.Services.AddUnique<IIndexDiagnosticsFactory, LuceneIndexDiagnosticsFactory>();
//            builder.Services.AddUnique<ILuceneDirectoryFactory, LuceneFileSystemDirectoryFactory>();
//        }
//    }
//}
