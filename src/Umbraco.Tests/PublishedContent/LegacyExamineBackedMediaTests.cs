using System;
using System.IO;
using System.Linq;
using Lucene.Net.Documents;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Tests.TestHelpers.ExamineHelpers;
using umbraco.MacroEngines;

namespace Umbraco.Tests.PublishedContent
{
    public class LegacyExamineBackedMediaTests : PublishedContentTestBase
    {
        public override void Initialize()
        {
            base.Initialize();
            UmbracoSettings.ForceSafeAliases = true;
            UmbracoSettings.UmbracoLibraryCacheDuration = 1800;
            UmbracoSettings.ForceSafeAliases = true;
        }

        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Ensure_Children_Are_Sorted()
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            var indexInit = new IndexInitializer();
            var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);
            indexer.RebuildIndex();

            var searcher = indexInit.GetUmbracoSearcher(newIndexFolder);
            var result = searcher.Search(searcher.CreateSearchCriteria().Id(1111).Compile());
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TotalItemCount);

            var searchItem = result.First();
            var backedMedia = new ExamineBackedMedia(searchItem, indexer, searcher);
            var children = backedMedia.ChildrenAsList.Value;

            var currSort = 0;
            for (var i = 0; i < children.Count(); i++)
            {
                Assert.GreaterOrEqual(children[i].SortOrder, currSort);
                currSort = children[i].SortOrder;
            }

        }

        [Test]
        public void Ensure_Result_Has_All_Values()
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            var indexInit = new IndexInitializer();
            var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);
            indexer.RebuildIndex();
            
            var searcher = indexInit.GetUmbracoSearcher(newIndexFolder);
            var result = searcher.Search(searcher.CreateSearchCriteria().Id(1111).Compile());
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TotalItemCount);

            var searchItem = result.First();
            var backedMedia = new ExamineBackedMedia(searchItem, indexer, searcher);

            Assert.AreEqual(searchItem.Id, backedMedia.Id);
            Assert.AreEqual(searchItem.Fields["sortOrder"], backedMedia.SortOrder.ToString());
            Assert.AreEqual(searchItem.Fields["urlName"], backedMedia.UrlName);
            Assert.AreEqual(DateTools.StringToDate(searchItem.Fields["createDate"]), backedMedia.CreateDate);
            Assert.AreEqual(DateTools.StringToDate(searchItem.Fields["updateDate"]), backedMedia.UpdateDate);
            Assert.AreEqual(Guid.Parse(searchItem.Fields["version"]), backedMedia.Version);
            Assert.AreEqual(searchItem.Fields["level"], backedMedia.Level.ToString());
            Assert.AreEqual(searchItem.Fields["writerID"], backedMedia.WriterID.ToString());
            Assert.AreEqual(searchItem.Fields["writerID"], backedMedia.CreatorID.ToString()); //there's only writerId in the xml
            Assert.AreEqual(searchItem.Fields["writerName"], backedMedia.CreatorName);
            Assert.AreEqual(searchItem.Fields["writerName"], backedMedia.WriterName); //tehre's only writer name in the xml
        }
    }
}