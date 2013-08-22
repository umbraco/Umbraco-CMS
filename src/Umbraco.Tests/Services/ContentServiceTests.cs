using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
	/// <summary>
    /// Tests covering all methods in the ContentService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [TestFixture, RequiresSTA]
    public class ContentServiceTests : BaseServiceTest
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

        //TODO Add test to verify there is only ONE newest document/content in cmsDocument table after updating.
        //TODO Add test to delete specific version (with and without deleting prior versions) and versions by date.

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
            var contentService = ServiceContext.ContentService as ContentService;

            // Act
            var content = contentService.CreateContent("Test", -1, "umbTextpage");

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.HasIdentity, Is.False);
            Assert.That(content.CreatorId, Is.EqualTo(0));//Default to zero/administrator
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
        public void Can_Get_Content_By_Id()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.GetById(1046);

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Id, Is.EqualTo(1046));
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
            Assert.That(content.Id, Is.EqualTo(1046));
        }

        [Test]
        public void Can_Get_Content_By_Level()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var contents = contentService.GetByLevel(2);

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
            var contents = contentService.GetChildren(1046);

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
            var contents = contentService.GetDescendants(1046);

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
            var parent = ServiceContext.ContentService.GetById(1046);
            ServiceContext.ContentService.Publish(parent);//Publishing root, so Text Page 2 can be updated.
            var subpage2 = contentService.GetById(1048);
            subpage2.Name = "Text Page 2 Updated";
            subpage2.SetValue("author", "Jane Doe");
            contentService.SaveAndPublish(subpage2, 0);//NOTE New versions are only added between publish-state-changed, so publishing to ensure addition version.

            // Act
            var versions = contentService.GetVersions(1048);

            // Assert
            Assert.That(versions.Any(), Is.True);
            Assert.That(versions.Count(), Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Can_Get_Root_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var contents = contentService.GetRootContent();

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
            var root = contentService.GetById(1046);
            contentService.SaveAndPublish(root);
            var content = contentService.GetById(1048);
            content.ExpireDate = DateTime.Now.AddSeconds(1);
            contentService.SaveAndPublish(content);

            // Act
            Thread.Sleep(new TimeSpan(0, 0, 0, 2));
            var contents = contentService.GetContentForExpiration();

            // Assert
            Assert.That(DateTime.Now.AddMinutes(-5) <= DateTime.Now);
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
            var contents = contentService.GetContentForRelease();

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
            var contents = contentService.GetContentInRecycleBin();

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
            var content = contentService.GetById(1046);
            bool published = contentService.Publish(content, 0);

            // Act
            bool unpublished = contentService.UnPublish(content, 0);

            // Assert
            Assert.That(published, Is.True);
            Assert.That(unpublished, Is.True);
            Assert.That(content.Published, Is.False);
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
            var content = contentService.GetById(1046);

            // Act
            bool unpublished = contentService.UnPublish(content, 0);
            var children = contentService.GetChildren(1046);

            // Assert
            Assert.That(published, Is.True);//Verify that everything was published
            
            //Verify that content with Id 1046 was unpublished
            Assert.That(unpublished, Is.True);
            Assert.That(content.Published, Is.False);

            //Verify that all children was unpublished
            Assert.That(children.Any(x => x.Published), Is.False);
            Assert.That(children.First(x => x.Id == 1047).Published, Is.False);//Released 5 mins ago, but should be unpublished
            Assert.That(children.First(x => x.Id == 1047).ReleaseDate.HasValue, Is.False);//Verify that the release date has been removed
            Assert.That(children.First(x => x.Id == 1048).Published, Is.False);//Expired 5 mins ago, so isn't be published
        }

        [Test]
        public void Can_RePublish_All_Content()
        {
            // Arrange
            var contentService = (ContentService)ServiceContext.ContentService;
            var rootContent = contentService.GetRootContent();
            foreach (var c in rootContent)
            {
                contentService.PublishWithChildren(c);
            }
            var allContent = rootContent.Concat(rootContent.SelectMany(x => x.Descendants()));
            //for testing we need to clear out the contentXml table so we can see if it worked
            var provider = new PetaPocoUnitOfWorkProvider();
            var uow =  provider.GetUnitOfWork();
            using (RepositoryResolver.Current.ResolveByType<IContentRepository>(uow))
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
            uow = provider.GetUnitOfWork();
            using (var repo = RepositoryResolver.Current.ResolveByType<IContentRepository>(uow))
            {
                Assert.AreEqual(allContent.Count(), uow.Database.ExecuteScalar<int>("select count(*) from cmsContentXml"));
            }
        }

        [Test]
        public void Can_RePublish_All_Content_Of_Type()
        {
            // Arrange
            var contentService = (ContentService)ServiceContext.ContentService;
            var rootContent = contentService.GetRootContent();
            foreach (var c in rootContent)
            {
                contentService.PublishWithChildren(c);
            }
            var allContent = rootContent.Concat(rootContent.SelectMany(x => x.Descendants()));
            //for testing we need to clear out the contentXml table so we can see if it worked
            var provider = new PetaPocoUnitOfWorkProvider();
            var uow = provider.GetUnitOfWork();
            using (RepositoryResolver.Current.ResolveByType<IContentRepository>(uow))
            {
                uow.Database.TruncateTable("cmsContentXml");
            }
            //for this test we are also going to save a revision for a content item that is not published, this is to ensure
            //that it's published version still makes it into the cmsContentXml table!
            contentService.Save(allContent.Last());


            // Act
            contentService.RePublishAll(new int[]{allContent.Last().ContentTypeId});

            // Assert            
            uow = provider.GetUnitOfWork();
            using (var repo = RepositoryResolver.Current.ResolveByType<IContentRepository>(uow))
            {
                Assert.AreEqual(allContent.Count(), uow.Database.ExecuteScalar<int>("select count(*) from cmsContentXml"));
            }
        }

        [Test]
        public void Can_Publish_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(1046);

            // Act
            bool published = contentService.Publish(content, 0);

            // Assert
            Assert.That(published, Is.True);
            Assert.That(content.Published, Is.True);
        }

        [Test]
        public void Can_Publish_Only_Valid_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentTypeService.Save(contentType);

            Content content = MockedContent.CreateSimpleContent(contentType, "Invalid Content", 1046);
            content.SetValue("author", string.Empty);
            contentService.Save(content, 0);

            // Act
            var parent = contentService.GetById(1046);
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
            var content = contentService.GetById(1046);

            // Act
            bool published = contentService.PublishWithChildren(content, 0);
            var children = contentService.GetChildren(1046);

            // Assert
            Assert.That(published, Is.True);//Nothing was cancelled, so should be true
            Assert.That(content.Published, Is.True);//No restrictions, so should be published
            Assert.That(children.First(x => x.Id == 1047).Published, Is.True);//Released 5 mins ago, so should be published
        }

        [Test]
        public void Cannot_Publish_Expired_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(1048); //This Content expired 5min ago
            content.ExpireDate = DateTime.Now.AddMinutes(-5);
            contentService.Save(content);

            var parent = contentService.GetById(1046);
            bool parentPublished = contentService.Publish(parent, 0);//Publish root Home node to enable publishing of '1048'

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
            var content = contentService.GetById(1047);
            content.ReleaseDate = DateTime.Now.AddHours(2);
            contentService.Save(content, 0);

            var parent = contentService.GetById(1046);
            bool parentPublished = contentService.Publish(parent, 0);//Publish root Home node to enable publishing of '1048'

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
            var content = contentService.CreateContent("Subpage with Unpublisehed Parent", 1046, "umbTextpage", 0);
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
            var content = contentService.GetById(1049);

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
            var content = contentService.CreateContent("Home US", - 1, "umbTextpage", 0);
            content.SetValue("author", "Barack Obama");

            // Act
            bool published = contentService.SaveAndPublish(content, 0);

            // Assert
            Assert.That(content.HasIdentity, Is.True);
            Assert.That(content.Published, Is.True);
            Assert.That(published, Is.True);
        }

        [Test]
        public void Can_Get_Published_Descendant_Versions()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var root = contentService.GetById(1046);
            var rootPublished = contentService.Publish(root);
            var content = contentService.GetById(1048);
            content.Properties["title"].Value = content.Properties["title"].Value + " Published";
            bool published = contentService.SaveAndPublish(content);

            var publishedVersion = content.Version;

            content.Properties["title"].Value = content.Properties["title"].Value + " Saved";
            contentService.Save(content);

            var savedVersion = content.Version;

            // Act
            var publishedDescendants = ((ContentService) contentService).GetPublishedDescendants(root);

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
            var currentContent = contentService.GetById(1048);
            Assert.That(currentContent.Published, Is.False);
            Assert.That(currentContent.Properties["title"].Value, Contains.Substring("Saved"));
            Assert.That(currentContent.Version, Is.EqualTo(savedVersion));
        }

        [Test]
        public void Can_Save_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.CreateContent("Home US", - 1, "umbTextpage", 0);
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
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Subpage 1", 1047);
            Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Subpage 2", 1047);
            var list = new List<IContent> {subpage, subpage2};

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
            var hierarchy = CreateContentHierarchy();

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
            var content = contentService.GetById(1049);

            // Act
            contentService.Delete(content, 0);
            var deleted = contentService.GetById(1049);

            // Assert
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void Can_Move_Content_To_RecycleBin()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(1048);

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
            Content subsubpage = MockedContent.CreateSimpleContent(contentType, "Text Page 3", 1047);
            contentService.Save(subsubpage, 0);

            var content = contentService.GetById(1046);

            // Act
            contentService.MoveToRecycleBin(content, 0);
            var descendants = contentService.GetDescendants(content);

            // Assert
            Assert.That(content.ParentId, Is.EqualTo(-20));
            Assert.That(content.Trashed, Is.True);
            Assert.That(descendants.Count(), Is.EqualTo(3));
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
        public void Can_Move_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(1049);

            // Act - moving out of recycle bin
            contentService.Move(content, 1046, 0);

            // Assert
            Assert.That(content.ParentId, Is.EqualTo(1046));
            Assert.That(content.Trashed, Is.False);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Can_Copy_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var temp = contentService.GetById(1048);

            // Act
            var copy = contentService.Copy(temp, temp.ParentId, false, 0);
            var content = contentService.GetById(1048);

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

        [Test, NUnit.Framework.Ignore]
        public void Can_Send_To_Publication()
        { }

        [Test]
        public void Can_Rollback_Version_On_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var parent = ServiceContext.ContentService.GetById(1046);
            ServiceContext.ContentService.Publish(parent);//Publishing root, so Text Page 2 can be updated.
            var subpage2 = contentService.GetById(1048);
            var version = subpage2.Version;
            var nameBeforeRollback = subpage2.Name;
            subpage2.Name = "Text Page 2 Updated";
            subpage2.SetValue("author", "Jane Doe");
            contentService.SaveAndPublish(subpage2, 0);//Saving and publishing, so a new version is created

            // Act
            var rollback = contentService.Rollback(1048, version, 0);

            // Assert
            Assert.That(rollback, Is.Not.Null);
            Assert.AreNotEqual(rollback.Version, subpage2.Version);
            Assert.That(rollback.GetValue<string>("author"), Is.Not.EqualTo("Jane Doe"));
            Assert.AreEqual(nameBeforeRollback, rollback.Name);
        }

        [Test]
        public void Can_Save_Lazy_Content()
        {	        
	        var unitOfWork = PetaPocoUnitOfWorkProvider.CreateUnitOfWork();
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");
            var root = ServiceContext.ContentService.GetById(1046);

            var c = new Lazy<IContent>(() => MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Page", root.Id));
            var c2 = new Lazy<IContent>(() => MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Subpage", c.Value.Id));
            var list = new List<Lazy<IContent>> {c, c2};

            var repository = RepositoryResolver.Current.ResolveByType<IContentRepository>(unitOfWork);
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

        [Test]
        public void Can_Verify_Content_Has_Published_Version()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(1046);
            bool published = contentService.PublishWithChildren(content, 0);
            var homepage = contentService.GetById(1046);
            homepage.Name = "Homepage";
            ServiceContext.ContentService.Save(homepage);

            // Act
            bool hasPublishedVersion = ServiceContext.ContentService.HasPublishedVersion(1046);

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
            Assert.That(sut.GetValue<DateTime>("dateTime"), Is.EqualTo(content.GetValue<DateTime>("dateTime")));
            Assert.That(sut.GetValue<string>("colorPicker"), Is.EqualTo("black"));
	        Assert.That(sut.GetValue<string>("folderBrowser"), Is.Empty);
            Assert.That(sut.GetValue<string>("ddlMultiple"), Is.EqualTo("1234,1235"));
            Assert.That(sut.GetValue<string>("rbList"), Is.EqualTo("random"));
            Assert.That(sut.GetValue<DateTime>("date"), Is.EqualTo(content.GetValue<DateTime>("date")));
            Assert.That(sut.GetValue<string>("ddl"), Is.EqualTo("1234"));
            Assert.That(sut.GetValue<string>("chklist"), Is.EqualTo("randomc"));
            Assert.That(sut.GetValue<int>("contentPicker"), Is.EqualTo(1090));
            Assert.That(sut.GetValue<int>("mediaPicker"), Is.EqualTo(1091));
            Assert.That(sut.GetValue<int>("memberPicker"), Is.EqualTo(1092));
            Assert.That(sut.GetValue<string>("simpleEditor"), Is.EqualTo("This is simply edited"));
            Assert.That(sut.GetValue<string>("ultimatePicker"), Is.EqualTo("1234,1235"));
            Assert.That(sut.GetValue<string>("relatedLinks"), Is.EqualTo("<links><link title=\"google\" link=\"http://google.com\" type=\"external\" newwindow=\"0\" /></links>"));
            Assert.That(sut.GetValue<string>("tags"), Is.EqualTo("this,is,tags"));
            Assert.That(sut.GetValue<string>("macroContainer"), Is.Empty);
            Assert.That(sut.GetValue<string>("imgCropper"), Is.Empty);
	    }

        private IEnumerable<IContent> CreateContentHierarchy()
        {
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");
            var root = ServiceContext.ContentService.GetById(1046);

			var list = new List<IContent>();

            for (int i = 0; i < 10; i++)
            {
				var content = MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Page " + i, root);

                list.Add(content);
                list.AddRange(CreateChildrenOf(contentType, content, 4));

                Console.WriteLine("Created: 'Hierarchy Simple Text Page {0}'", i);
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

                Console.WriteLine("Created: 'Hierarchy Simple Text Subpage {0}' - Depth: {1}", i, depth);
            }
            return list;
        }
    }
}