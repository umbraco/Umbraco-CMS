using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine
{

    /// <summary>
    /// Tests the standard indexing capabilities
    /// </summary>
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.None)]
    public class IndexTest : ExamineBaseTest
    {
        [Test]
        public void Events_Ignoring_Node()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var index = IndexInitializer.GetUmbracoIndexer(HostingEnvironment, RunningRuntimeState, luceneDir,
                //make parent id 999 so all are ignored
                validator: new ContentValueSetValidator(false, 999)))

            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                var searcher = index.Searcher;

                var contentService = new ExamineDemoDataContentService();
                //get a node from the data repo
                var node = contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                                          .Root
                                          .Elements()
                                          .First();

                var valueSet = node.ConvertToValueSet(IndexTypes.Content);
                index.IndexItems(new[] { valueSet });

                var found = searcher.CreateQuery().Id((string)node.Attribute("id")).Execute();

                Assert.AreEqual(0, found.TotalItemCount);
            }



        }

        [Test]
        public void Index_Property_Data_With_Value_Indexer()
        {
            var contentValueSetBuilder = IndexInitializer.GetContentValueSetBuilder(false);

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var index = IndexInitializer.GetUmbracoIndexer(
                HostingEnvironment,
                RunningRuntimeState,
                luceneDir,
                validator: new ContentValueSetValidator(false)))

            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                index.CreateIndex();

                var contentType = ContentTypeBuilder.CreateBasicContentType();
                contentType.AddPropertyType(new PropertyType(TestHelper.ShortStringHelper, "test", ValueStorageType.Ntext)
                {
                    Alias = "grid",
                    Name = "Grid",
                    PropertyEditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.Grid
                });
                var content = ContentBuilder.CreateBasicContent(contentType);
                content.Id = 555;
                content.Path = "-1,555";
                var gridVal = new GridValue
                {
                    Name = "n1",
                    Sections = new List<GridValue.GridSection>
                    {
                        new GridValue.GridSection
                        {
                            Grid = "g1",
                            Rows = new List<GridValue.GridRow>
                            {
                                new GridValue.GridRow
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "row1",
                                    Areas = new List<GridValue.GridArea>
                                    {
                                        new GridValue.GridArea
                                        {
                                            Grid = "g2",
                                            Controls = new List<GridValue.GridControl>
                                            {
                                                new GridValue.GridControl
                                                {
                                                    Editor = new GridValue.GridEditor
                                                    {
                                                        Alias = "editor1",
                                                        View = "view1"
                                                    },
                                                    Value = "value1"
                                                },
                                                new GridValue.GridControl
                                                {
                                                    Editor = new GridValue.GridEditor
                                                    {
                                                        Alias = "editor1",
                                                        View = "view1"
                                                    },
                                                    Value = "value2"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(gridVal);
                content.Properties["grid"].SetValue(json);

                var valueSet = contentValueSetBuilder.GetValueSets(content);
                index.IndexItems(valueSet);

                var results = index.Searcher.CreateQuery().Id(555).Execute();
                Assert.AreEqual(1, results.TotalItemCount);

                var result = results.First();
                Assert.IsTrue(result.Values.ContainsKey("grid.row1"));
                Assert.AreEqual("value1", result.AllValues["grid.row1"][0]);
                Assert.AreEqual("value2", result.AllValues["grid.row1"][1]);
                Assert.IsTrue(result.Values.ContainsKey("grid"));
                Assert.AreEqual("value1 value2 ", result["grid"]);
                Assert.IsTrue(result.Values.ContainsKey($"{UmbracoExamineFieldNames.RawFieldPrefix}grid"));
                Assert.AreEqual(json, result[$"{UmbracoExamineFieldNames.RawFieldPrefix}grid"]);
            }
        }

        [Test]
        public void Rebuild_Index()
        {
            var contentRebuilder = IndexInitializer.GetContentIndexRebuilder(IndexInitializer.GetMockContentService(), false);
            var mediaRebuilder = IndexInitializer.GetMediaIndexRebuilder(IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var index = IndexInitializer.GetUmbracoIndexer(
                HostingEnvironment,
                RunningRuntimeState,
                luceneDir,
                validator: new ContentValueSetValidator(false)))
            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                //create the whole thing
                contentRebuilder.Populate(index);
                mediaRebuilder.Populate(index);

                var result = index.Searcher.CreateQuery().All().Execute();

                Assert.AreEqual(29, result.TotalItemCount);
            }
        }

        ///// <summary>
        /// <summary>
        /// Check that the node signalled as protected in the content service is not present in the index.
        /// </summary>
        [Test]
        public void Index_Protected_Content_Not_Indexed()
        {
            var rebuilder = IndexInitializer.GetContentIndexRebuilder(IndexInitializer.GetMockContentService(), false);

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var index = IndexInitializer.GetUmbracoIndexer(HostingEnvironment, RunningRuntimeState, luceneDir))
            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                var searchContext = ((LuceneSearcher)index.Searcher).GetSearchContext();

                using (var searchRef = searchContext.GetSearcher())
                {
                    var searcher = searchRef.IndexSearcher;

                    //create the whole thing
                    rebuilder.Populate(index);

                    var protectedQuery = new BooleanQuery();
                    protectedQuery.Add(
                        new BooleanClause(
                            new TermQuery(new Term(ExamineFieldNames.CategoryFieldName, IndexTypes.Content)),
                            Occur.MUST));

                    protectedQuery.Add(
                        new BooleanClause(
                            new TermQuery(new Term(ExamineFieldNames.ItemIdFieldName, ExamineDemoDataContentService.ProtectedNode.ToString())),
                            Occur.MUST));

                    var collector = TopScoreDocCollector.Create(100, true);

                    searcher.Search(protectedQuery, collector);

                    Assert.AreEqual(0, collector.TotalHits, "Protected node should not be indexed");
                }
            }
        }

        [Test]
        public void Index_Move_Media_From_Non_Indexable_To_Indexable_ParentID()
        {
            // create a validator with
            // publishedValuesOnly false
            // parentId 1116 (only content under that parent will be indexed)
            var validator = new ContentValueSetValidator(false, 1116);

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var index = IndexInitializer.GetUmbracoIndexer(HostingEnvironment, RunningRuntimeState, luceneDir, validator: validator))
            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                //get a node from the data repo (this one exists underneath 2222)
                var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                                        .Root.Elements()
                                        .First(x => (int)x.Attribute("id") == 2112);

                var currPath = (string)node.Attribute("path"); //should be : -1,1111,2222,2112
                Assert.AreEqual("-1,1111,2222,2112", currPath);

                //ensure it's indexed
                index.IndexItem(node.ConvertToValueSet(IndexTypes.Media));

                //it will not exist because it exists under 2222
                var results = index.Searcher.CreateQuery().Id(2112).Execute();
                Assert.AreEqual(0, results.Count());

                //now mimic moving 2112 to 1116
                //node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
                node.SetAttributeValue("path", "-1,1116,2112");
                node.SetAttributeValue("parentID", "1116");

                //now reindex the node, this should first delete it and then WILL add it because of the parent id constraint
                index.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Media) });

                //now ensure it exists
                results = index.Searcher.CreateQuery().Id(2112).Execute();
                Assert.AreEqual(1, results.Count());
            }
        }

        [Test]
        public void Index_Move_Media_To_Non_Indexable_ParentID()
        {
            // create a validator with
            // publishedValuesOnly false
            // parentId 2222 (only content under that parent will be indexed)
            var validator = new ContentValueSetValidator(false, 2222);

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var index = IndexInitializer.GetUmbracoIndexer(HostingEnvironment, RunningRuntimeState, luceneDir, validator: validator))
            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                var searcher = index.Searcher;

                //get a node from the data repo (this one exists underneath 2222)
                var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                                    .Root.Elements()
                                    .First(x => (int)x.Attribute("id") == 2112);

                var currPath = (string)node.Attribute("path"); //should be : -1,1111,2222,2112
                Assert.AreEqual("-1,1111,2222,2112", currPath);

                //ensure it's indexed
                index.IndexItem(node.ConvertToValueSet(IndexTypes.Media));

                //it will exist because it exists under 2222
                var results = searcher.CreateQuery().Id(2112).Execute();
                Assert.AreEqual(1, results.Count());

                //now mimic moving the node underneath 1116 instead of 2222
                node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
                node.SetAttributeValue("parentID", "1116");

                //now reindex the node, this should first delete it and then NOT add it because of the parent id constraint
                index.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Media) });

                //now ensure it's deleted
                results = searcher.CreateQuery().Id(2112).Execute();
                Assert.AreEqual(0, results.Count());
            }
        }


        /// <summary>
        /// This will ensure that all 'Content' (not media) is cleared from the index using the Lucene API directly.
        /// We then call the Examine method to re-index Content and do some comparisons to ensure that it worked correctly.
        /// </summary>
        [Test]
        public void Index_Reindex_Content()
        {
            var rebuilder = IndexInitializer.GetContentIndexRebuilder(IndexInitializer.GetMockContentService(), false);

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var index = IndexInitializer.GetUmbracoIndexer(HostingEnvironment, RunningRuntimeState, luceneDir,
                validator: new ContentValueSetValidator(false)))
            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                //create the whole thing
                rebuilder.Populate(index);

                var result = index.Searcher
                    .CreateQuery()
                    .Field(ExamineFieldNames.CategoryFieldName, IndexTypes.Content)
                    .Execute();
                Assert.AreEqual(21, result.TotalItemCount);

                //delete all content
                index.DeleteFromIndex(result.Select(x => x.Id));

                //ensure it's all gone
                result = index.Searcher.CreateQuery().Field(ExamineFieldNames.CategoryFieldName, IndexTypes.Content).Execute();
                Assert.AreEqual(0, result.TotalItemCount);

                //call our indexing methods
                rebuilder.Populate(index);

                index.WaitForChanges();

                result = index.Searcher
                    .CreateQuery()
                    .Field(ExamineFieldNames.CategoryFieldName, IndexTypes.Content)
                    .Execute();

                Assert.AreEqual(21, result.TotalItemCount);
            }
        }

        /// <summary>
        /// This will delete an item from the index and ensure that all children of the node are deleted too!
        /// </summary>
        [Test]
        public void Index_Delete_Index_Item_Ensure_Heirarchy_Removed()
        {
            var rebuilder = IndexInitializer.GetContentIndexRebuilder(IndexInitializer.GetMockContentService(), false);

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var index = IndexInitializer.GetUmbracoIndexer(HostingEnvironment, RunningRuntimeState, luceneDir))
            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                var searcher = index.Searcher;

                //create the whole thing
                rebuilder.Populate(index);

                //now delete a node that has children

                index.DeleteFromIndex(1140.ToString());
                //this node had children: 1141 & 1142, let's ensure they are also removed

                var results = searcher.CreateQuery().Id(1141).Execute();
                Assert.AreEqual(0, results.Count());

                results = searcher.CreateQuery().Id(1142).Execute();
                Assert.AreEqual(0, results.Count());

            }
        }

        private readonly ExamineDemoDataMediaService _mediaService = new ExamineDemoDataMediaService();
    }
}
