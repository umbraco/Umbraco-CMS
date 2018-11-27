using System;
using System.Linq;
using Examine;
using LightInject;
using Lucene.Net.Store;
using NUnit.Framework;
using Umbraco.Tests.Testing;
using Umbraco.Examine;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Tests.UmbracoExamine
{

    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class EventsTest : ExamineBaseTest
    {
        [Test]
        public void Events_Ignoring_Node()
        {
            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, ScopeProvider.SqlContext, Container.GetInstance<PropertyEditorCollection>(),
                //make parent id 999 so all are ignored
                options: new UmbracoContentIndexerOptions(false, false, 999)))
            using (indexer.ProcessNonAsync())
            {
                var searcher = indexer.GetSearcher();

                var contentService = new ExamineDemoDataContentService();
                //get a node from the data repo
                var node = contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                                          .Root
                                          .Elements()
                                          .First();

                var valueSet = node.ConvertToValueSet(IndexTypes.Content);
                indexer.IndexItems(new[] { valueSet });

                var found = searcher.Search(searcher.CreateCriteria().Id((string)node.Attribute("id")).Compile());

                Assert.AreEqual(0, found.TotalItemCount);
            }



        }

    }
}
