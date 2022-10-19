using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Examine;
using Lucene.Net.Util;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

/// <summary>
///     Tests the standard indexing capabilities
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
public class IndexTest : ExamineBaseTest
{
    [Test]
    public void GivenValidationParentNode_WhenContentIndexedUnderDifferentParent_DocumentIsNotIndexed()
    {
        using (GetSynchronousContentIndex(false, out var index, out _, out _, 999))
        {
            var searcher = index.Searcher;

            var contentService = new ExamineDemoDataContentService();
            //get a node from the data repo
            var node = contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            var valueSet = node.ConvertToValueSet(IndexTypes.Content);

            // Ignored since the path isn't under 999
            index.IndexItems(new[] { valueSet });
            Assert.AreEqual(0, searcher.CreateQuery().Id(valueSet.Id).Execute().TotalItemCount);

            // Change so that it's under 999 and verify
            var values = valueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());
            values["path"] = new List<object> { "-1,999," + valueSet.Id };
            var newValueSet = new ValueSet(valueSet.Id, valueSet.Category, valueSet.ItemType, values.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
            index.IndexItems(new[] { newValueSet });
            Assert.AreEqual(1, searcher.CreateQuery().Id(valueSet.Id).Execute().TotalItemCount);
        }
    }

    [Test]
    public void GivenIndexingDocument_WhenRichTextPropertyData_CanStoreImmenseFields()
    {
        using (GetSynchronousContentIndex(false, out var index, out _, out var contentValueSetBuilder))
        {
            index.CreateIndex();

            var contentType = ContentTypeBuilder.CreateBasicContentType();
            contentType.AddPropertyType(new PropertyType(TestHelper.ShortStringHelper, "test", ValueStorageType.Ntext)
            {
                Alias = "rte",
                Name = "RichText",
                PropertyEditorAlias = Constants.PropertyEditors.Aliases.TinyMce
            });

            var content = ContentBuilder.CreateBasicContent(contentType);
            content.Id = 555;
            content.Path = "-1,555";

            var luceneStringFieldMaxLength = ByteBlockPool.BYTE_BLOCK_SIZE - 2;
            var faker = new Faker();
            var immenseText = faker.Random.String(luceneStringFieldMaxLength + 10);

            content.Properties["rte"].SetValue(immenseText);

            var valueSet = contentValueSetBuilder.GetValueSets(content);
            index.IndexItems(valueSet);

            var results = index.Searcher.CreateQuery().Id(555).Execute();
            var result = results.First();

            var key = $"{UmbracoExamineFieldNames.RawFieldPrefix}rte";
            Assert.IsTrue(result.Values.ContainsKey(key));
            Assert.Greater(result.Values[key].Length, luceneStringFieldMaxLength);
        }
    }

    [Test]
    public void GivenIndexingDocument_WhenGridPropertyData_ThenDataIndexedInSegregatedFields()
    {
        using (GetSynchronousContentIndex(false, out var index, out _, out var contentValueSetBuilder))
        {
            index.CreateIndex();

            var contentType = ContentTypeBuilder.CreateBasicContentType();
            contentType.AddPropertyType(new PropertyType(TestHelper.ShortStringHelper, "test", ValueStorageType.Ntext)
            {
                Alias = "grid",
                Name = "Grid",
                PropertyEditorAlias = Constants.PropertyEditors.Aliases.Grid
            });
            var content = ContentBuilder.CreateBasicContent(contentType);
            content.Id = 555;
            content.Path = "-1,555";
            var gridVal = new GridValue
            {
                Name = "n1",
                Sections = new List<GridValue.GridSection>
                {
                    new()
                    {
                        Grid = "g1",
                        Rows = new List<GridValue.GridRow>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Name = "row1",
                                Areas = new List<GridValue.GridArea>
                                {
                                    new()
                                    {
                                        Grid = "g2",
                                        Controls = new List<GridValue.GridControl>
                                        {
                                            new()
                                            {
                                                Editor = new GridValue.GridEditor
                                                {
                                                    Alias = "editor1",
                                                    View = "view1"
                                                },
                                                Value = "value1"
                                            },
                                            new()
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
    public void GivenEmptyIndex_WhenUsingWithContentAndMediaPopulators_ThenIndexPopulated()
    {
        var mediaRebuilder = IndexInitializer.GetMediaIndexRebuilder(IndexInitializer.GetMockMediaService());

        using (GetSynchronousContentIndex(false, out var index, out var contentRebuilder, out _))
        {
            //create the whole thing
            contentRebuilder.Populate(index);
            mediaRebuilder.Populate(index);

            var result = index.Searcher.CreateQuery().All().Execute();

            Assert.AreEqual(29, result.TotalItemCount);
        }
    }

    /// <summary>
    ///     Check that the node signalled as protected in the content service is not present in the index.
    /// </summary>
    [Test]
    public void GivenPublishedContentIndex_WhenProtectedContentIndexed_ThenItIsIgnored()
    {
        using (GetSynchronousContentIndex(true, out var index, out var contentRebuilder, out _))
        {
            //create the whole thing
            contentRebuilder.Populate(index);

            Assert.Greater(
                index.Searcher.CreateQuery().All().Execute().TotalItemCount,
                0);

            Assert.AreEqual(
                0,
                index.Searcher.CreateQuery().Id(ExamineDemoDataContentService.ProtectedNode.ToString()).Execute()
                    .TotalItemCount);
        }
    }

    [Test]
    public void GivenMediaUnderNonIndexableParent_WhenMediaMovedUnderIndexableParent_ThenItIsIncludedInTheIndex()
    {
        // create a validator with
        // publishedValuesOnly false
        // parentId 1116 (only content under that parent will be indexed)
        using (GetSynchronousContentIndex(false, out var index, out var contentRebuilder, out _, 1116))
        {
            //get a node from the data repo (this one exists underneath 2222)
            var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]").Root.Elements()
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
    public void GivenMediaUnderIndexableParent_WhenMediaMovedUnderNonIndexableParent_ThenItIsRemovedFromTheIndex()
    {
        // create a validator with
        // publishedValuesOnly false
        // parentId 2222 (only content under that parent will be indexed)
        using (GetSynchronousContentIndex(false, out var index, out var contentRebuilder, out _, 2222))
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
    ///     This will ensure that all 'Content' (not media) is cleared from the index using the Lucene API directly.
    ///     We then call the Examine method to re-index Content and do some comparisons to ensure that it worked correctly.
    /// </summary>
    [Test]
    public void GivenEmptyIndex_WhenIndexedWithContentPopulator_ThenTheIndexIsPopulated()
    {
        using (GetSynchronousContentIndex(false, out var index, out var contentRebuilder, out _))
        {
            //create the whole thing
            contentRebuilder.Populate(index);

            var result = index.Searcher
                .CreateQuery()
                .Field(ExamineFieldNames.CategoryFieldName, IndexTypes.Content)
                .Execute();
            Assert.AreEqual(21, result.TotalItemCount);

            //delete all content
            index.DeleteFromIndex(result.Select(x => x.Id));

            //ensure it's all gone
            result = index.Searcher.CreateQuery().Field(ExamineFieldNames.CategoryFieldName, IndexTypes.Content)
                .Execute();
            Assert.AreEqual(0, result.TotalItemCount);

            //call our indexing methods
            contentRebuilder.Populate(index);

            result = index.Searcher
                .CreateQuery()
                .Field(ExamineFieldNames.CategoryFieldName, IndexTypes.Content)
                .Execute();

            Assert.AreEqual(21, result.TotalItemCount);
        }
    }

    /// <summary>
    ///     This will delete an item from the index and ensure that all children of the node are deleted too!
    /// </summary>
    [Test]
    public void GivenPopulatedIndex_WhenDocumentDeleted_ThenItsHierarchyIsAlsoDeleted()
    {
        using (GetSynchronousContentIndex(false, out var index, out var contentRebuilder, out _))
        {
            var searcher = index.Searcher;

            //create the whole thing
            contentRebuilder.Populate(index);

            var results = searcher.CreateQuery().Id(1141).Execute();
            Assert.AreEqual(1, results.Count());

            //now delete a node that has children

            index.DeleteFromIndex(1140.ToString());
            //this node had children: 1141 & 1142, let's ensure they are also removed

            results = searcher.CreateQuery().Id(1141).Execute();
            Assert.AreEqual(0, results.Count());

            results = searcher.CreateQuery().Id(1142).Execute();
            Assert.AreEqual(0, results.Count());
        }
    }

    private readonly ExamineDemoDataMediaService _mediaService = new();
}
