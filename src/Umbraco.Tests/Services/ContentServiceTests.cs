using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering all methods in the ContentService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class ContentServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            VersionableRepositoryBase<int, IContent>.ThrowOnWarning = true;
        }

        [TearDown]
        public override void TearDown()
        {
            VersionableRepositoryBase<int, IContent>.ThrowOnWarning = false;

            base.TearDown();
        }

        //TODO Add test to verify there is only ONE newest document/content in cmsDocument table after updating.
        //TODO Add test to delete specific version (with and without deleting prior versions) and versions by date.

        [Test]
        public void Create_Blueprint()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentTypeService.Save(contentType);

            var blueprint = MockedContent.CreateTextpageContent(contentType, "hello", -1);
            blueprint.SetValue("title", "blueprint 1");
            blueprint.SetValue("bodyText", "blueprint 2");
            blueprint.SetValue("keywords", "blueprint 3");
            blueprint.SetValue("description", "blueprint 4");

            contentService.SaveBlueprint(blueprint);

            var found = contentService.GetBlueprintsForContentTypes().ToArray();
            Assert.AreEqual(1, found.Length);
            
            //ensures it's not found by normal content
            var contentFound = contentService.GetById(found[0].Id);
            Assert.IsNull(contentFound);
        }

        [Test]
        public void Delete_Blueprint()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentTypeService.Save(contentType);

            var blueprint = MockedContent.CreateTextpageContent(contentType, "hello", -1);
            blueprint.SetValue("title", "blueprint 1");
            blueprint.SetValue("bodyText", "blueprint 2");
            blueprint.SetValue("keywords", "blueprint 3");
            blueprint.SetValue("description", "blueprint 4");

            contentService.SaveBlueprint(blueprint);

            contentService.DeleteBlueprint(blueprint);

            var found = contentService.GetBlueprintsForContentTypes().ToArray();
            Assert.AreEqual(0, found.Length);
        }

        [Test]
        public void Create_Content_From_Blueprint()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentTypeService.Save(contentType);

            var blueprint = MockedContent.CreateTextpageContent(contentType, "hello", -1);
            blueprint.SetValue("title", "blueprint 1");
            blueprint.SetValue("bodyText", "blueprint 2");
            blueprint.SetValue("keywords", "blueprint 3");
            blueprint.SetValue("description", "blueprint 4");

            contentService.SaveBlueprint(blueprint);

            var fromBlueprint = contentService.CreateContentFromBlueprint(blueprint, "hello world");
            contentService.Save(fromBlueprint);

            Assert.IsTrue(fromBlueprint.HasIdentity);
            Assert.AreEqual("blueprint 1", fromBlueprint.Properties["title"].Value);
            Assert.AreEqual("blueprint 2", fromBlueprint.Properties["bodyText"].Value);
            Assert.AreEqual("blueprint 3", fromBlueprint.Properties["keywords"].Value);
            Assert.AreEqual("blueprint 4", fromBlueprint.Properties["description"].Value);
        }

        [Test]
        public void Get_All_Blueprints()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            var ct1 = MockedContentTypes.CreateTextpageContentType("ct1");
            contentTypeService.Save(ct1);
            var ct2 = MockedContentTypes.CreateTextpageContentType("ct2");
            contentTypeService.Save(ct2);

            for (int i = 0; i < 10; i++)
            {
                var blueprint = MockedContent.CreateTextpageContent(i % 2 == 0 ? ct1 : ct2, "hello" + i, -1);
                contentService.SaveBlueprint(blueprint);
            }            

            var found = contentService.GetBlueprintsForContentTypes().ToArray();
            Assert.AreEqual(10, found.Length);

            found = contentService.GetBlueprintsForContentTypes(ct1.Id).ToArray();
            Assert.AreEqual(5, found.Length);

            found = contentService.GetBlueprintsForContentTypes(ct2.Id).ToArray();
            Assert.AreEqual(5, found.Length);
        }

        /// <summary>
        /// Ensures that we don't unpublish all nodes when a node is deleted that has an invalid path of -1
        /// Note: it is actually the MoveToRecycleBin happening on the initial deletion of a node through the UI
        /// that causes the issue.
        /// Regression test: http://issues.umbraco.org/issue/U4-9336
        /// </summary>
        [Test]
        public void Moving_Node_To_Recycle_Bin_With_Invalid_Path()
        {
            var contentService = ServiceContext.ContentService;
            var root = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 1);
            Assert.IsTrue(contentService.PublishWithStatus(root).Success);
            var content = contentService.CreateContentWithIdentity("Test", -1, "umbTextpage", 0);
            Assert.IsTrue(contentService.PublishWithStatus(content).Success);
            var hierarchy = CreateContentHierarchy().OrderBy(x => x.Level).ToArray();
            contentService.Save(hierarchy, 0);
            foreach (var c in hierarchy)
            {
                Assert.IsTrue(contentService.PublishWithStatus(c).Success);
            }

            //now make the data corrupted :/
            DatabaseContext.Database.Execute("UPDATE umbracoNode SET path = '-1' WHERE id = @id", new {id = content.Id});

            //re-get with the corrupt path
            content = contentService.GetById(content.Id);

            // here we get all descendants by the path of the node being moved to bin, and unpublish all of them.
            // since the path is invalid, there's logic in here to fix that if it's possible and re-persist the entity.
            var moveResult = ServiceContext.ContentService.WithResult().MoveToRecycleBin(content);

            Assert.IsTrue(moveResult.Success);

            //re-get with the fixed/moved path
            content = contentService.GetById(content.Id);

            Assert.AreEqual("-1,-20," + content.Id, content.Path);

            //re-get
            hierarchy = contentService.GetByIds(hierarchy.Select(x => x.Id).ToArray()).OrderBy(x => x.Level).ToArray();

            Assert.That(hierarchy.All(c => c.Trashed == false), Is.True);
            Assert.That(hierarchy.All(c => c.Path.StartsWith("-1,-20") == false), Is.True);
        }

        [Test]
        public void Remove_Scheduled_Publishing_Date()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.CreateContentWithIdentity("Test", -1, "umbTextpage", 0);

            content.ReleaseDate = DateTime.Now.AddHours(2);
            contentService.Save(content, 0);

            content = contentService.GetById(content.Id);
            content.ReleaseDate = null;
            contentService.Save(content, 0);


            // Assert
            Assert.IsTrue(contentService.PublishWithStatus(content).Success);
        }

        [Test]
        public void Get_Top_Version_Ids()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.CreateContentWithIdentity("Test", -1, "umbTextpage", 0);
            for (int i = 0; i < 20; i++)
            {
                content.SetValue("bodyText", "hello world " + Guid.NewGuid());
                contentService.SaveAndPublishWithStatus(content);
            }


            // Assert
            var allVersions = contentService.GetVersionIds(content.Id, int.MaxValue);
            Assert.AreEqual(21, allVersions.Count());

            var topVersions = contentService.GetVersionIds(content.Id, 4);
            Assert.AreEqual(4, topVersions.Count());
        }

        [Test]
        public void Get_By_Ids_Sorted()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var results = new List<IContent>();
            for (int i = 0; i < 20; i++)
            {
                results.Add(contentService.CreateContentWithIdentity("Test", -1, "umbTextpage", 0));
            }

            var sortedGet = contentService.GetByIds(new[] { results[10].Id, results[5].Id, results[12].Id }).ToArray();

            // Assert
            Assert.AreEqual(sortedGet[0].Id, results[10].Id);
            Assert.AreEqual(sortedGet[1].Id, results[5].Id);
            Assert.AreEqual(sortedGet[2].Id, results[12].Id);
        }

        [Test]
        public void Count_All()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            for (int i = 0; i < 20; i++)
            {
                contentService.CreateContentWithIdentity("Test", -1, "umbTextpage", 0);
            }

            // Assert
            Assert.AreEqual(24, contentService.Count());
        }

        [Test]
        public void Count_By_Content_Type()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbBlah", "test Doc Type");
            contentTypeService.Save(contentType);

            // Act
            for (int i = 0; i < 20; i++)
            {
                contentService.CreateContentWithIdentity("Test", -1, "umbBlah", 0);
            }

            // Assert
            Assert.AreEqual(20, contentService.Count(contentTypeAlias: "umbBlah"));
        }

        [Test]
        public void Count_Children()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbBlah", "test Doc Type");
            contentTypeService.Save(contentType);
            var parent = contentService.CreateContentWithIdentity("Test", -1, "umbBlah", 0);

            // Act
            for (int i = 0; i < 20; i++)
            {
                contentService.CreateContentWithIdentity("Test", parent, "umbBlah");
            }

            // Assert
            Assert.AreEqual(20, contentService.CountChildren(parent.Id));
        }

        [Test]
        public void Count_Descendants()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbBlah", "test Doc Type");
            contentTypeService.Save(contentType);
            var parent = contentService.CreateContentWithIdentity("Test", -1, "umbBlah", 0);

            // Act
            IContent current = parent;
            for (int i = 0; i < 20; i++)
            {
                current = contentService.CreateContentWithIdentity("Test", current, "umbBlah");
            }

            // Assert
            Assert.AreEqual(20, contentService.CountDescendants(parent.Id));
        }

        [Test]
        public void GetAncestors_Returns_Empty_List_When_Path_Is_Null()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var current = new Mock<IContent>();
            var res = contentService.GetAncestors(current.Object);

            // Assert
            Assert.IsEmpty(res);
        }

        [Test]
        public void Tags_For_Entity_Are_Not_Exposed_Via_Tag_Api_When_Content_Is_Recycled()
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
            content1.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content2);

            // Act
            contentService.MoveToRecycleBin(content1);

            // Assert

            //there should be no tags for this entity
            var tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(0, tags.Count());

            //these tags should still be returned since they still have actively published content assigned
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(4, allTags.Count());
        }

        [Test]
        public void All_Tags_Are_Not_Exposed_Via_Tag_Api_When_Content_Is_Recycled()
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
            content1.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content2);

            // Act
            contentService.MoveToRecycleBin(content1);
            contentService.MoveToRecycleBin(content2);

            // Assert

            //there should be no exposed content tags now that nothing is published.
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(0, allTags.Count());
        }

        [Test]
        public void All_Tags_Are_Not_Exposed_Via_Tag_Api_When_Content_Is_Un_Published()
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
            content1.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content2);

            // Act
            contentService.UnPublish(content1);
            contentService.UnPublish(content2);

            // Assert

            //there should be no exposed content tags now that nothing is published.
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(0, allTags.Count());
        }

        [Test]
        public void Tags_Are_Not_Exposed_Via_Tag_Api_When_Content_Is_Re_Published()
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
            content1.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content2);

            contentService.UnPublish(content1);
            contentService.UnPublish(content2);

            // Act
            contentService.Publish(content1);

            // Assert

            var tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(4, tags.Count());
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(4, allTags.Count());
        }

        [Test]
        public void Tags_Are_Not_Exposed_Via_Tag_Api_When_Content_Is_Restored()
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
            content1.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content2);

            contentService.MoveToRecycleBin(content1);
            contentService.MoveToRecycleBin(content2);

            // Act
            contentService.Move(content1, -1);
            contentService.Publish(content1);

            // Assert

            var tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(4, tags.Count());
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(4, allTags.Count());
        }

        [Test]
        public void Create_Tag_Data_Bulk_Publish_Operation()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var dataTypeService = ServiceContext.DataTypeService;
            //set the pre-values
            dataTypeService.SavePreValues(1041, new Dictionary<string, PreValue>
            {
                {"group", new PreValue("test")},
                {"storageType", new PreValue("Csv")}
            });
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", DataTypeDatabaseType.Ntext, "tags")
                {
                    DataTypeDefinitionId = 1041
                });
            contentTypeService.Save(contentType);
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => contentType.Id), 0, contentType.Alias) };

            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);
            content.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Save(content);

            var child1 = MockedContent.CreateSimpleContent(contentType, "child 1 content", content.Id);
            child1.SetTags("tags", new[] { "hello1", "world1", "some1" }, true);
            contentService.Save(child1);

            var child2 = MockedContent.CreateSimpleContent(contentType, "child 2 content", content.Id);
            child2.SetTags("tags", new[] { "hello2", "world2" }, true);
            contentService.Save(child2);

            // Act
            contentService.PublishWithChildrenWithStatus(content, includeUnpublished: true);

            // Assert
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;

            Assert.AreEqual(4, DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));

            Assert.AreEqual(3, DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = child1.Id, propTypeId = propertyTypeId }));

            Assert.AreEqual(2, DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = child2.Id, propTypeId = propertyTypeId }));
        }

        [Test]
        public void Does_Not_Create_Tag_Data_For_Non_Published_Version()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", DataTypeDatabaseType.Ntext, "tags")
                {
                    DataTypeDefinitionId = 1041
                });
            contentTypeService.Save(contentType);
            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);
            content.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content);

            // Act
            content.SetTags("tags", new[] { "another", "world" }, false);
            contentService.Save(content);

            // Assert

            //the value will have changed but the tags db table will not have
            Assert.AreEqual(5, content.Properties["tags"].Value.ToString().Split(',').Distinct().Count());
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            Assert.AreEqual(4, DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));
        }

        [Test]
        public void Can_Replace_Tag_Data_To_Published_Content()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", DataTypeDatabaseType.Ntext, "tags")
                {
                    DataTypeDefinitionId = 1041
                });
            contentTypeService.Save(contentType);

            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);


            // Act
            content.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content);

            // Assert
            Assert.AreEqual(4, content.Properties["tags"].Value.ToString().Split(',').Distinct().Count());
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            Assert.AreEqual(4, DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));
        }

        [Test]
        public void Can_Append_Tag_Data_To_Published_Content()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", DataTypeDatabaseType.Ntext, "tags")
                {
                    DataTypeDefinitionId = 1041
                });
            contentTypeService.Save(contentType);
            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);
            content.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.PublishWithStatus(content);

            // Act
            content.SetTags("tags", new[] { "another", "world" }, false);
            contentService.PublishWithStatus(content);

            // Assert
            Assert.AreEqual(5, content.Properties["tags"].Value.ToString().Split(',').Distinct().Count());
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            Assert.AreEqual(5, DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));
        }

        [Test]
        public void Can_Remove_Tag_Data_To_Published_Content()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", DataTypeDatabaseType.Ntext, "tags")
                {
                    DataTypeDefinitionId = 1041
                });
            contentTypeService.Save(contentType);
            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);
            content.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.PublishWithStatus(content);

            // Act
            content.RemoveTags("tags", new[] { "some", "world" });
            contentService.PublishWithStatus(content);

            // Assert
            Assert.AreEqual(2, content.Properties["tags"].Value.ToString().Split(',').Distinct().Count());
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            Assert.AreEqual(2, DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));
        }

        [Test]
        public void Can_Remove_Property_Type()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.CreateContent("Test", -1, "umbTextpage", 0);

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.HasIdentity, Is.False);
        }

        [Test]
        public void Can_Create_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.CreateContent("Test", -1, "umbTextpage", 0);

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.HasIdentity, Is.False);
        }

        [Test]
        public void Can_Create_Content_Without_Explicitly_Set_User()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.CreateContent("Test", -1, "umbTextpage");

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.HasIdentity, Is.False);
            Assert.That(content.CreatorId, Is.EqualTo(0));//Default to zero/administrator
        }

        [Test]
        public void Can_Save_New_Content_With_Explicit_User()
        {
            var user = new User()
            {
                Name = "Test",
                Email = "test@test.com",
                Username = "test",
                RawPasswordValue = "test"
            };
            ServiceContext.UserService.Save(user);
            var content = new Content("Test", -1, ServiceContext.ContentTypeService.GetContentType("umbTextpage"));

            // Act
            ServiceContext.ContentService.Save(content, (int)user.Id);

            // Assert
            Assert.That(content.CreatorId, Is.EqualTo(user.Id));
            Assert.That(content.WriterId, Is.EqualTo(user.Id));
        }

        [Test]
        public void Cannot_Create_Content_With_Non_Existing_ContentType_Alias()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act & Assert
            Assert.Throws<Exception>(() => contentService.CreateContent("Test", -1, "umbAliasDoesntExist"));
        }

        [Test]
        public void Cannot_Save_Content_With_Empty_Name()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = new Content(string.Empty, -1, ServiceContext.ContentTypeService.GetContentType("umbTextpage"));

            // Act & Assert
            Assert.Throws<ArgumentException>(() => contentService.Save(content));
        }

        [Test]
        public void Can_Get_Content_By_Id()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Id, Is.EqualTo(NodeDto.NodeIdSeed + 1));
        }

        [Test]
        public void Can_Get_Content_By_Guid_Key()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.GetById(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Id, Is.EqualTo(NodeDto.NodeIdSeed + 1));
        }

        [Test]
        public void Can_Get_Content_By_Level()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var contents = contentService.GetByLevel(2).ToList();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Can_Get_Children_Of_Content_Id()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var contents = contentService.GetChildren(NodeDto.NodeIdSeed + 1).ToList();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Can_Get_Descendents_Of_Contnet()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var hierarchy = CreateContentHierarchy();
            contentService.Save(hierarchy, 0);

            // Act
            var contents = contentService.GetDescendants(NodeDto.NodeIdSeed + 1).ToList();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(52));
        }

        [Test]
        public void Can_Get_All_Versions_Of_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var parent = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 1);
            ServiceContext.ContentService.Publish(parent);//Publishing root, so Text Page 2 can be updated.
            var subpage2 = contentService.GetById(NodeDto.NodeIdSeed + 3);

            subpage2.Name = "Text Page 2 Updated";
            subpage2.SetValue("author", "Jane Doe");
            contentService.SaveAndPublishWithStatus(subpage2, 0);//NOTE New versions are only added between publish-state-changed, so publishing to ensure addition version.

            subpage2.Name = "Text Page 2 Updated again";
            subpage2.SetValue("author", "Bob Hope");
            contentService.SaveAndPublishWithStatus(subpage2, 0);//NOTE New versions are only added between publish-state-changed, so publishing to ensure addition version.

            // Act
            var versions = contentService.GetVersions(NodeDto.NodeIdSeed + 3).ToList();

            // Assert
            Assert.That(versions.Any(), Is.True);
            Assert.That(versions.Count(), Is.GreaterThanOrEqualTo(2));

            //ensure each version contains the correct property values
            Assert.AreEqual("John Doe", versions[2].GetValue("author"));
            Assert.AreEqual("Jane Doe", versions[1].GetValue("author"));
            Assert.AreEqual("Bob Hope", versions[0].GetValue("author"));
        }

        [Test]
        public void Can_Get_Root_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var contents = contentService.GetRootContent().ToList();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_Get_Content_For_Expiration()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var root = contentService.GetById(NodeDto.NodeIdSeed + 1);
            contentService.SaveAndPublishWithStatus(root);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 3);
            content.ExpireDate = DateTime.Now.AddSeconds(1);
            contentService.SaveAndPublishWithStatus(content);

            // Act
            Thread.Sleep(new TimeSpan(0, 0, 0, 2));
            var contents = contentService.GetContentForExpiration().ToList();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_Get_Content_For_Release()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var contents = contentService.GetContentForRelease().ToList();

            // Assert
            Assert.That(DateTime.Now.AddMinutes(-5) <= DateTime.Now);
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_Get_Content_In_RecycleBin()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var contents = contentService.GetContentInRecycleBin().ToList();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_UnPublish_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);
            bool published = contentService.Publish(content, 0);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            using (var uow = provider.GetUnitOfWork())
            {
                Assert.IsTrue(uow.Database.Exists<ContentXmlDto>(content.Id));
            }

            // Act
            bool unpublished = contentService.UnPublish(content, 0);

            // Assert
            Assert.That(published, Is.True);
            Assert.That(unpublished, Is.True);
            Assert.That(content.Published, Is.False);

            using (var uow = provider.GetUnitOfWork())
            {
                Assert.IsFalse(uow.Database.Exists<ContentXmlDto>(content.Id));
            }
        }

        /// <summary>
        /// This test is ignored because the way children are handled when
        /// parent is unpublished is treated differently now then from when this test
        /// was written.
        /// The correct case is now that Root is UnPublished removing the children
        /// from cache, but still having them "Published" in the "background".
        /// Once the Parent is Published the Children should re-appear as published.
        /// </summary>
        [Test, NUnit.Framework.Ignore]
        public void Can_UnPublish_Root_Content_And_Verify_Children_Is_UnPublished()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var published = contentService.RePublishAll(0);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);

            // Act
            bool unpublished = contentService.UnPublish(content, 0);
            var children = contentService.GetChildren(NodeDto.NodeIdSeed + 1).ToList();

            // Assert
            Assert.That(published, Is.True);//Verify that everything was published

            //Verify that content with Id (NodeDto.NodeIdSeed + 1) was unpublished
            Assert.That(unpublished, Is.True);
            Assert.That(content.Published, Is.False);

            //Verify that all children was unpublished
            Assert.That(children.Any(x => x.Published), Is.False);
            Assert.That(children.First(x => x.Id == NodeDto.NodeIdSeed + 2).Published, Is.False);//Released 5 mins ago, but should be unpublished
            Assert.That(children.First(x => x.Id == NodeDto.NodeIdSeed + 2).ReleaseDate.HasValue, Is.False);//Verify that the release date has been removed
            Assert.That(children.First(x => x.Id == NodeDto.NodeIdSeed + 3).Published, Is.False);//Expired 5 mins ago, so isn't be published
        }

        [Test]
        public void Can_RePublish_All_Content()
        {
            // Arrange
            var contentService = (ContentService)ServiceContext.ContentService;
            var rootContent = contentService.GetRootContent().ToList();
            foreach (var c in rootContent)
            {
                contentService.PublishWithChildren(c);
            }
            var allContent = rootContent.Concat(rootContent.SelectMany(x => x.Descendants()));
            //for testing we need to clear out the contentXml table so we can see if it worked
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            using (var uow = provider.GetUnitOfWork())
            {
                uow.Database.TruncateTable("cmsContentXml");
            }


            //for this test we are also going to save a revision for a content item that is not published, this is to ensure
            //that it's published version still makes it into the cmsContentXml table!
            contentService.Save(allContent.Last());

            // Act
            var published = contentService.RePublishAll(0);

            // Assert
            Assert.IsTrue(published);
            using (var uow = provider.GetUnitOfWork())
            {
                Assert.AreEqual(allContent.Count(), uow.Database.ExecuteScalar<int>("select count(*) from cmsContentXml"));
            }
        }

        [Test]
        public void Can_RePublish_All_Content_Of_Type()
        {
            // Arrange
            var contentService = (ContentService)ServiceContext.ContentService;
            var rootContent = contentService.GetRootContent().ToList();
            foreach (var c in rootContent)
            {
                contentService.PublishWithChildren(c);
            }
            var allContent = rootContent.Concat(rootContent.SelectMany(x => x.Descendants())).ToList();
            //for testing we need to clear out the contentXml table so we can see if it worked
            var provider = new PetaPocoUnitOfWorkProvider(Logger);

            using (var uow = provider.GetUnitOfWork())
            {
                uow.Database.TruncateTable("cmsContentXml");
            }
            //for this test we are also going to save a revision for a content item that is not published, this is to ensure
            //that it's published version still makes it into the cmsContentXml table!
            contentService.Save(allContent.Last());

            // Act
            contentService.RePublishAll(new int[] { allContent.Last().ContentTypeId });

            // Assert
            using (var uow = provider.GetUnitOfWork())
            {
                Assert.AreEqual(allContent.Count(), uow.Database.ExecuteScalar<int>("select count(*) from cmsContentXml"));
            }
        }

        [Test]
        public void Can_Publish_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);

            // Act
            bool published = contentService.Publish(content, 0);

            // Assert
            Assert.That(published, Is.True);
            Assert.That(content.Published, Is.True);
        }

        [Test]
        public void IsPublishable()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var parent = contentService.CreateContent("parent", -1, "umbTextpage");
            contentService.SaveAndPublishWithStatus(parent);
            var content = contentService.CreateContent("child", parent, "umbTextpage");
            contentService.Save(content);

            Assert.IsTrue(contentService.IsPublishable(content));
            contentService.UnPublish(parent);
            Assert.IsFalse(contentService.IsPublishable(content));
        }

        [Test]
        public void Can_Publish_Content_WithEvents()
        {
            ContentService.Publishing += ContentServiceOnPublishing;

            // tests that during 'publishing' event, what we get from the repo is the 'old' content,
            // because 'publishing' fires before the 'saved' event ie before the content is actually
            // saved

            try
            {
                var contentService = ServiceContext.ContentService;
                var content = contentService.GetById(NodeDto.NodeIdSeed + 1);
                Assert.AreEqual("Home", content.Name);

                content.Name = "foo";
                var published = contentService.Publish(content, 0);

                Assert.That(published, Is.True);
                Assert.That(content.Published, Is.True);

                var e = ServiceContext.ContentService.GetById(content.Id);
                Assert.AreEqual("foo", e.Name);
            }
            finally
            {
                ContentService.Publishing -= ContentServiceOnPublishing;
            }
        }

        private void ContentServiceOnPublishing(IPublishingStrategy sender, PublishEventArgs<IContent> args)
        {
            Assert.AreEqual(1, args.PublishedEntities.Count());
            var entity = args.PublishedEntities.First();
            Assert.AreEqual("foo", entity.Name);

            var e = ServiceContext.ContentService.GetById(entity.Id);
            Assert.AreEqual("Home", e.Name);
        }

        [Test]
        public void Can_Publish_Only_Valid_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentTypeService.Save(contentType);

            Content content = MockedContent.CreateSimpleContent(contentType, "Invalid Content", NodeDto.NodeIdSeed + 1);
            content.SetValue("author", string.Empty);
            contentService.Save(content, 0);

            // Act
            var parent = contentService.GetById(NodeDto.NodeIdSeed + 1);
            bool parentPublished = contentService.Publish(parent, 0);
            bool published = contentService.Publish(content, 0);

            // Assert
            Assert.That(parentPublished, Is.True);
            Assert.That(published, Is.False);
            Assert.That(content.IsValid(), Is.False);
            Assert.That(parent.Published, Is.True);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Can_Publish_Content_Children()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);

            // Act
            bool published = contentService.PublishWithChildren(content, 0);
            var children = contentService.GetChildren(NodeDto.NodeIdSeed + 1);

            // Assert
            Assert.That(published, Is.True);//Nothing was cancelled, so should be true
            Assert.That(content.Published, Is.True);//No restrictions, so should be published
            Assert.That(children.First(x => x.Id == NodeDto.NodeIdSeed + 2).Published, Is.True);//Released 5 mins ago, so should be published
        }

        [Test]
        public void Cannot_Publish_Expired_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 3); //This Content expired 5min ago
            content.ExpireDate = DateTime.Now.AddMinutes(-5);
            contentService.Save(content);

            var parent = contentService.GetById(NodeDto.NodeIdSeed + 1);
            bool parentPublished = contentService.Publish(parent, 0);//Publish root Home node to enable publishing of 'NodeDto.NodeIdSeed + 3'

            // Act
            bool published = contentService.Publish(content, 0);

            // Assert
            Assert.That(parentPublished, Is.True);
            Assert.That(published, Is.False);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Cannot_Publish_Content_Awaiting_Release()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 2);
            content.ReleaseDate = DateTime.Now.AddHours(2);
            contentService.Save(content, 0);

            var parent = contentService.GetById(NodeDto.NodeIdSeed + 1);
            bool parentPublished = contentService.Publish(parent, 0);//Publish root Home node to enable publishing of 'NodeDto.NodeIdSeed + 3'

            // Act
            bool published = contentService.Publish(content, 0);

            // Assert
            Assert.That(parentPublished, Is.True);
            Assert.That(published, Is.False);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Cannot_Publish_Content_Where_Parent_Is_Unpublished()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Subpage with Unpublisehed Parent", NodeDto.NodeIdSeed + 1, "umbTextpage", 0);
            contentService.Save(content, 0);

            // Act
            bool published = contentService.PublishWithChildren(content, 0);

            // Assert
            Assert.That(published, Is.False);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Cannot_Publish_Trashed_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);

            // Act
            bool published = contentService.Publish(content, 0);

            // Assert
            Assert.That(published, Is.False);
            Assert.That(content.Published, Is.False);
            Assert.That(content.Trashed, Is.True);
        }

        [Test]
        public void Can_Save_And_Publish_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            // Act
            bool published = contentService.SaveAndPublish(content, 0);

            // Assert
            Assert.That(content.HasIdentity, Is.True);
            Assert.That(content.Published, Is.True);
            Assert.That(published, Is.True);
        }

        /// <summary>
        /// Try to immitate a new child content item being created through the UI.
        /// This content item will have no Id, Path or Identity.
        /// It seems like this is wiped somewhere in the process when creating an item through the UI
        /// and we need to make sure we handle nullchecks for these properties when creating content.
        /// This is unfortunately not caught by the normal ContentService tests.
        /// </summary>
        [Test]
        public void Can_Save_And_Publish_Content_And_Child_Without_Identity()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            // Act
            var published = contentService.SaveAndPublish(content, 0);
            var childContent = contentService.CreateContent("Child", content.Id, "umbTextpage", 0);
            // Reset all identity properties
            childContent.Id = 0;
            childContent.Path = null;
            ((Content)childContent).ResetIdentity();
            var childPublished = contentService.SaveAndPublish(childContent, 0);

            // Assert
            Assert.That(content.HasIdentity, Is.True);
            Assert.That(content.Published, Is.True);
            Assert.That(childContent.HasIdentity, Is.True);
            Assert.That(childContent.Published, Is.True);
            Assert.That(published, Is.True);
            Assert.That(childPublished, Is.True);
        }

        [Test]
        public void Can_Get_Published_Descendant_Versions()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var root = contentService.GetById(NodeDto.NodeIdSeed + 1);
            var rootPublished = contentService.Publish(root);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 3);
            content.Properties["title"].Value = content.Properties["title"].Value + " Published";
            bool published = contentService.SaveAndPublish(content);

            var publishedVersion = content.Version;

            content.Properties["title"].Value = content.Properties["title"].Value + " Saved";
            contentService.Save(content);

            var savedVersion = content.Version;

            // Act
            var publishedDescendants = ((ContentService)contentService).GetPublishedDescendants(root).ToList();

            // Assert
            Assert.That(rootPublished, Is.True);
            Assert.That(published, Is.True);
            Assert.That(publishedDescendants.Any(x => x.Version == publishedVersion), Is.True);
            Assert.That(publishedDescendants.Any(x => x.Version == savedVersion), Is.False);

            //Ensure that the published content version has the correct property value and is marked as published
            var publishedContentVersion = publishedDescendants.First(x => x.Version == publishedVersion);
            Assert.That(publishedContentVersion.Published, Is.True);
            Assert.That(publishedContentVersion.Properties["title"].Value, Contains.Substring("Published"));

            //Ensure that the saved content version has the correct property value and is not marked as published
            var savedContentVersion = contentService.GetByVersion(savedVersion);
            Assert.That(savedContentVersion.Published, Is.False);
            Assert.That(savedContentVersion.Properties["title"].Value, Contains.Substring("Saved"));

            //Ensure that the latest version of the content is the saved and not-yet-published one
            var currentContent = contentService.GetById(NodeDto.NodeIdSeed + 3);
            Assert.That(currentContent.Published, Is.False);
            Assert.That(currentContent.Properties["title"].Value, Contains.Substring("Saved"));
            Assert.That(currentContent.Version, Is.EqualTo(savedVersion));
        }

        [Test]
        public void Can_Save_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            // Act
            contentService.Save(content, 0);

            // Assert
            Assert.That(content.HasIdentity, Is.True);
        }

        [Test]
        public void Can_Bulk_Save_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = contentTypeService.GetContentType("umbTextpage");
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Subpage 1", NodeDto.NodeIdSeed + 2);
            Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Subpage 2", NodeDto.NodeIdSeed + 2);
            var list = new List<IContent> { subpage, subpage2 };

            // Act
            contentService.Save(list, 0);

            // Assert
            Assert.That(list.Any(x => !x.HasIdentity), Is.False);
        }

        [Test]
        public void Can_Bulk_Save_New_Hierarchy_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var hierarchy = CreateContentHierarchy().ToList();

            // Act
            contentService.Save(hierarchy, 0);

            Assert.That(hierarchy.Any(), Is.True);
            Assert.That(hierarchy.Any(x => x.HasIdentity == false), Is.False);
            //all parent id's should be ok, they are lazy and if they equal zero an exception will be thrown
            Assert.DoesNotThrow(() => hierarchy.Any(x => x.ParentId != 0));

        }

        [Test]
        public void Can_Delete_Content_Of_Specific_ContentType()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = contentTypeService.GetContentType("umbTextpage");

            // Act
            contentService.DeleteContentOfType(contentType.Id);
            var rootContent = contentService.GetRootContent();
            var contents = contentService.GetContentOfContentType(contentType.Id);

            // Assert
            Assert.That(rootContent.Any(), Is.False);
            Assert.That(contents.Any(x => !x.Trashed), Is.False);
        }

        [Test]
        public void Can_Delete_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);

            // Act
            contentService.Delete(content, 0);
            var deleted = contentService.GetById(NodeDto.NodeIdSeed + 4);

            // Assert
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void Can_Move_Content_To_RecycleBin()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 3);

            // Act
            contentService.MoveToRecycleBin(content, 0);

            // Assert
            Assert.That(content.ParentId, Is.EqualTo(-20));
            Assert.That(content.Trashed, Is.True);
        }

        [Test]
        public void Can_Move_Content_Structure_To_RecycleBin_And_Empty_RecycleBin()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");
            Content subsubpage = MockedContent.CreateSimpleContent(contentType, "Text Page 3", NodeDto.NodeIdSeed + 2);
            contentService.Save(subsubpage, 0);

            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);

            // Act
            contentService.MoveToRecycleBin(content, 0);
            var descendants = contentService.GetDescendants(content).ToList();

            // Assert
            Assert.That(content.ParentId, Is.EqualTo(-20));
            Assert.That(content.Trashed, Is.True);
            Assert.That(descendants.Count, Is.EqualTo(3));
            Assert.That(descendants.Any(x => x.Path.Contains("-20") == false), Is.False);

            //Empty Recycle Bin
            contentService.EmptyRecycleBin();
            var trashed = contentService.GetContentInRecycleBin();

            Assert.That(trashed.Any(), Is.False);
        }

        [Test]
        public void Can_Empty_RecycleBin()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            contentService.EmptyRecycleBin();
            var contents = contentService.GetContentInRecycleBin();

            // Assert
            Assert.That(contents.Any(), Is.False);
        }

        [Test]
        public void Ensures_Permissions_Are_Retained_For_Copied_Descendants_With_Explicit_Permissions()
        {
            // Arrange
            var userGroup = MockedUserGroup.CreateUserGroup("1");
            ServiceContext.UserService.Save(userGroup);

            var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
            contentType.AllowedContentTypes = new List<ContentTypeSort>
            {
                new ContentTypeSort(new Lazy<int>(() => contentType.Id), 0, contentType.Alias)
            };
            ServiceContext.ContentTypeService.Save(contentType);

            var parentPage = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(parentPage);            

            var childPage = MockedContent.CreateSimpleContent(contentType, "child", parentPage);
            ServiceContext.ContentService.Save(childPage);
            //assign explicit permissions to the child
            ServiceContext.ContentService.AssignContentPermission(childPage, 'A', new[] { userGroup.Id });

            //Ok, now copy, what should happen is the childPage will retain it's own permissions
            var parentPage2 = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(parentPage2);

            var copy = ServiceContext.ContentService.Copy(childPage, parentPage2.Id, false, true);

            //get the permissions and verify
            var permissions = ServiceContext.UserService.GetPermissionsForPath(userGroup, copy.Path, fallbackToDefaultPermissions: true);
            var allPermissions = permissions.GetAllPermissions().ToArray();
            Assert.AreEqual(1, allPermissions.Length);
            Assert.AreEqual("A", allPermissions[0]);
        }

        [Test]
        public void Ensures_Permissions_Are_Inherited_For_Copied_Descendants()
        {
            // Arrange
            var userGroup = MockedUserGroup.CreateUserGroup("1");
            ServiceContext.UserService.Save(userGroup);

            var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
            contentType.AllowedContentTypes = new List<ContentTypeSort>
            {
                new ContentTypeSort(new Lazy<int>(() => contentType.Id), 0, contentType.Alias)
            };
            ServiceContext.ContentTypeService.Save(contentType);

            var parentPage = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(parentPage);
            ServiceContext.ContentService.AssignContentPermission(parentPage, 'A', new[] { userGroup.Id });

            var childPage1 = MockedContent.CreateSimpleContent(contentType, "child1", parentPage);
            ServiceContext.ContentService.Save(childPage1);
            var childPage2 = MockedContent.CreateSimpleContent(contentType, "child2", childPage1);
            ServiceContext.ContentService.Save(childPage2);
            var childPage3 = MockedContent.CreateSimpleContent(contentType, "child3", childPage2);
            ServiceContext.ContentService.Save(childPage3);

            //Verify that the children have the inherited permissions
            var descendants = ServiceContext.ContentService.GetDescendants(parentPage).ToArray();
            Assert.AreEqual(3, descendants.Length);

            foreach (var descendant in descendants)
            {
                var permissions = ServiceContext.UserService.GetPermissionsForPath(userGroup, descendant.Path, fallbackToDefaultPermissions: true);
                var allPermissions = permissions.GetAllPermissions().ToArray();
                Assert.AreEqual(1, allPermissions.Length);
                Assert.AreEqual("A", allPermissions[0]);
            }

            //create a new parent with a new permission structure
            var parentPage2 = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(parentPage2);
            ServiceContext.ContentService.AssignContentPermission(parentPage2, 'B', new[] { userGroup.Id });

            //Now copy, what should happen is the child pages will now have permissions inherited from the new parent
            var copy = ServiceContext.ContentService.Copy(childPage1, parentPage2.Id, false, true);
            
            descendants = ServiceContext.ContentService.GetDescendants(parentPage2).ToArray();
            Assert.AreEqual(3, descendants.Length);

            foreach (var descendant in descendants)
            {
                var permissions = ServiceContext.UserService.GetPermissionsForPath(userGroup, descendant.Path, fallbackToDefaultPermissions: true);
                var allPermissions = permissions.GetAllPermissions().ToArray();
                Assert.AreEqual(1, allPermissions.Length);
                Assert.AreEqual("B", allPermissions[0]);
            }
        }

        [Test]
        public void Can_Empty_RecycleBin_With_Content_That_Has_All_Related_Data()
        {
            // Arrange
            //need to:
            // * add relations
            // * add permissions
            // * add notifications
            // * public access
            // * tags
            // * domain
            // * published & preview data
            // * multiple versions

            var contentType = MockedContentTypes.CreateAllTypesContentType("test", "test");
            ServiceContext.ContentTypeService.Save(contentType, 0);

            object obj =
               new
               {
                   tags = "Hello,World"
               };
            var content1 = MockedContent.CreateBasicContent(contentType);
            content1.PropertyValues(obj);
            content1.ResetDirtyProperties(false);
            ServiceContext.ContentService.Save(content1, 0);
            Assert.IsTrue(ServiceContext.ContentService.PublishWithStatus(content1, 0).Success);
            var content2 = MockedContent.CreateBasicContent(contentType);
            content2.PropertyValues(obj);
            content2.ResetDirtyProperties(false);
            ServiceContext.ContentService.Save(content2, 0);
            Assert.IsTrue(ServiceContext.ContentService.PublishWithStatus(content2, 0).Success);

            var editorGroup = ServiceContext.UserService.GetUserGroupByAlias("editor");
            editorGroup.StartContentId = content1.Id;
            ServiceContext.UserService.Save(editorGroup);

            var admin = ServiceContext.UserService.GetUserById(0);
            admin.StartContentIds = new[] {content1.Id};
            ServiceContext.UserService.Save(admin);

            ServiceContext.RelationService.Save(new RelationType(Constants.ObjectTypes.DocumentGuid, Constants.ObjectTypes.DocumentGuid, "test"));
            Assert.IsNotNull(ServiceContext.RelationService.Relate(content1, content2, "test"));

            ServiceContext.PublicAccessService.Save(new PublicAccessEntry(content1, content2, content2, new List<PublicAccessRule>
            {
                new PublicAccessRule
                {
                    RuleType = "test",
                    RuleValue = "test"
                }
            }));
            Assert.IsTrue(ServiceContext.PublicAccessService.AddRule(content1, "test2", "test2").Success);

            var user = ServiceContext.UserService.GetUserById(0);
            var userGroup = ServiceContext.UserService.GetUserGroupByAlias(user.Groups.First().Alias);
            Assert.IsNotNull(ServiceContext.NotificationService.CreateNotification(user, content1, "test"));

            ServiceContext.ContentService.AssignContentPermission(content1, 'A', new[] { userGroup.Id});

            Assert.IsTrue(ServiceContext.DomainService.Save(new UmbracoDomain("www.test.com", "en-AU")
            {
                RootContentId = content1.Id
            }).Success);

            // Act
            ServiceContext.ContentService.MoveToRecycleBin(content1);
            ServiceContext.ContentService.EmptyRecycleBin();
            var contents = ServiceContext.ContentService.GetContentInRecycleBin();

            // Assert
            Assert.That(contents.Any(), Is.False);
        }

        [Test]
        public void Can_Move_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);

            // Act - moving out of recycle bin
            contentService.Move(content, NodeDto.NodeIdSeed + 1, 0);

            // Assert
            Assert.That(content.ParentId, Is.EqualTo(NodeDto.NodeIdSeed + 1));
            Assert.That(content.Trashed, Is.False);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Can_Copy_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var temp = contentService.GetById(NodeDto.NodeIdSeed + 3);

            // Act
            var copy = contentService.Copy(temp, temp.ParentId, false, 0);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 3);

            // Assert
            Assert.That(copy, Is.Not.Null);
            Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
            Assert.AreNotSame(content, copy);
            foreach (var property in copy.Properties)
            {
                Assert.AreNotEqual(property.Id, content.Properties[property.Alias].Id);
                Assert.AreEqual(property.Value, content.Properties[property.Alias].Value);
            }
            //Assert.AreNotEqual(content.Name, copy.Name);
        }

        [Test]
        public void Can_Copy_Recursive()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var temp = contentService.GetById(NodeDto.NodeIdSeed + 1);
            Assert.AreEqual("Home", temp.Name);
            Assert.AreEqual(2, temp.Children().Count());

            // Act
            var copy = contentService.Copy(temp, temp.ParentId, false, true, 0);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);

            // Assert
            Assert.That(copy, Is.Not.Null);
            Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
            Assert.AreNotSame(content, copy);
            Assert.AreEqual(2, copy.Children().Count());

            var child = contentService.GetById(NodeDto.NodeIdSeed + 2);
            var childCopy = copy.Children().First();
            Assert.AreEqual(childCopy.Name, child.Name);
            Assert.AreNotEqual(childCopy.Id, child.Id);
            Assert.AreNotEqual(childCopy.Key, child.Key);
        }

        [Test]
        public void Can_Copy_NonRecursive()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var temp = contentService.GetById(NodeDto.NodeIdSeed + 1);
            Assert.AreEqual("Home", temp.Name);
            Assert.AreEqual(2, temp.Children().Count());

            // Act
            var copy = contentService.Copy(temp, temp.ParentId, false, false, 0);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);

            // Assert
            Assert.That(copy, Is.Not.Null);
            Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
            Assert.AreNotSame(content, copy);
            Assert.AreEqual(0, copy.Children().Count());
        }

        [Test]
        public void Can_Copy_Content_With_Tags()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");
            var temp = MockedContent.CreateSimpleContent(contentType, "Simple Text Page", -1);
            var prop = temp.Properties.First();
            temp.SetTags(prop.Alias, new[] { "hello", "world" }, true);
            var status = contentService.PublishWithStatus(temp);

            // Act
            var copy = contentService.Copy(temp, temp.ParentId, false, 0);

            // Assert
            var copiedTags = ServiceContext.TagService.GetTagsForEntity(copy.Id).ToArray();
            Assert.AreEqual(2, copiedTags.Count());
            Assert.AreEqual("hello", copiedTags[0].Text);
            Assert.AreEqual("world", copiedTags[1].Text);
        }

        [Test, NUnit.Framework.Ignore]
        public void Can_Send_To_Publication()
        { }

        [Test]
        public void Can_Rollback_Version_On_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var parent = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 1);
            ServiceContext.ContentService.Publish(parent);//Publishing root, so Text Page 2 can be updated.
            var subpage2 = contentService.GetById(NodeDto.NodeIdSeed + 3);
            var version = subpage2.Version;
            var nameBeforeRollback = subpage2.Name;
            subpage2.Name = "Text Page 2 Updated";
            subpage2.SetValue("author", "Jane Doe");
            contentService.SaveAndPublishWithStatus(subpage2, 0);//Saving and publishing, so a new version is created

            // Act
            var rollback = contentService.Rollback(NodeDto.NodeIdSeed + 3, version, 0);

            // Assert
            Assert.That(rollback, Is.Not.Null);
            Assert.AreNotEqual(rollback.Version, subpage2.Version);
            Assert.That(rollback.GetValue<string>("author"), Is.Not.EqualTo("Jane Doe"));
            Assert.AreEqual(nameBeforeRollback, rollback.Name);
        }

        [Test]
        public void Can_Save_Lazy_Content()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Mock.Of<ILogger>());
            var unitOfWork = provider.GetUnitOfWork();
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");
            var root = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 1);

            var c = new Lazy<IContent>(() => MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Page", root.Id));
            var c2 = new Lazy<IContent>(() => MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Subpage", c.Value.Id));
            var list = new List<Lazy<IContent>> { c, c2 };

            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                foreach (var content in list)
                {
                    repository.AddOrUpdate(content.Value);
                    unitOfWork.Commit();
                }

                Assert.That(c.Value.HasIdentity, Is.True);
                Assert.That(c2.Value.HasIdentity, Is.True);

                Assert.That(c.Value.Id > 0, Is.True);
                Assert.That(c2.Value.Id > 0, Is.True);

                Assert.That(c.Value.ParentId > 0, Is.True);
                Assert.That(c2.Value.ParentId > 0, Is.True);
            }

        }

        [Test]
        public void Can_Verify_Content_Has_Published_Version()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 1);
            bool published = contentService.PublishWithChildren(content, 0);
            var homepage = contentService.GetById(NodeDto.NodeIdSeed + 1);
            homepage.Name = "Homepage";
            ServiceContext.ContentService.Save(homepage);

            // Act
            bool hasPublishedVersion = ServiceContext.ContentService.HasPublishedVersion(NodeDto.NodeIdSeed + 1);

            // Assert
            Assert.That(published, Is.True);
            Assert.That(homepage.Published, Is.False);
            Assert.That(hasPublishedVersion, Is.True);
        }

        [Test]
        public void Can_Verify_Property_Types_On_Content()
        {
            // Arrange
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateAllTypesContentType("allDataTypes", "All DataTypes");
            contentTypeService.Save(contentType);
            var contentService = ServiceContext.ContentService;
            var content = MockedContent.CreateAllTypesContent(contentType, "Random Content", -1);
            contentService.Save(content);
            var id = content.Id;

            // Act
            var sut = contentService.GetById(id);

            // Arrange
            Assert.That(sut.GetValue<bool>("isTrue"), Is.True);
            Assert.That(sut.GetValue<int>("number"), Is.EqualTo(42));
            Assert.That(sut.GetValue<string>("bodyText"), Is.EqualTo("Lorem Ipsum Body Text Test"));
            Assert.That(sut.GetValue<string>("singleLineText"), Is.EqualTo("Single Line Text Test"));
            Assert.That(sut.GetValue<string>("multilineText"), Is.EqualTo("Multiple lines \n in one box"));
            Assert.That(sut.GetValue<string>("upload"), Is.EqualTo("/media/1234/koala.jpg"));
            Assert.That(sut.GetValue<string>("label"), Is.EqualTo("Non-editable label"));
            //SD: This is failing because the 'content' call to GetValue<DateTime> always has empty milliseconds
            //MCH: I'm guessing this is an issue because of the format the date is actually stored as, right? Cause we don't do any formatting when saving or loading
            Assert.That(sut.GetValue<DateTime>("dateTime").ToString("G"), Is.EqualTo(content.GetValue<DateTime>("dateTime").ToString("G")));
            Assert.That(sut.GetValue<string>("colorPicker"), Is.EqualTo("black"));
            //that one is gone in 7.4
            //Assert.That(sut.GetValue<string>("folderBrowser"), Is.Null);
            Assert.That(sut.GetValue<string>("ddlMultiple"), Is.EqualTo("1234,1235"));
            Assert.That(sut.GetValue<string>("rbList"), Is.EqualTo("random"));
            Assert.That(sut.GetValue<DateTime>("date").ToString("G"), Is.EqualTo(content.GetValue<DateTime>("date").ToString("G")));
            Assert.That(sut.GetValue<string>("ddl"), Is.EqualTo("1234"));
            Assert.That(sut.GetValue<string>("chklist"), Is.EqualTo("randomc"));
            Assert.That(sut.GetValue<Udi>("contentPicker"), Is.EqualTo(Udi.Create(Constants.UdiEntityType.Document, new Guid("74ECA1D4-934E-436A-A7C7-36CC16D4095C"))));
            Assert.That(sut.GetValue<Udi>("mediaPicker"), Is.EqualTo(Udi.Create(Constants.UdiEntityType.Media, new Guid("44CB39C8-01E5-45EB-9CF8-E70AAF2D1691"))));
            Assert.That(sut.GetValue<Udi>("memberPicker"), Is.EqualTo(Udi.Create(Constants.UdiEntityType.Member, new Guid("9A50A448-59C0-4D42-8F93-4F1D55B0F47D"))));
            Assert.That(sut.GetValue<string>("relatedLinks"), Is.EqualTo("<links><link title=\"google\" link=\"http://google.com\" type=\"external\" newwindow=\"0\" /></links>"));
            Assert.That(sut.GetValue<string>("tags"), Is.EqualTo("this,is,tags"));
        }

        [Test]
        public void Can_Delete_Previous_Versions_Not_Latest()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);
            var version = content.Version;

            // Act
            contentService.DeleteVersion(NodeDto.NodeIdSeed + 4, version, true, 0);
            var sut = contentService.GetById(NodeDto.NodeIdSeed + 4);

            // Assert
            Assert.That(sut.Version, Is.EqualTo(version));
        }

        [Test]
        public void Ensure_Content_Xml_Created()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            contentService.Save(content);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);

            using (var uow = provider.GetUnitOfWork())
            {
                Assert.IsFalse(uow.Database.Exists<ContentXmlDto>(content.Id));
            }

            contentService.Publish(content);

            using (var uow = provider.GetUnitOfWork())
            {
                Assert.IsTrue(uow.Database.Exists<ContentXmlDto>(content.Id));
            }
        }

        [Test]
        public void Ensure_Preview_Xml_Created()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            contentService.Save(content);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);

            using (var uow = provider.GetUnitOfWork())
            {
                Assert.IsTrue(uow.Database.SingleOrDefault<PreviewXmlDto>("WHERE nodeId=@nodeId AND versionId = @versionId", new { nodeId = content.Id, versionId = content.Version }) != null);
            }
        }

        [Test]
        public void Created_HasPublishedVersion_Not()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            contentService.Save(content);

            Assert.IsTrue(content.HasIdentity);
            Assert.IsFalse(content.HasPublishedVersion);

            content = contentService.GetById(content.Id);
            Assert.IsFalse(content.HasPublishedVersion);
        }

        [Test]
        public void Published_HasPublishedVersion_Self()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            contentService.SaveAndPublishWithStatus(content);

            Assert.IsTrue(content.HasIdentity);
            Assert.IsTrue(content.HasPublishedVersion);
            Assert.AreEqual(content.PublishedVersionGuid, content.Version);

            content = contentService.GetById(content.Id);
            Assert.IsTrue(content.HasPublishedVersion);
            Assert.AreEqual(content.PublishedVersionGuid, content.Version);
        }

        [Test]
        public void PublishedWithChanges_HasPublishedVersion_Other()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            contentService.SaveAndPublishWithStatus(content);
            content.SetValue("author", "James Dean");
            contentService.Save(content);

            Assert.IsTrue(content.HasIdentity);
            Assert.IsTrue(content.HasPublishedVersion);
            Assert.AreNotEqual(content.PublishedVersionGuid, content.Version);

            content = contentService.GetById(content.Id);
            Assert.IsFalse(content.Published);
            Assert.IsTrue(content.HasPublishedVersion);
            Assert.AreNotEqual(content.PublishedVersionGuid, content.Version);

            var published = contentService.GetPublishedVersion(content);
            Assert.IsTrue(published.Published);
            Assert.IsTrue(published.HasPublishedVersion);
            Assert.AreEqual(published.PublishedVersionGuid, published.Version);
        }

        [Test]
        public void Unpublished_HasPublishedVersion_Not()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            contentService.SaveAndPublishWithStatus(content);
            contentService.UnPublish(content);

            Assert.IsTrue(content.HasIdentity);
            Assert.IsFalse(content.HasPublishedVersion);

            content = contentService.GetById(content.Id);
            Assert.IsFalse(content.HasPublishedVersion);
        }

        [Test]
        public void HasPublishedVersion_Method()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", -1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            contentService.Save(content);
            Assert.IsTrue(content.HasIdentity);
            Assert.IsFalse(content.HasPublishedVersion);
            Assert.IsFalse(content.HasPublishedVersion());

            contentService.SaveAndPublishWithStatus(content);
            Assert.IsTrue(content.HasPublishedVersion);
            Assert.IsTrue(content.HasPublishedVersion());
        }

        [Test]
        public void Can_Get_Paged_Children()
        {
            var service = ServiceContext.ContentService;
            // Start by cleaning the "db"
            var umbTextPage = service.GetById(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
            service.Delete(umbTextPage);

            var contentType = MockedContentTypes.CreateSimpleContentType();
            ServiceContext.ContentTypeService.Save(contentType);
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType);
                ServiceContext.ContentService.Save(c1);
            }

            long total;
            var entities = service.GetPagedChildren(-1, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedChildren(-1, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
        }

        [Test]
        public void Can_Get_Paged_Children_Dont_Get_Descendants()
        {
            var service = ServiceContext.ContentService;
            // Start by cleaning the "db"
            var umbTextPage = service.GetById(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
            service.Delete(umbTextPage);

            var contentType = MockedContentTypes.CreateSimpleContentType();
            ServiceContext.ContentTypeService.Save(contentType);
            // only add 9 as we also add a content with children
            for (int i = 0; i < 9; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType);
                ServiceContext.ContentService.Save(c1);
            }

            var willHaveChildren = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(willHaveChildren);
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, "Content" + i, willHaveChildren.Id);
                ServiceContext.ContentService.Save(c1);
            }

            long total;
            // children in root including the folder - not the descendants in the folder
            var entities = service.GetPagedChildren(-1, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedChildren(-1, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));

            // children in folder
            entities = service.GetPagedChildren(willHaveChildren.Id, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedChildren(willHaveChildren.Id, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
        }

        private IEnumerable<IContent> CreateContentHierarchy()
        {
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");
            var root = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 1);

            var list = new List<IContent>();

            for (int i = 0; i < 10; i++)
            {
                var content = MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Page " + i, root);

                list.Add(content);
                list.AddRange(CreateChildrenOf(contentType, content, 4));

                Debug.Print("Created: 'Hierarchy Simple Text Page {0}'", i);
            }

            return list;
        }

        private IEnumerable<IContent> CreateChildrenOf(IContentType contentType, IContent content, int depth)
        {
            var list = new List<IContent>();
            for (int i = 0; i < depth; i++)
            {
                var c = MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Subpage " + i, content);
                list.Add(c);

                Debug.Print("Created: 'Hierarchy Simple Text Subpage {0}' - Depth: {1}", i, depth);
            }
            return list;
        }

        private ContentRepository CreateRepository(IScopeUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository)
        {
            var templateRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var tagRepository = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, templateRepository);
            var repository = new ContentRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, contentTypeRepository, templateRepository, tagRepository, Mock.Of<IContentSection>());
            return repository;
        }
    }
}