using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Services
{
    /// <summary>
    /// Tests covering methods in the TagService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class TagServiceTests : UmbracoIntegrationTest
    {
        public PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

        [Test]
        public void TagApiConsistencyTest()
        {
            var contentService = GetRequiredService<IContentService>();
            var contentTypeService = GetRequiredService<IContentTypeService>();
            var tagService = GetRequiredService<ITagService>();
            var dataTypeService = GetRequiredService<IDataTypeService>();
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            IContent content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, dataTypeService, "tags", new[] { "cow", "pig", "goat" });
            contentService.SaveAndPublish(content1);

            // change
            content1.AssignTags(PropertyEditorCollection, dataTypeService, "tags", new[] { "elephant" }, true);
            content1.RemoveTags(PropertyEditorCollection, dataTypeService, "tags", new[] { "cow" });
            contentService.SaveAndPublish(content1);

            // more changes
            content1.AssignTags(PropertyEditorCollection, dataTypeService, "tags", new[] { "mouse" }, true);
            contentService.SaveAndPublish(content1);
            content1.RemoveTags(PropertyEditorCollection, dataTypeService, "tags", new[] { "mouse" });
            contentService.SaveAndPublish(content1);

            // get it back
            content1 = contentService.GetById(content1.Id);
            var tagsValue = content1.GetValue("tags").ToString();
            var tagsValues = JsonConvert.DeserializeObject<string[]>(tagsValue);
            Assert.AreEqual(3, tagsValues.Length);
            Assert.Contains("pig", tagsValues);
            Assert.Contains("goat", tagsValues);
            Assert.Contains("elephant", tagsValues);

            var tags = tagService.GetTagsForProperty(content1.Id, "tags").ToArray();
            Assert.IsTrue(tags.All(x => x.Group == "default"));
            tagsValues = tags.Select(x => x.Text).ToArray();

            Assert.AreEqual(3, tagsValues.Length);
            Assert.Contains("pig", tagsValues);
            Assert.Contains("goat", tagsValues);
            Assert.Contains("elephant", tagsValues);
        }

        [Test]
        public void TagList_Contains_NodeCount()
        {
            var contentService = GetRequiredService<IContentService>();
            var contentTypeService = GetRequiredService<IContentTypeService>();
            var tagService = GetRequiredService<ITagService>();
            var dataTypeService = GetRequiredService<IDataTypeService>();
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.Tags, ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = Constants.DataTypes.Tags
                });
            contentTypeService.Save(contentType);

            var content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, dataTypeService, "tags", new[] { "cow", "pig", "goat" });
            contentService.SaveAndPublish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.AssignTags(PropertyEditorCollection, dataTypeService, "tags", new[] { "cow", "pig" });
            contentService.SaveAndPublish(content2);

            var content3 = MockedContent.CreateSimpleContent(contentType, "Tagged content 3", -1);
            content3.AssignTags(PropertyEditorCollection, dataTypeService, "tags", new[] { "cow" });
            contentService.SaveAndPublish(content3);

            // Act
            var tags = tagService.GetAllContentTags()
                .OrderByDescending(x => x.NodeCount)
                .ToList();

            // Assert
            Assert.AreEqual(3, tags.Count);
            Assert.AreEqual("cow", tags[0].Text);
            Assert.AreEqual(3, tags[0].NodeCount);
            Assert.AreEqual("pig", tags[1].Text);
            Assert.AreEqual(2, tags[1].NodeCount);
            Assert.AreEqual("goat", tags[2].Text);
            Assert.AreEqual(1, tags[2].NodeCount);
        }
    }
}
