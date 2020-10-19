﻿using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.Testing;
using Umbraco.Examine;

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
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, HostingEnvironment, RuntimeState, luceneDir,
                //make parent id 999 so all are ignored
                validator: new ContentValueSetValidator(false, 999)))
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

                var found = searcher.CreateQuery().Id((string)node.Attribute("id")).Execute();

                Assert.AreEqual(0, found.TotalItemCount);
            }



        }

    }
}
