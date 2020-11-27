using System.Linq;
using System.Threading;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using NUnit.Framework;
using Umbraco.Tests.Testing;
using Umbraco.Examine;
using Umbraco.Core.Composing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Umbraco.Core;

namespace Umbraco.Tests.UmbracoExamine
{
    /// <summary>
    /// Tests the standard indexing capabilities
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class IndexTest : ExamineBaseTest
    {
        [Test]
        public void Index_Property_Data_With_Value_Indexer()
        {
            var contentValueSetBuilder = IndexInitializer.GetContentValueSetBuilder(Factory.GetInstance<PropertyEditorCollection>(), ScopeProvider, false);

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(false)))
            using (indexer.ProcessNonAsync())
            {
                indexer.CreateIndex();

                var contentType = MockedContentTypes.CreateBasicContentType();
                contentType.AddPropertyType(new PropertyType("test", ValueStorageType.Ntext)
                {
                    Alias = "grid",
                    Name = "Grid",
                    PropertyEditorAlias = Core.Constants.PropertyEditors.Aliases.Grid
                });
                var content = MockedContent.CreateBasicContent(contentType);
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
                indexer.IndexItems(valueSet);

                var searcher = indexer.GetSearcher();

                var results = searcher.CreateQuery().Id(555).Execute();
                Assert.AreEqual(1, results.TotalItemCount);

                var result = results.First();
                Assert.IsTrue(result.Values.ContainsKey("grid.row1"));
                Assert.AreEqual("value1", result.AllValues["grid.row1"][0]);
                Assert.AreEqual("value2", result.AllValues["grid.row1"][1]);
                Assert.IsTrue(result.Values.ContainsKey("grid"));
                Assert.AreEqual("value1 value2 ", result["grid"]);
                Assert.IsTrue(result.Values.ContainsKey($"{UmbracoExamineIndex.RawFieldPrefix}grid"));
                Assert.AreEqual(json, result[$"{UmbracoExamineIndex.RawFieldPrefix}grid"]);
            }
        }

        [Test]
        public void Rebuild_Index()
        {
            var contentRebuilder = IndexInitializer.GetContentIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockContentService(), ScopeProvider, false);
            var mediaRebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(false)))
            using (indexer.ProcessNonAsync())
            {

                var searcher = indexer.GetSearcher();

                //create the whole thing
                contentRebuilder.Populate(indexer);
                mediaRebuilder.Populate(indexer);

                var result = searcher.CreateQuery().All().Execute();

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
            var rebuilder = IndexInitializer.GetContentIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockContentService(), ScopeProvider, false);


            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir))
            using (indexer.ProcessNonAsync())
            using (var searcher = ((LuceneSearcher)indexer.GetSearcher()).GetLuceneSearcher())
            {
                //create the whole thing
                rebuilder.Populate(indexer);


                var protectedQuery = new BooleanQuery();
                protectedQuery.Add(
                    new BooleanClause(
                        new TermQuery(new Term(LuceneIndex.CategoryFieldName, IndexTypes.Content)),
                        Occur.MUST));

                protectedQuery.Add(
                    new BooleanClause(
                        new TermQuery(new Term(LuceneIndex.ItemIdFieldName, ExamineDemoDataContentService.ProtectedNode.ToString())),
                        Occur.MUST));

                var collector = TopScoreDocCollector.Create(100, true);

                searcher.Search(protectedQuery, collector);

                Assert.AreEqual(0, collector.TotalHits, "Protected node should not be indexed");
            }

        }

        [Test]
        public void Index_Move_Media_From_Non_Indexable_To_Indexable_ParentID()
        {
            // create a validator with
            // publishedValuesOnly false
            // parentId 1116 (only content under that parent will be indexed)
            var validator = new ContentValueSetValidator(false, 1116);

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, validator: validator))
            using (indexer.ProcessNonAsync())
            {
                var searcher = indexer.GetSearcher();

                //get a node from the data repo (this one exists underneath 2222)
                var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                                        .Root.Elements()
                                        .First(x => (int) x.Attribute("id") == 2112);

                var currPath = (string)node.Attribute("path"); //should be : -1,1111,2222,2112
                Assert.AreEqual("-1,1111,2222,2112", currPath);

                //ensure it's indexed
                indexer.IndexItem(node.ConvertToValueSet(IndexTypes.Media));

                //it will not exist because it exists under 2222
                var results = searcher.CreateQuery().Id(2112).Execute();
                Assert.AreEqual(0, results.Count());

                //now mimic moving 2112 to 1116
                //node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
                node.SetAttributeValue("path", "-1,1116,2112");
                node.SetAttributeValue("parentID", "1116");

                //now reindex the node, this should first delete it and then WILL add it because of the parent id constraint
                indexer.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Media) });

                //now ensure it exists
                results = searcher.CreateQuery().Id(2112).Execute();
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

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer1 = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, validator: validator))
            using (indexer1.ProcessNonAsync())
            {
                var searcher = indexer1.GetSearcher();

                //get a node from the data repo (this one exists underneath 2222)
                var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                                    .Root.Elements()
                                    .First(x => (int) x.Attribute("id") == 2112);

                var currPath = (string)node.Attribute("path"); //should be : -1,1111,2222,2112
                Assert.AreEqual("-1,1111,2222,2112", currPath);

                //ensure it's indexed
                indexer1.IndexItem(node.ConvertToValueSet(IndexTypes.Media));

                //it will exist because it exists under 2222
                var results = searcher.CreateQuery().Id(2112).Execute();
                Assert.AreEqual(1, results.Count());

                //now mimic moving the node underneath 1116 instead of 2222
                node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
                node.SetAttributeValue("parentID", "1116");

                //now reindex the node, this should first delete it and then NOT add it because of the parent id constraint
                indexer1.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Media) });

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
            var rebuilder = IndexInitializer.GetContentIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockContentService(), ScopeProvider, false);
            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(false)))
            using (indexer.ProcessNonAsync())
            {

                var searcher = indexer.GetSearcher();

                //create the whole thing
                rebuilder.Populate(indexer);

                var result = searcher.CreateQuery().Field(LuceneIndex.CategoryFieldName, IndexTypes.Content).Execute();
                Assert.AreEqual(21, result.TotalItemCount);

                //delete all content
                foreach (var r in result)
                {
                    indexer.DeleteFromIndex(r.Id);
                }


                //ensure it's all gone
                result = searcher.CreateQuery().Field(LuceneIndex.CategoryFieldName, IndexTypes.Content).Execute();
                Assert.AreEqual(0, result.TotalItemCount);

                //call our indexing methods
                rebuilder.Populate(indexer);

                result = searcher.CreateQuery().Field(LuceneIndex.CategoryFieldName, IndexTypes.Content).Execute();
                Assert.AreEqual(21, result.TotalItemCount);
            }
        }

        /// <summary>
        /// This will delete an item from the index and ensure that all children of the node are deleted too!
        /// </summary>
        [Test]
        public void Index_Delete_Index_Item_Ensure_Heirarchy_Removed()
        {

            var rebuilder = IndexInitializer.GetContentIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockContentService(), ScopeProvider, false);

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir))
            using (indexer.ProcessNonAsync())
            {
                var searcher = indexer.GetSearcher();

                //create the whole thing
                rebuilder.Populate(indexer);

                //now delete a node that has children

                indexer.DeleteFromIndex(1140.ToString());
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
