using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering methods in the TagService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class TagServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
	        base.Initialize();
        }
		
		[TearDown]
		public override void TearDown()
		{   
      		base.TearDown();
		}

        [Test]
        public void TagList_Contains_NodeCount()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", DataTypeDatabaseType.Ntext, "tags")
                {
                    DataTypeDefinitionId = 1041
                });
            contentTypeService.Save(contentType);

            var content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetTags("tags", new[] { "cow", "pig", "goat" }, true);
            contentService.Publish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetTags("tags", new[] { "cow", "pig" }, true);
            contentService.Publish(content2);

            var content3 = MockedContent.CreateSimpleContent(contentType, "Tagged content 3", -1);
            content3.SetTags("tags", new[] { "cow" }, true);
            contentService.Publish(content3);

            // Act            
            var tags = tagService.GetAllContentTags()
                .OrderByDescending(x => x.NodeCount)
                .ToList();

            // Assert
            Assert.AreEqual(3, tags.Count());
            Assert.AreEqual("cow", tags[0].Text);
            Assert.AreEqual(3, tags[0].NodeCount);
            Assert.AreEqual("pig", tags[1].Text);
            Assert.AreEqual(2, tags[1].NodeCount);
            Assert.AreEqual("goat", tags[2].Text);
            Assert.AreEqual(1, tags[2].NodeCount);
        }
    }
}