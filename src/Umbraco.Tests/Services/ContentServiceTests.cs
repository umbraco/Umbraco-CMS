using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.Testing;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Cache;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.LegacyXmlPublishedCache;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering all methods in the ContentService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [TestFixture]
    [Category("Slow")]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest,
        PublishedRepositoryEvents = true,
        WithApplication = true,
        Logger = UmbracoTestOptions.Logger.Console)]
    public class ContentServiceTests : TestWithSomeContentBase
    {
        // TODO: Add test to verify there is only ONE newest document/content in {Constants.DatabaseSchema.Tables.Document} table after updating.
        // TODO: Add test to delete specific version (with and without deleting prior versions) and versions by date.

        public override void SetUp()
        {
            base.SetUp();
            ContentRepositoryBase.ThrowOnWarning = true;
        }

        public override void TearDown()
        {
            ContentRepositoryBase.ThrowOnWarning = false;
            base.TearDown();
        }

        protected override void Compose()
        {
            base.Compose();

            Composition.RegisterUnique(factory => Mock.Of<ILocalizedTextService>());
        }

        [Test]
        public void Create_Blueprint()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            contentTypeService.Save(contentType);

            var blueprint = MockedContent.CreateTextpageContent(contentType, "hello", Constants.System.Root);
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

            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            contentTypeService.Save(contentType);

            var blueprint = MockedContent.CreateTextpageContent(contentType, "hello", Constants.System.Root);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var contentService = ServiceContext.ContentService;
                var contentTypeService = ServiceContext.ContentTypeService;

                var contentType = MockedContentTypes.CreateTextPageContentType();
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
                contentTypeService.Save(contentType);

                var blueprint = MockedContent.CreateTextpageContent(contentType, "hello", Constants.System.Root);
                blueprint.SetValue("title", "blueprint 1");
                blueprint.SetValue("bodyText", "blueprint 2");
                blueprint.SetValue("keywords", "blueprint 3");
                blueprint.SetValue("description", "blueprint 4");

                contentService.SaveBlueprint(blueprint);

                var fromBlueprint = contentService.CreateContentFromBlueprint(blueprint, "hello world");
                contentService.Save(fromBlueprint);

                Assert.IsTrue(fromBlueprint.HasIdentity);
                Assert.AreEqual("blueprint 1", fromBlueprint.Properties["title"].GetValue());
                Assert.AreEqual("blueprint 2", fromBlueprint.Properties["bodyText"].GetValue());
                Assert.AreEqual("blueprint 3", fromBlueprint.Properties["keywords"].GetValue());
                Assert.AreEqual("blueprint 4", fromBlueprint.Properties["description"].GetValue());
            }

        }

        [Test]
        public void Get_All_Blueprints()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            var ct1 = MockedContentTypes.CreateTextPageContentType("ct1");
            ServiceContext.FileService.SaveTemplate(ct1.DefaultTemplate);
            contentTypeService.Save(ct1);
            var ct2 = MockedContentTypes.CreateTextPageContentType("ct2");
            ServiceContext.FileService.SaveTemplate(ct2.DefaultTemplate);
            contentTypeService.Save(ct2);

            for (int i = 0; i < 10; i++)
            {
                var blueprint = MockedContent.CreateTextpageContent(i % 2 == 0 ? ct1 : ct2, "hello" + i, Constants.System.Root);
                contentService.SaveBlueprint(blueprint);
            }

            var found = contentService.GetBlueprintsForContentTypes().ToArray();
            Assert.AreEqual(10, found.Length);

            found = contentService.GetBlueprintsForContentTypes(ct1.Id).ToArray();
            Assert.AreEqual(5, found.Length);

            found = contentService.GetBlueprintsForContentTypes(ct2.Id).ToArray();
            Assert.AreEqual(5, found.Length);
        }

        [Test]
        public void Perform_Scheduled_Publishing()
        {
            var langUk = new Language("en-GB") { IsDefault = true };
            var langFr = new Language("fr-FR");

            ServiceContext.LocalizationService.Save(langFr);
            ServiceContext.LocalizationService.Save(langUk);

            var ctInvariant = MockedContentTypes.CreateBasicContentType("invariantPage");
            ServiceContext.ContentTypeService.Save(ctInvariant);

            var ctVariant = MockedContentTypes.CreateBasicContentType("variantPage");
            ctVariant.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(ctVariant);

            var now = DateTime.Now;

            //10x invariant content, half is scheduled to be published in 5 seconds, the other half is scheduled to be unpublished in 5 seconds
            var invariant = new List<IContent>();
            for (var i = 0; i < 10; i++)
            {
                var c = MockedContent.CreateBasicContent(ctInvariant);
                c.Name = "name" + i;
                if (i % 2 == 0)
                {
                    c.ContentSchedule.Add(now.AddSeconds(5), null); //release in 5 seconds
                    var r = ServiceContext.ContentService.Save(c);
                    Assert.IsTrue(r.Success, r.Result.ToString());
                }
                else
                {
                    c.ContentSchedule.Add(null, now.AddSeconds(5)); //expire in 5 seconds
                    var r = ServiceContext.ContentService.SaveAndPublish(c);
                    Assert.IsTrue(r.Success, r.Result.ToString());
                }
                invariant.Add(c);
            }

            //10x variant content, half is scheduled to be published in 5 seconds, the other half is scheduled to be unpublished in 5 seconds
            var variant = new List<IContent>();
            var alternatingCulture = langFr.IsoCode;
            for (var i = 0; i < 10; i++)
            {
                var c = MockedContent.CreateBasicContent(ctVariant);
                c.SetCultureName("name-uk" + i, langUk.IsoCode);
                c.SetCultureName("name-fr" + i, langFr.IsoCode);

                if (i % 2 == 0)
                {
                    c.ContentSchedule.Add(alternatingCulture, now.AddSeconds(5), null); //release in 5 seconds
                    var r = ServiceContext.ContentService.Save(c);
                    Assert.IsTrue(r.Success, r.Result.ToString());

                    alternatingCulture = alternatingCulture == langFr.IsoCode ? langUk.IsoCode : langFr.IsoCode;
                }
                else
                {
                    c.ContentSchedule.Add(alternatingCulture, null, now.AddSeconds(5)); //expire in 5 seconds
                    var r = ServiceContext.ContentService.SaveAndPublish(c);
                    Assert.IsTrue(r.Success, r.Result.ToString());
                }
                variant.Add(c);
            }


            var runSched = ServiceContext.ContentService.PerformScheduledPublish(
                now.AddMinutes(1)).ToList(); //process anything scheduled before a minute from now

            //this is 21 because the test data installed before this test runs has a scheduled item!
            Assert.AreEqual(21, runSched.Count);
            Assert.AreEqual(20, runSched.Count(x => x.Success),
                string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));

            Assert.AreEqual(5, runSched.Count(x => x.Result == PublishResultType.SuccessPublish),
                string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));
            Assert.AreEqual(5, runSched.Count(x => x.Result == PublishResultType.SuccessUnpublish),
                string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));
            Assert.AreEqual(5, runSched.Count(x => x.Result == PublishResultType.SuccessPublishCulture),
                string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));
            Assert.AreEqual(5, runSched.Count(x => x.Result == PublishResultType.SuccessUnpublishCulture),
                string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));

            //re-run the scheduled publishing, there should be no results
            runSched = ServiceContext.ContentService.PerformScheduledPublish(
               now.AddMinutes(1)).ToList();

            Assert.AreEqual(0, runSched.Count);
        }

        [Test]
        public void Remove_Scheduled_Publishing_Date()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.CreateAndSave("Test", Constants.System.Root, "umbTextpage", Constants.Security.SuperUserId);

            content.ContentSchedule.Add(null, DateTime.Now.AddHours(2));
            contentService.Save(content, Constants.Security.SuperUserId);
            Assert.AreEqual(1, content.ContentSchedule.FullSchedule.Count);

            content = contentService.GetById(content.Id);
            var sched = content.ContentSchedule.FullSchedule;
            Assert.AreEqual(1, sched.Count);
            Assert.AreEqual(1, sched.Count(x => x.Culture == string.Empty));
            content.ContentSchedule.Clear(ContentScheduleAction.Expire);
            contentService.Save(content, Constants.Security.SuperUserId);


            // Assert
            content = contentService.GetById(content.Id);
            sched = content.ContentSchedule.FullSchedule;
            Assert.AreEqual(0, sched.Count);
            Assert.IsTrue(contentService.SaveAndPublish(content).Success);
        }

        [Test]
        public void Get_Top_Version_Ids()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.CreateAndSave("Test", Constants.System.Root, "umbTextpage", Constants.Security.SuperUserId);
            for (var i = 0; i < 20; i++)
            {
                content.SetValue("bodyText", "hello world " + Guid.NewGuid());
                contentService.SaveAndPublish(content);
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
            for (var i = 0; i < 20; i++)
            {
                results.Add(contentService.CreateAndSave("Test", Constants.System.Root, "umbTextpage", 0));
            }

            var sortedGet = contentService.GetByIds(new[] {results[10].Id, results[5].Id, results[12].Id}).ToArray();

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
                contentService.CreateAndSave("Test", Constants.System.Root, "umbTextpage", Constants.Security.SuperUserId);
            }

            // Assert
            Assert.AreEqual(25, contentService.Count());
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
                contentService.CreateAndSave("Test", Constants.System.Root, "umbBlah", Constants.Security.SuperUserId);
            }

            // Assert
            Assert.AreEqual(20, contentService.Count(documentTypeAlias: "umbBlah"));
        }

        [Test]
        public void Count_Children()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbBlah", "test Doc Type");
            contentTypeService.Save(contentType);
            var parent = contentService.CreateAndSave("Test", Constants.System.Root, "umbBlah", Constants.Security.SuperUserId);

            // Act
            for (int i = 0; i < 20; i++)
            {
                contentService.CreateAndSave("Test", parent, "umbBlah");
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
            var parent = contentService.CreateAndSave("Test", Constants.System.Root, "umbBlah", Constants.Security.SuperUserId);

            // Act
            IContent current = parent;
            for (int i = 0; i < 20; i++)
            {
                current = contentService.CreateAndSave("Test", current, "umbBlah");
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
        public void Can_Remove_Property_Type()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.Create("Test", Constants.System.Root, "umbTextpage", Constants.Security.SuperUserId);

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
            var content = contentService.Create("Test", Constants.System.Root, "umbTextpage", Constants.Security.SuperUserId);

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.HasIdentity, Is.False);
        }

        [Test]
        public void Automatically_Track_Relations()
        {
            var mt = MockedContentTypes.CreateSimpleMediaType("testMediaType", "Test Media Type");
            ServiceContext.MediaTypeService.Save(mt);
            var m1 = MockedMedia.CreateSimpleMedia(mt, "hello 1", -1);
            var m2 = MockedMedia.CreateSimpleMedia(mt, "hello 1", -1);
            ServiceContext.MediaService.Save(m1);
            ServiceContext.MediaService.Save(m2);

            var ct = MockedContentTypes.CreateTextPageContentType("richTextTest");
            ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
            
            ServiceContext.ContentTypeService.Save(ct);

            var c1 = MockedContent.CreateTextpageContent(ct, "my content 1", -1);
            ServiceContext.ContentService.Save(c1);

            var c2 = MockedContent.CreateTextpageContent(ct, "my content 2", -1);

            //'bodyText' is a property with a RTE property editor which we knows tracks relations
            c2.Properties["bodyText"].SetValue(@"<p>
        <img src='/media/12312.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + @"' />
</p><p><img src='/media/234234.jpg' data-udi=""umb://media/" + m2.Key.ToString("N") + @""" />
</p>
<p>
    <a href=""{locallink:umb://document/" + c1.Key.ToString("N") + @"}"">hello</a>
</p>");

            ServiceContext.ContentService.Save(c2);

            var relations = ServiceContext.RelationService.GetByParentId(c2.Id).ToList();
            Assert.AreEqual(3, relations.Count);
            Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedMediaAlias, relations[0].RelationType.Alias);
            Assert.AreEqual(m1.Id, relations[0].ChildId);
            Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedMediaAlias, relations[1].RelationType.Alias);
            Assert.AreEqual(m2.Id, relations[1].ChildId);
            Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedDocumentAlias, relations[2].RelationType.Alias);
            Assert.AreEqual(c1.Id, relations[2].ChildId);
        }

        [Test]
        public void Can_Create_Content_Without_Explicitly_Set_User()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.Create("Test", Constants.System.Root, "umbTextpage");

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.HasIdentity, Is.False);
            Assert.That(content.CreatorId, Is.EqualTo(Constants.Security.SuperUserId)); //Default to -1 aka SuperUser (unknown) since we didn't explicitly set this in the Create call
        }

        [Test]
        public void Can_Save_New_Content_With_Explicit_User()
        {
            var user = new User
                {
                    Name = "Test",
                    Email = "test@test.com",
                    Username = "test",
                RawPasswordValue = "test"
                };
            ServiceContext.UserService.Save(user);
            var content = new Content("Test", Constants.System.Root, ServiceContext.ContentTypeService.Get("umbTextpage"));

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
            Assert.Throws<Exception>(() => contentService.Create("Test", Constants.System.Root, "umbAliasDoesntExist"));
        }

        [Test]
        public void Cannot_Save_Content_With_Empty_Name()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = new Content(string.Empty, Constants.System.Root, ServiceContext.ContentTypeService.Get("umbTextpage"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => contentService.Save(content));
        }

        [Test]
        public void Can_Get_Content_By_Id()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            var content = contentService.GetById(NodeDto.NodeIdSeed + 2 );

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Id, Is.EqualTo(NodeDto.NodeIdSeed + 2));
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
            Assert.That(content.Id, Is.EqualTo(NodeDto.NodeIdSeed + 2));
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
        public void Can_Get_All_Versions_Of_Content()
        {
            var contentService = ServiceContext.ContentService;

            var parent = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 2);
            Assert.IsFalse(parent.Published);
            ServiceContext.ContentService.SaveAndPublish(parent); // publishing parent, so Text Page 2 can be updated.

            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);
            Assert.IsFalse(content.Published);
            var versions = contentService.GetVersions(NodeDto.NodeIdSeed + 4).ToList();
            Assert.AreEqual(1, versions.Count);

            var version1 = content.VersionId;
            Console.WriteLine($"1 e={content.VersionId} p={content.PublishedVersionId}");

            content.Name = "Text Page 2 Updated";
            content.SetValue("author", "Jane Doe");
            contentService.SaveAndPublish(content); // publishes the current version, creates a version

            var version2 = content.VersionId;
            Console.WriteLine($"2 e={content.VersionId} p={content.PublishedVersionId}");

            content.Name = "Text Page 2 ReUpdated";
            content.SetValue("author", "Bob Hope");
            contentService.SaveAndPublish(content); // publishes again, creates a version

            var version3 = content.VersionId;
            Console.WriteLine($"3 e={content.VersionId} p={content.PublishedVersionId}");

            var content1 = contentService.GetById(content.Id);
            Assert.AreEqual("Bob Hope", content1.GetValue("author"));
            Assert.AreEqual("Bob Hope", content1.GetValue("author", published: true));

            content.Name = "Text Page 2 ReReUpdated";
            content.SetValue("author", "John Farr");
            contentService.Save(content); // no new version

            content1 = contentService.GetById(content.Id);
            Assert.AreEqual("John Farr", content1.GetValue("author"));
            Assert.AreEqual("Bob Hope", content1.GetValue("author", published: true));

            versions = contentService.GetVersions(NodeDto.NodeIdSeed + 4).ToList();
            Assert.AreEqual(3, versions.Count);

            // versions come with most recent first
            Assert.AreEqual(version3, versions[0].VersionId); // the edited version
            Assert.AreEqual(version2, versions[1].VersionId); // the published version
            Assert.AreEqual(version1, versions[2].VersionId); // the previously published version

            // p is always the same, published version
            // e is changing, actual version we're loading
            Console.WriteLine();
            foreach (var version in ((IEnumerable<IContent>) versions).Reverse())
                Console.WriteLine($"+ e={((Content) version).VersionId} p={((Content) version).PublishedVersionId}");

            // and proper values
            // first, the current (edited) version, with edited and published versions
            Assert.AreEqual("John Farr", versions[0].GetValue("author")); // current version has the edited value
            Assert.AreEqual("Bob Hope", versions[0].GetValue("author", published: true)); // and the published published value

            // then, the current (published) version, with edited == published
            Assert.AreEqual("Bob Hope", versions[1].GetValue("author")); // own edited version
            Assert.AreEqual("Bob Hope", versions[1].GetValue("author", published: true)); // and published

            // then, the first published version - with values as 'edited'
            Assert.AreEqual("Jane Doe", versions[2].GetValue("author")); // own edited version
            Assert.AreEqual("Bob Hope", versions[2].GetValue("author", published: true)); // and published
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
            var root = contentService.GetById(NodeDto.NodeIdSeed + 2);
            contentService.SaveAndPublish(root);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);
            content.ContentSchedule.Add(null, DateTime.Now.AddSeconds(1));
            contentService.SaveAndPublish(content);

            // Act
            Thread.Sleep(new TimeSpan(0, 0, 0, 2));
            var contents = contentService.GetContentForExpiration(DateTime.Now).ToList();

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
            var contents = contentService.GetContentForRelease(DateTime.Now).ToList();

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
            var contents = contentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var _).ToList();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_Unpublish_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 2);
            var published = contentService.SaveAndPublish(content, userId: 0);

            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.IsTrue(scope.Database.Exists<ContentXmlDto>(content.Id));
            }

            // Act
            var unpublished = contentService.Unpublish(content, userId: 0);

            // Assert
            Assert.That(published.Success, Is.True);
            Assert.That(unpublished.Success, Is.True);
            Assert.That(content.Published, Is.False);
            Assert.AreEqual(PublishResultType.SuccessUnpublish, unpublished.Result);

            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.IsFalse(scope.Database.Exists<ContentXmlDto>(content.Id));
            }
        }

        [Test]
        public void Can_Unpublish_Content_Variation()
        {
            var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr, out var contentType);

            content.PublishCulture(CultureImpact.Explicit(langFr.IsoCode, langFr.IsDefault));
            content.PublishCulture(CultureImpact.Explicit(langUk.IsoCode, langUk.IsDefault));
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

            var published = ServiceContext.ContentService.SaveAndPublish(content, new[]{ langFr.IsoCode , langUk.IsoCode });
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.IsTrue(published.Success);
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

            var unpublished = ServiceContext.ContentService.Unpublish(content, langFr.IsoCode);
            Assert.IsTrue(unpublished.Success);
            Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));


        }

        [Test]
        public void Can_Publish_Culture_After_Last_Culture_Unpublished()
        {
            var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr, out var contentType);

            var published = ServiceContext.ContentService.SaveAndPublish(content, new[] { langFr.IsoCode, langUk.IsoCode });
            Assert.AreEqual(PublishedState.Published, content.PublishedState);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);

            var unpublished = ServiceContext.ContentService.Unpublish(content, langUk.IsoCode); //first culture
            Assert.IsTrue(unpublished.Success);
            Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
            Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));

            content = ServiceContext.ContentService.GetById(content.Id);

            unpublished = ServiceContext.ContentService.Unpublish(content, langFr.IsoCode); //last culture
            Assert.IsTrue(unpublished.Success);
            Assert.AreEqual(PublishResultType.SuccessUnpublishLastCulture, unpublished.Result);
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

            content = ServiceContext.ContentService.GetById(content.Id);

            published = ServiceContext.ContentService.SaveAndPublish(content, langUk.IsoCode);
            Assert.AreEqual(PublishedState.Published, content.PublishedState);
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            
            content = ServiceContext.ContentService.GetById(content.Id); //reget
            Assert.AreEqual(PublishedState.Published, content.PublishedState);
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            
        }

        

        [Test]
        public void Unpublish_All_Cultures_Has_Unpublished_State()
        {
            var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr, out var contentType);

            var published = ServiceContext.ContentService.SaveAndPublish(content, new[] { langFr.IsoCode, langUk.IsoCode });
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
            Assert.IsTrue(published.Success);
            Assert.AreEqual(PublishedState.Published, content.PublishedState);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
            Assert.AreEqual(PublishedState.Published, content.PublishedState);

            var unpublished = ServiceContext.ContentService.Unpublish(content, langFr.IsoCode); //first culture
            Assert.IsTrue(unpublished.Success);
            Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
            Assert.AreEqual(PublishedState.Published, content.PublishedState); //still published

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

            unpublished = ServiceContext.ContentService.Unpublish(content, langUk.IsoCode); //last culture
            Assert.IsTrue(unpublished.Success);
            Assert.AreEqual(PublishResultType.SuccessUnpublishLastCulture, unpublished.Result);
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
            Assert.AreEqual(PublishedState.Unpublished, content.PublishedState); //the last culture was unpublished so the document should also reflect this

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.AreEqual(PublishedState.Unpublished, content.PublishedState); //just double checking
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));          
            Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
        }

        [Test]
        public void Unpublishing_Mandatory_Language_Unpublishes_Document()
        {
            var langUk = new Language("en-GB") { IsDefault = true, IsMandatory = true };
            var langFr = new Language("fr-FR");

            ServiceContext.LocalizationService.Save(langFr);
            ServiceContext.LocalizationService.Save(langUk);

            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            IContent content = new Content("content", Constants.System.Root, contentType);
            content.SetCultureName("content-fr", langFr.IsoCode);
            content.SetCultureName("content-en", langUk.IsoCode);

            var published = ServiceContext.ContentService.SaveAndPublish(content, new[] { langFr.IsoCode, langUk.IsoCode });
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
            Assert.IsTrue(published.Success);
            Assert.AreEqual(PublishedState.Published, content.PublishedState);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            
            var unpublished = ServiceContext.ContentService.Unpublish(content, langUk.IsoCode); //unpublish mandatory lang
            Assert.IsTrue(unpublished.Success);
            Assert.AreEqual(PublishResultType.SuccessUnpublishMandatoryCulture, unpublished.Result);            
            Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode)); //remains published
            Assert.AreEqual(PublishedState.Unpublished, content.PublishedState); 
        }

        [Test]
        public void Unpublishing_Already_Unpublished_Culture()
        {
            var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr, out var contentType);

            var published = ServiceContext.ContentService.SaveAndPublish(content, new[] { langFr.IsoCode, langUk.IsoCode });
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
            Assert.IsTrue(published.Success);
            Assert.AreEqual(PublishedState.Published, content.PublishedState);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);

            var unpublished = ServiceContext.ContentService.Unpublish(content, langUk.IsoCode); 
            Assert.IsTrue(unpublished.Success);
            Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
            Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

            content = ServiceContext.ContentService.GetById(content.Id);

            //Change some data since Unpublish should always Save
            content.SetCultureName("content-en-updated", langUk.IsoCode);

            unpublished = ServiceContext.ContentService.Unpublish(content, langUk.IsoCode); //unpublish again
            Assert.IsTrue(unpublished.Success);
            Assert.AreEqual(PublishResultType.SuccessUnpublishAlready, unpublished.Result);
            Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

            content = ServiceContext.ContentService.GetById(content.Id);
            //ensure that even though the culture was already unpublished that the data was still persisted
            Assert.AreEqual("content-en-updated", content.GetCultureName(langUk.IsoCode));
        }

        [Test]
        public void Publishing_No_Cultures_Still_Saves()
        {
            var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr, out var contentType);

            var published = ServiceContext.ContentService.SaveAndPublish(content, new[] { langFr.IsoCode, langUk.IsoCode });
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
            Assert.IsTrue(published.Success);
            Assert.AreEqual(PublishedState.Published, content.PublishedState);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);

            //Change some data since SaveAndPublish should always Save
            content.SetCultureName("content-en-updated", langUk.IsoCode);

            var saved = ServiceContext.ContentService.SaveAndPublish(content, new string [] { }); //save without cultures            
            Assert.AreEqual(PublishResultType.FailedPublishNothingToPublish, saved.Result);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            //ensure that even though nothing was published that the data was still persisted
            Assert.AreEqual("content-en-updated", content.GetCultureName(langUk.IsoCode));
        }


        [Test]
        public void Pending_Invariant_Property_Changes_Affect_Default_Language_Edited_State()
        {
            // Arrange

            var langGB = new Language("en-GB") { IsDefault = true };
            var langFr = new Language("fr-FR");

            ServiceContext.LocalizationService.Save(langFr);
            ServiceContext.LocalizationService.Save(langGB);

            var contentType = MockedContentTypes.CreateMetaContentType();
            contentType.Variations = ContentVariation.Culture;
            foreach(var prop in contentType.PropertyTypes)
                prop.Variations = ContentVariation.Culture;
            var keywordsProp = contentType.PropertyTypes.Single(x => x.Alias == "metakeywords");
            keywordsProp.Variations = ContentVariation.Nothing; // this one is invariant

            ServiceContext.ContentTypeService.Save(contentType);

            IContent content = new Content("content", Constants.System.Root, contentType);
            content.SetCultureName("content-en", langGB.IsoCode);
            content.SetCultureName("content-fr", langFr.IsoCode);

            Assert.IsTrue(ServiceContext.ContentService.SaveAndPublish(content, new []{ langGB.IsoCode , langFr.IsoCode }).Success);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.AreEqual(PublishedState.Published, content.PublishedState);
            Assert.IsTrue(content.IsCulturePublished(langGB.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsFalse(content.IsCultureEdited(langGB.IsoCode));
            Assert.IsFalse(content.IsCultureEdited(langFr.IsoCode));

            //update the invariant property and save a pending version
            content.SetValue("metakeywords", "hello");
            ServiceContext.ContentService.Save(content);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.AreEqual(PublishedState.Published, content.PublishedState);
            Assert.IsTrue(content.IsCulturePublished(langGB.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCultureEdited(langGB.IsoCode));
            Assert.IsFalse(content.IsCultureEdited(langFr.IsoCode));
        }

        [Test]
        public void Can_Publish_Content_Variation_And_Detect_Changed_Cultures()
        {
            CreateEnglishAndFrenchDocumentType(out var langUk, out var langFr, out var contentType);

            IContent content = new Content("content", Constants.System.Root, contentType);
            content.SetCultureName("content-fr", langFr.IsoCode);
            var published = ServiceContext.ContentService.SaveAndPublish(content, langFr.IsoCode);
            //audit log will only show that french was published
            var lastLog = ServiceContext.AuditService.GetLogs(content.Id).Last();
            Assert.AreEqual($"Published languages: French (France)", lastLog.Comment);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            content.SetCultureName("content-en", langUk.IsoCode);
            published = ServiceContext.ContentService.SaveAndPublish(content, langUk.IsoCode);
            //audit log will only show that english was published
            lastLog = ServiceContext.AuditService.GetLogs(content.Id).Last();
            Assert.AreEqual($"Published languages: English (United Kingdom)", lastLog.Comment);
        }

        [Test]
        public void Can_Unpublish_Content_Variation_And_Detect_Changed_Cultures()
        {
            // Arrange

            var langGB = new Language("en-GB") { IsDefault = true, IsMandatory = true };
            var langFr = new Language("fr-FR");

            ServiceContext.LocalizationService.Save(langFr);
            ServiceContext.LocalizationService.Save(langGB);

            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            IContent content = new Content("content", Constants.System.Root, contentType);
            content.SetCultureName("content-fr", langFr.IsoCode);
            content.SetCultureName("content-gb", langGB.IsoCode);
            var published = ServiceContext.ContentService.SaveAndPublish(content, new[] {langGB.IsoCode, langFr.IsoCode});
            Assert.IsTrue(published.Success);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            var unpublished = ServiceContext.ContentService.Unpublish(content, langFr.IsoCode);
            //audit log will only show that french was unpublished
            var lastLog = ServiceContext.AuditService.GetLogs(content.Id).Last();
            Assert.AreEqual($"Unpublished languages: French (France)", lastLog.Comment);

            //re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            content.SetCultureName("content-en", langGB.IsoCode);
            unpublished = ServiceContext.ContentService.Unpublish(content, langGB.IsoCode);
            //audit log will only show that english was published
            var logs = ServiceContext.AuditService.GetLogs(content.Id).ToList();
            Assert.AreEqual($"Unpublished languages: English (United Kingdom)", logs[logs.Count - 2].Comment);
            Assert.AreEqual($"Unpublished (mandatory language unpublished)", logs[logs.Count - 1].Comment);
        }

        [Test]
        public void Can_Publish_Content_1()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 2);

            // Act
            var published = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

            // Assert
            Assert.That(published.Success, Is.True);
            Assert.That(content.Published, Is.True);
        }

        [Test]
        public void Can_Publish_Content_2()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 2);

            // Act
            var published = contentService.SaveAndPublish(content, userId: 0);

            // Assert
            Assert.That(published.Success, Is.True);
            Assert.That(content.Published, Is.True);
        }

        [Test]
        public void IsPublishable()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var parent = contentService.Create("parent", Constants.System.Root, "umbTextpage");

            contentService.SaveAndPublish(parent);
            var content = contentService.Create("child", parent, "umbTextpage");
            contentService.Save(content);

            Assert.IsTrue(contentService.IsPathPublishable(content));
            contentService.Unpublish(parent);
            Assert.IsFalse(contentService.IsPathPublishable(content));
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
                var content = contentService.GetById(NodeDto.NodeIdSeed + 2);
                Assert.AreEqual("Home", content.Name);

                content.Name = "foo";
                var published = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

                Assert.That(published.Success, Is.True);
                Assert.That(content.Published, Is.True);

                var e = ServiceContext.ContentService.GetById(content.Id);
                Assert.AreEqual("foo", e.Name);
            }
            finally
            {
                ContentService.Publishing -= ContentServiceOnPublishing;
            }
        }

        private void ContentServiceOnPublishing(IContentService sender, PublishEventArgs<IContent> args)
        {
            Assert.AreEqual(1, args.PublishedEntities.Count());
            var entity = args.PublishedEntities.First();
            Assert.AreEqual("foo", entity.Name);

            var e = ServiceContext.ContentService.GetById(entity.Id);
            Assert.AreEqual("Home", e.Name);
        }

        [Test]
        public void Can_Not_Publish_Invalid_Cultures()
        {
            var contentService = ServiceContext.ContentService;
            var content = Mock.Of<IContent>(c => c.ContentType == Mock.Of<ISimpleContentType>(s => s.Variations == ContentVariation.Culture));

            Assert.Throws<InvalidOperationException>(() => contentService.SaveAndPublish(content, new[] {"*"}));
            Assert.Throws<InvalidOperationException>(() => contentService.SaveAndPublish(content, new string[] { null }));
            Assert.Throws<InvalidOperationException>(() => contentService.SaveAndPublish(content, new[] { "*", null }));
            Assert.Throws<InvalidOperationException>(() => contentService.SaveAndPublish(content, new[] { "en-US", "*", "es-ES" }));
        }

        [Test]
        public void Can_Publish_Only_Valid_Content()
        {
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentTypeService.Save(contentType);

            const int parentId = NodeDto.NodeIdSeed + 2;

            var contentService = ServiceContext.ContentService;
            
            var parent = contentService.GetById(parentId);

            var parentPublished = contentService.SaveAndPublish(parent);

            // parent can publish values
            // and therefore can be published
            Assert.IsTrue(parentPublished.Success);
            Assert.IsTrue(parent.Published);

            var content = MockedContent.CreateSimpleContent(contentType, "Invalid Content", parentId);
            content.SetValue("author", string.Empty);
            Assert.IsFalse(content.HasIdentity);

            // content cannot publish values because they are invalid
            var propertyValidationService = new PropertyValidationService(Factory.GetInstance<PropertyEditorCollection>(), ServiceContext.DataTypeService, ServiceContext.TextService);
            var isValid = propertyValidationService.IsPropertyDataValid(content, out var invalidProperties, CultureImpact.Invariant);
            Assert.IsFalse(isValid);
            Assert.IsNotEmpty(invalidProperties);

            // and therefore cannot be published,
            // because it did not have a published version at all
            var contentPublished = contentService.SaveAndPublish(content);
            Assert.IsFalse(contentPublished.Success);
            Assert.AreEqual(PublishResultType.FailedPublishContentInvalid, contentPublished.Result);
            Assert.IsFalse(content.Published);

            //Ensure it saved though
            Assert.Greater(content.Id, 0);
            Assert.IsTrue(content.HasIdentity);
        }
        

        [Test]
        public void Can_Publish_And_Unpublish_Cultures_In_Single_Operation()
        {
            //TODO: This is using an internal API - we aren't exposing this publicly (at least for now) but we'll keep the test around

            var langFr = new Language("fr");
            var langDa = new Language("da");
            ServiceContext.LocalizationService.Save(langFr);
            ServiceContext.LocalizationService.Save(langDa);

            var ct = MockedContentTypes.CreateBasicContentType();
            ct.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(ct);

            IContent content = MockedContent.CreateBasicContent(ct);
            content.SetCultureName("name-fr", langFr.IsoCode);
            content.SetCultureName("name-da", langDa.IsoCode);

            content.PublishCulture(CultureImpact.Explicit(langFr.IsoCode, langFr.IsDefault));
            var result = ((ContentService)ServiceContext.ContentService).CommitDocumentChanges(content);
            Assert.IsTrue(result.Success);
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsFalse(content.IsCulturePublished(langDa.IsoCode));

            content.UnpublishCulture(langFr.IsoCode);
            content.PublishCulture(CultureImpact.Explicit(langDa.IsoCode, langDa.IsDefault));

            result = ((ContentService)ServiceContext.ContentService).CommitDocumentChanges(content);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(PublishResultType.SuccessMixedCulture, result.Result);

            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langDa.IsoCode));
        }

        // documents: an enumeration of documents, in tree order
        // map: applies (if needed) PublishValue, returns a value indicating whether to proceed with the branch
        private IEnumerable<IContent> MapPublishValues(IEnumerable<IContent> documents, Func<IContent, bool> map)
        {
            var exclude = new HashSet<int>();
            foreach (var document in documents)
            {
                if (exclude.Contains(document.ParentId))
                {
                    exclude.Add(document.Id);
                    continue;
                }
                if (!map(document))
                {
                    exclude.Add(document.Id);
                    continue;
                }
                yield return document;
            }
        }

        [Test]
        public void Can_Publish_Content_Children()
        {
            const int parentId = NodeDto.NodeIdSeed + 2;

            var contentService = ServiceContext.ContentService;
            var parent = contentService.GetById(parentId);

            Console.WriteLine(" " + parent.Id);
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while(page * pageSize < total)
            {
                var descendants = contentService.GetPagedDescendants(parent.Id, page++, pageSize, out total);
                foreach (var x in descendants)
                    Console.WriteLine("          ".Substring(0, x.Level) + x.Id);
            }

            Console.WriteLine();

            // publish parent & its branch
            // only those that are not already published
            // only invariant/neutral values
            var parentPublished = contentService.SaveAndPublishBranch(parent, true);

            foreach (var result in parentPublished)
                Console.WriteLine("          ".Substring(0, result.Content.Level) + $"{result.Content.Id}: {result.Result}");

            // everything should be successful
            Assert.IsTrue(parentPublished.All(x => x.Success));
            Assert.IsTrue(parent.Published);

            var children = contentService.GetPagedChildren(parentId, 0, 500, out var totalChildren); //we only want the first so page size, etc.. is abitrary

            // children are published including ... that was released 5 mins ago
            Assert.IsTrue(children.First(x => x.Id == NodeDto.NodeIdSeed + 4).Published);
        }

        [Test]
        public void Cannot_Publish_Expired_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 4); //This Content expired 5min ago
            content.ContentSchedule.Add(null, DateTime.Now.AddMinutes(-5));
            contentService.Save(content);

            var parent = contentService.GetById(NodeDto.NodeIdSeed + 2);
            var parentPublished = contentService.SaveAndPublish(parent, userId: Constants.Security.SuperUserId);//Publish root Home node to enable publishing of 'NodeDto.NodeIdSeed + 3'

            // Act
            var published = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

            // Assert
            Assert.That(parentPublished.Success, Is.True);
            Assert.That(published.Success, Is.False);
            Assert.That(content.Published, Is.False);
            Assert.AreEqual(PublishResultType.FailedPublishHasExpired, published.Result);
        }

        [Test]
        public void Cannot_Publish_Expired_Culture()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateBasicContent(contentType);
            content.SetCultureName("Hello", "en-US");
            content.ContentSchedule.Add("en-US", null, DateTime.Now.AddMinutes(-5));
            ServiceContext.ContentService.Save(content);

            var published = ServiceContext.ContentService.SaveAndPublish(content, "en-US", Constants.Security.SuperUserId);

            Assert.IsFalse(published.Success);
            Assert.AreEqual(PublishResultType.FailedPublishCultureHasExpired, published.Result);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Cannot_Publish_Content_Awaiting_Release()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 3);
            content.ContentSchedule.Add(DateTime.Now.AddHours(2), null);
            contentService.Save(content, Constants.Security.SuperUserId);

            var parent = contentService.GetById(NodeDto.NodeIdSeed + 2);
            var parentPublished = contentService.SaveAndPublish(parent, userId: Constants.Security.SuperUserId);//Publish root Home node to enable publishing of 'NodeDto.NodeIdSeed + 3'

            // Act
            var published = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

            // Assert
            Assert.That(parentPublished.Success, Is.True);
            Assert.That(published.Success, Is.False);
            Assert.That(content.Published, Is.False);
            Assert.AreEqual(PublishResultType.FailedPublishAwaitingRelease, published.Result);
        }

        // V9 - Tests.Integration
        [Test]
        public void Failed_Publish_Should_Not_Update_Edited_State_When_Edited_True()
        {
            const int rootNodeId = NodeDto.NodeIdSeed + 2;

            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(rootNodeId);
            contentService.SaveAndPublish(content);

            content.Properties[0].SetValue("Foo", culture: string.Empty);
            content.ContentSchedule.Add(DateTime.Now.AddHours(2), null);
            contentService.Save(content);

            // Act
            var result = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.Success);
                Assert.IsTrue(result.Content.Published);
                Assert.AreEqual(PublishResultType.FailedPublishAwaitingRelease, result.Result);

                // We changed property data
                Assert.IsTrue(result.Content.Edited, "result.Content.Edited");
            });
        }

        // V9 - Tests.Integration
        [Test]
        public void Failed_Publish_Should_Not_Update_Edited_State_When_Edited_False()
        {
            const int rootNodeId = NodeDto.NodeIdSeed + 2;

            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(rootNodeId);
            contentService.SaveAndPublish(content);

            content.ContentSchedule.Add(DateTime.Now.AddHours(2), null);
            contentService.Save(content);

            // Act
            var result = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.Success);
                Assert.IsTrue(result.Content.Published);
                Assert.AreEqual(PublishResultType.FailedPublishAwaitingRelease, result.Result);

                // We didn't change any property data
                Assert.IsFalse(result.Content.Edited, "result.Content.Edited");
            });
        }

        [Test]
        public void Cannot_Publish_Culture_Awaiting_Release()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateBasicContent(contentType);
            content.SetCultureName("Hello", "en-US");
            content.ContentSchedule.Add("en-US", DateTime.Now.AddHours(2), null);
            ServiceContext.ContentService.Save(content);

            var published = ServiceContext.ContentService.SaveAndPublish(content, "en-US", Constants.Security.SuperUserId);

            Assert.IsFalse(published.Success);
            Assert.AreEqual(PublishResultType.FailedPublishCultureAwaitingRelease, published.Result);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Cannot_Publish_Content_Where_Parent_Is_Unpublished()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.Create("Subpage with Unpublished Parent", NodeDto.NodeIdSeed + 2, "umbTextpage", Constants.Security.SuperUserId);
            contentService.Save(content, Constants.Security.SuperUserId);

            // Act
            var published = contentService.SaveAndPublishBranch(content, true);

            // Assert
            Assert.That(published.All(x => x.Success), Is.False);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Cannot_Publish_Trashed_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 6);

            // Act
            var published = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

            // Assert
            Assert.That(published.Success, Is.False);
            Assert.That(content.Published, Is.False);
            Assert.That(content.Trashed, Is.True);
        }

        [Test]
        public void Can_Save_And_Publish_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.Create("Home US", - 1, "umbTextpage", Constants.Security.SuperUserId);
            content.SetValue("author", "Barack Obama");

            // Act
            var published = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

            // Assert
            Assert.That(content.HasIdentity, Is.True);
            Assert.That(content.Published, Is.True);
            Assert.IsTrue(published.Success);
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
            var content = contentService.Create("Home US", Constants.System.Root, "umbTextpage", Constants.Security.SuperUserId);
            content.SetValue("author", "Barack Obama");

            // Act
            var published = contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);
            var childContent = contentService.Create("Child", content.Id, "umbTextpage", Constants.Security.SuperUserId);
            // Reset all identity properties
            childContent.Id = 0;
            childContent.Path = null;
            ((Content)childContent).ResetIdentity();
            var childPublished = contentService.SaveAndPublish(childContent, userId: Constants.Security.SuperUserId);

            // Assert
            Assert.That(content.HasIdentity, Is.True);
            Assert.That(content.Published, Is.True);
            Assert.That(childContent.HasIdentity, Is.True);
            Assert.That(childContent.Published, Is.True);
            Assert.That(published.Success, Is.True);
            Assert.That(childPublished.Success, Is.True);
        }

        [Test]
        public void Can_Get_Published_Descendant_Versions()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            var root = contentService.GetById(NodeDto.NodeIdSeed + 2);
            var rootPublished = contentService.SaveAndPublish(root);

            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);
            content.Properties["title"].SetValue(content.Properties["title"].GetValue() + " Published");
            var contentPublished = contentService.SaveAndPublish(content);
            var publishedVersion = content.VersionId;

            content.Properties["title"].SetValue(content.Properties["title"].GetValue() + " Saved");
            contentService.Save(content);
            Assert.AreEqual(publishedVersion, content.VersionId);

            // Act
            var publishedDescendants = ((ContentService) contentService).GetPublishedDescendants(root).ToList();
            Assert.AreNotEqual(0, publishedDescendants.Count);

            // Assert
            Assert.IsTrue(rootPublished.Success);
            Assert.IsTrue(contentPublished.Success);

            //Console.WriteLine(publishedVersion);
            //foreach (var d in publishedDescendants) Console.WriteLine(d.Version);
            Assert.IsTrue(publishedDescendants.Any(x => x.VersionId == publishedVersion));

            //Ensure that the published content version has the correct property value and is marked as published
            var publishedContentVersion = publishedDescendants.First(x => x.VersionId == publishedVersion);
            Assert.That(publishedContentVersion.Published, Is.True);
            Assert.That(publishedContentVersion.Properties["title"].GetValue(published: true), Contains.Substring("Published"));

            // and has the correct draft properties
            Assert.That(publishedContentVersion.Properties["title"].GetValue(), Contains.Substring("Saved"));

            //Ensure that the latest version of the content is ok
            var currentContent = contentService.GetById(NodeDto.NodeIdSeed + 4);
            Assert.That(currentContent.Published, Is.True);
            Assert.That(currentContent.Properties["title"].GetValue(published: true), Contains.Substring("Published"));
            Assert.That(currentContent.Properties["title"].GetValue(), Contains.Substring("Saved"));
            Assert.That(currentContent.VersionId, Is.EqualTo(publishedContentVersion.VersionId));
        }

        [Test]
        public void Can_Save_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.Create("Home US", - 1, "umbTextpage", Constants.Security.SuperUserId);
            content.SetValue("author", "Barack Obama");

            // Act
            contentService.Save(content, Constants.Security.SuperUserId);

            // Assert
            Assert.That(content.HasIdentity, Is.True);
        }

        [Test]
        public void Can_Update_Content_Property_Values()
        {
            IContentType contentType = MockedContentTypes.CreateSimpleContentType();
            ServiceContext.ContentTypeService.Save(contentType);
            IContent content = MockedContent.CreateSimpleContent(contentType, "hello");
            content.SetValue("title", "title of mine");
            content.SetValue("bodyText", "hello world");
            ServiceContext.ContentService.SaveAndPublish(content);

            // re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            content.SetValue("title", "another title of mine");          // Change a value
            content.SetValue("bodyText", null);                          // Clear a value
            content.SetValue("author", "new author");                    // Add a value
            ServiceContext.ContentService.SaveAndPublish(content);

            // re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.AreEqual("another title of mine", content.GetValue("title"));
            Assert.IsNull(content.GetValue("bodyText"));
            Assert.AreEqual("new author", content.GetValue("author"));

            content.SetValue("title", "new title");
            content.SetValue("bodyText", "new body text");
            content.SetValue("author", "new author text");
            ServiceContext.ContentService.Save(content);                // new non-published version

            // re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            content.SetValue("title", null);                            // Clear a value
            content.SetValue("bodyText", null);                         // Clear a value
            ServiceContext.ContentService.Save(content);                // saving non-published version

            // re-get
            content = ServiceContext.ContentService.GetById(content.Id);
            Assert.IsNull(content.GetValue("title"));                   // Test clearing the value worked with the non-published version
            Assert.IsNull(content.GetValue("bodyText"));
            Assert.AreEqual("new author text", content.GetValue("author"));

            // make sure that the published version remained the same
            var publishedContent = ServiceContext.ContentService.GetVersion(content.PublishedVersionId);
            Assert.AreEqual("another title of mine", publishedContent.GetValue("title"));
            Assert.IsNull(publishedContent.GetValue("bodyText"));
            Assert.AreEqual("new author", publishedContent.GetValue("author"));
        }

        [Test]
        public void Can_Bulk_Save_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = contentTypeService.Get("umbTextpage");
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Subpage 1", NodeDto.NodeIdSeed + 2);
            Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Subpage 2", NodeDto.NodeIdSeed + 2);
            var list = new List<IContent> {subpage, subpage2};

            // Act
            contentService.Save(list, Constants.Security.SuperUserId);

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
            contentService.Save(hierarchy, Constants.Security.SuperUserId);

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
            var contentType = contentTypeService.Get("umbTextpage");

            // Act
            contentService.DeleteOfType(contentType.Id);
            var rootContent = contentService.GetRootContent();
            var contents = contentService.GetPagedOfType(contentType.Id, 0, int.MaxValue, out var _, null);

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
            contentService.Delete(content, Constants.Security.SuperUserId);
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
            contentService.MoveToRecycleBin(content, Constants.Security.SuperUserId);

            // Assert
            Assert.That(content.ParentId, Is.EqualTo(-20));
            Assert.That(content.Trashed, Is.True);
        }

        [Test]
        public void Can_Move_Content_Structure_To_RecycleBin_And_Empty_RecycleBin()
        {
            var contentService = ServiceContext.ContentService;
            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");

            var subsubpage = MockedContent.CreateSimpleContent(contentType, "Text Page 3", NodeDto.NodeIdSeed + 3);
            contentService.Save(subsubpage, Constants.Security.SuperUserId);

            var content = contentService.GetById(NodeDto.NodeIdSeed + 2);
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            var descendants = new List<IContent>();
            while(page * pageSize < total)
                descendants.AddRange(contentService.GetPagedDescendants(content.Id, page++, pageSize, out total));

            Assert.AreNotEqual(-20, content.ParentId);
            Assert.IsFalse(content.Trashed);
            Assert.AreEqual(4, descendants.Count);
            Assert.IsFalse(descendants.Any(x => x.Path.StartsWith("-1,-20,")));
            Assert.IsFalse(descendants.Any(x => x.Trashed));

            contentService.MoveToRecycleBin(content, Constants.Security.SuperUserId);

            descendants.Clear();
            page = 0;
            while (page * pageSize < total)
                descendants.AddRange(contentService.GetPagedDescendants(content.Id, page++, pageSize, out total));

            Assert.AreEqual(-20, content.ParentId);
            Assert.IsTrue(content.Trashed);
            Assert.AreEqual(4, descendants.Count);
            Assert.IsTrue(descendants.All(x => x.Path.StartsWith("-1,-20,")));
            Assert.True(descendants.All(x => x.Trashed));

            contentService.EmptyRecycleBin(Constants.Security.SuperUserId);
            var trashed = contentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var _).ToList();
            Assert.IsEmpty(trashed);
        }

        [Test]
        public void Can_Empty_RecycleBin()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            // Act
            contentService.EmptyRecycleBin(Constants.Security.SuperUserId);
            var contents = contentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var _).ToList();

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
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType);

            var parentPage = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(parentPage);

            var childPage = MockedContent.CreateSimpleContent(contentType, "child", parentPage);
            ServiceContext.ContentService.Save(childPage);
            //assign explicit permissions to the child
            ServiceContext.ContentService.SetPermission(childPage, 'A', new[] { userGroup.Id });

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
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType);

            var parentPage = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(parentPage);
            ServiceContext.ContentService.SetPermission(parentPage, 'A', new[] { userGroup.Id });

            var childPage1 = MockedContent.CreateSimpleContent(contentType, "child1", parentPage);
            ServiceContext.ContentService.Save(childPage1);
            var childPage2 = MockedContent.CreateSimpleContent(contentType, "child2", childPage1);
            ServiceContext.ContentService.Save(childPage2);
            var childPage3 = MockedContent.CreateSimpleContent(contentType, "child3", childPage2);
            ServiceContext.ContentService.Save(childPage3);

            //Verify that the children have the inherited permissions
            var descendants = new List<IContent>();
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while(page * pageSize < total)
                descendants.AddRange(ServiceContext.ContentService.GetPagedDescendants(parentPage.Id, page++, pageSize, out total));
            Assert.AreEqual(3, descendants.Count);

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
            ServiceContext.ContentService.SetPermission(parentPage2, 'B', new[] { userGroup.Id });

            //Now copy, what should happen is the child pages will now have permissions inherited from the new parent
            var copy = ServiceContext.ContentService.Copy(childPage1, parentPage2.Id, false, true);

            descendants.Clear();
            page = 0;
            while (page * pageSize < total)
                descendants.AddRange(ServiceContext.ContentService.GetPagedDescendants(parentPage2.Id, page++, pageSize, out total));
            Assert.AreEqual(3, descendants.Count);

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
            ServiceContext.ContentTypeService.Save(contentType, Constants.Security.SuperUserId);

            object obj =
                new
                {
                    tags = "[\"Hello\",\"World\"]"
                };
            var content1 = MockedContent.CreateBasicContent(contentType);
            content1.PropertyValues(obj);
            content1.ResetDirtyProperties(false);
            ServiceContext.ContentService.Save(content1, Constants.Security.SuperUserId);
            Assert.IsTrue(ServiceContext.ContentService.SaveAndPublish(content1, userId: 0).Success);
            var content2 = MockedContent.CreateBasicContent(contentType);
            content2.PropertyValues(obj);
            content2.ResetDirtyProperties(false);
            ServiceContext.ContentService.Save(content2, Constants.Security.SuperUserId);
            Assert.IsTrue(ServiceContext.ContentService.SaveAndPublish(content2, userId: 0).Success);

            var editorGroup = ServiceContext.UserService.GetUserGroupByAlias(Constants.Security.EditorGroupAlias);
            editorGroup.StartContentId = content1.Id;
            ServiceContext.UserService.Save(editorGroup);

            var admin = ServiceContext.UserService.GetUserById(Constants.Security.SuperUserId);
            admin.StartContentIds = new[] {content1.Id};
            ServiceContext.UserService.Save(admin);

            ServiceContext.RelationService.Save(new RelationType("test", "test", false, Constants.ObjectTypes.Document, Constants.ObjectTypes.Document));
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

            var user = ServiceContext.UserService.GetUserById(Constants.Security.SuperUserId);
            var userGroup = ServiceContext.UserService.GetUserGroupByAlias(user.Groups.First().Alias);
            Assert.IsNotNull(ServiceContext.NotificationService.CreateNotification(user, content1, "X"));

            ServiceContext.ContentService.SetPermission(content1, 'A', new[] { userGroup.Id });

            Assert.IsTrue(ServiceContext.DomainService.Save(new UmbracoDomain("www.test.com", "en-AU")
            {
                RootContentId = content1.Id
            }).Success);

            // Act
            ServiceContext.ContentService.MoveToRecycleBin(content1);
            ServiceContext.ContentService.EmptyRecycleBin(Constants.Security.SuperUserId);
            var contents = ServiceContext.ContentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var _).ToList();

            // Assert
            Assert.That(contents.Any(), Is.False);
        }

        [Test]
        public void Can_Move_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 5);

            // Act - moving out of recycle bin
            contentService.Move(content, NodeDto.NodeIdSeed + 2);

            // Assert
            Assert.That(content.ParentId, Is.EqualTo(NodeDto.NodeIdSeed + 2));
            Assert.That(content.Trashed, Is.False);
            Assert.That(content.Published, Is.False);
        }

        [Test]
        public void Can_Copy_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var temp = contentService.GetById(NodeDto.NodeIdSeed + 4);

            // Act
            var copy = contentService.Copy(temp, temp.ParentId, false, Constants.Security.SuperUserId);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);

            // Assert
            Assert.That(copy, Is.Not.Null);
            Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
            Assert.AreNotSame(content, copy);
            foreach (var property in copy.Properties)
            {
                Assert.AreEqual(property.GetValue(), content.Properties[property.Alias].GetValue());
            }
            //Assert.AreNotEqual(content.Name, copy.Name);
        }

        [Test]
        public void Can_Copy_And_Modify_Content_With_Events()
        {
            // see https://github.com/umbraco/Umbraco-CMS/issues/5513

            TypedEventHandler<IContentService, CopyEventArgs<IContent>> copying = (sender, args) =>
            {
                args.Copy.SetValue("title", "1");
                args.Original.SetValue("title", "2");                
            };

            TypedEventHandler<IContentService, CopyEventArgs<IContent>> copied = (sender, args) =>
            {
                var copyVal = args.Copy.GetValue<string>("title");
                var origVal = args.Original.GetValue<string>("title");

                Assert.AreEqual("1", copyVal);
                Assert.AreEqual("2", origVal);
            };

            try
            {                
                var contentService = ServiceContext.ContentService;
                
                ContentService.Copying += copying;
                ContentService.Copied += copied;

                var contentType = MockedContentTypes.CreateSimpleContentType();
                ServiceContext.ContentTypeService.Save(contentType);
                var content = MockedContent.CreateSimpleContent(contentType);
                content.SetValue("title", "New Value");
                contentService.Save(content);

                var copy = contentService.Copy(content, content.ParentId, false, Constants.Security.SuperUserId);
                Assert.AreEqual("1", copy.GetValue("title"));
            }
            finally
            {
                ContentService.Copying -= copying;
                ContentService.Copied -= copied;
            }
        }

        [Test]
        public void Can_Copy_Recursive()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var temp = contentService.GetById(NodeDto.NodeIdSeed + 2);
            Assert.AreEqual("Home", temp.Name);
            Assert.AreEqual(3, contentService.CountChildren(temp.Id));

            // Act
            var copy = contentService.Copy(temp, temp.ParentId, false, true, Constants.Security.SuperUserId);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 2);

            // Assert
            Assert.That(copy, Is.Not.Null);
            Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
            Assert.AreNotSame(content, copy);
            Assert.AreEqual(3, contentService.CountChildren(copy.Id));

            var child = contentService.GetById(NodeDto.NodeIdSeed + 3);
            var childCopy = contentService.GetPagedChildren(copy.Id, 0, 500, out var total).First();
            Assert.AreEqual(childCopy.Name, child.Name);
            Assert.AreNotEqual(childCopy.Id, child.Id);
            Assert.AreNotEqual(childCopy.Key, child.Key);
        }

        [Test]
        public void Can_Copy_NonRecursive()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var temp = contentService.GetById(NodeDto.NodeIdSeed + 2);
            Assert.AreEqual("Home", temp.Name);
            Assert.AreEqual(3, contentService.CountChildren(temp.Id));

            // Act
            var copy = contentService.Copy(temp, temp.ParentId, false, false, Constants.Security.SuperUserId);
            var content = contentService.GetById(NodeDto.NodeIdSeed + 2);

            // Assert
            Assert.That(copy, Is.Not.Null);
            Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
            Assert.AreNotSame(content, copy);
            Assert.AreEqual(0, contentService.CountChildren(copy.Id));
        }

        [Test]
        public void Can_Copy_Content_With_Tags()
        {
            const string propAlias = "tags";

            var contentService = ServiceContext.ContentService;

            // create a content type that has a 'tags' property
            // the property needs to support tags, else nothing works of course!
            var contentType = MockedContentTypes.CreateSimpleContentType3("umbTagsPage", "TagsPage");
            contentType.Key = new Guid("78D96D30-1354-4A1E-8450-377764200C58");
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateSimpleContent(contentType, "Simple Tags Page", Constants.System.Root);
            content.AssignTags(propAlias, new[] {"hello", "world"});
            contentService.Save(content);

            // value has been set but no tags have been created (not published)
            Assert.AreEqual("[\"hello\",\"world\"]", content.GetValue(propAlias));
            var contentTags = ServiceContext.TagService.GetTagsForEntity(content.Id).ToArray();
            Assert.AreEqual(0, contentTags.Length);

            // reloading the content yields the same result
            content = (Content) contentService.GetById(content.Id);
            Assert.AreEqual("[\"hello\",\"world\"]", content.GetValue(propAlias));
            contentTags = ServiceContext.TagService.GetTagsForEntity(content.Id).ToArray();
            Assert.AreEqual(0, contentTags.Length);

            // publish
            contentService.SaveAndPublish(content);

            // now tags have been set (published)
            Assert.AreEqual("[\"hello\",\"world\"]", content.GetValue(propAlias));
            contentTags = ServiceContext.TagService.GetTagsForEntity(content.Id).ToArray();
            Assert.AreEqual(2, contentTags.Length);

            // copy
            var copy = contentService.Copy(content, content.ParentId, false);

            // copy is not published, so property has value, but no tags have been created
            Assert.AreEqual("[\"hello\",\"world\"]", copy.GetValue(propAlias));
            var copiedTags = ServiceContext.TagService.GetTagsForEntity(copy.Id).ToArray();
            Assert.AreEqual(0, copiedTags.Length);

            // publish
            contentService.SaveAndPublish(copy);

            // now tags have been set (published)
            copiedTags = ServiceContext.TagService.GetTagsForEntity(copy.Id).ToArray();

            Assert.AreEqual(2, copiedTags.Length);
            Assert.AreEqual("hello", copiedTags[0].Text);
            Assert.AreEqual("world", copiedTags[1].Text);
        }

        [Test]
        public void Can_Rollback_Version_On_Content()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;

            var parent = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 2);
            Assert.IsFalse(parent.Published);
            ServiceContext.ContentService.SaveAndPublish(parent); // publishing parent, so Text Page 2 can be updated.

            var content = contentService.GetById(NodeDto.NodeIdSeed + 4);
            Assert.IsFalse(content.Published);

            var versions = contentService.GetVersions(NodeDto.NodeIdSeed + 4).ToList();
            Assert.AreEqual(1, versions.Count);

            var version1 = content.VersionId;

            content.Name = "Text Page 2 Updated";
            content.SetValue("author", "Francis Doe");

            // non published = edited
            Assert.IsTrue(content.Edited);

            contentService.SaveAndPublish(content); // new version
            var version2 = content.VersionId;
            Assert.AreNotEqual(version1, version2);

            Assert.IsTrue(content.Published);
            Assert.IsFalse(content.Edited);
            Assert.AreEqual("Francis Doe", contentService.GetById(content.Id).GetValue<string>("author")); // version2 author is Francis

            Assert.AreEqual("Text Page 2 Updated", content.Name);
            Assert.AreEqual("Text Page 2 Updated", content.PublishName);

            content.Name = "Text Page 2 ReUpdated";
            content.SetValue("author", "Jane Doe");

            // is not actually 'edited' until changes have been saved
            Assert.IsFalse(content.Edited);
            contentService.Save(content); // just save changes
            Assert.IsTrue(content.Edited);

            Assert.AreEqual("Text Page 2 ReUpdated", content.Name);
            Assert.AreEqual("Text Page 2 Updated", content.PublishName);

            content.Name = "Text Page 2 ReReUpdated";

            contentService.SaveAndPublish(content); // new version
            var version3 = content.VersionId;
            Assert.AreNotEqual(version2, version3);

            Assert.IsTrue(content.Published);
            Assert.IsFalse(content.Edited);
            Assert.AreEqual("Jane Doe", contentService.GetById(content.Id).GetValue<string>("author")); // version3 author is Jane

            Assert.AreEqual("Text Page 2 ReReUpdated", content.Name);
            Assert.AreEqual("Text Page 2 ReReUpdated", content.PublishName);

            // here we have
            // version1, first published version
            // version2, second published version
            // version3, third and current published version

            // rollback all values to version1
            var rollback = contentService.GetById(NodeDto.NodeIdSeed + 4);
            var rollto = contentService.GetVersion(version1);
            rollback.CopyFrom(rollto);
            rollback.Name = rollto.Name; // must do it explicitly
            contentService.Save(rollback);

            Assert.IsNotNull(rollback);
            Assert.IsTrue(rollback.Published);
            Assert.IsTrue(rollback.Edited);
            Assert.AreEqual("Francis Doe", contentService.GetById(content.Id).GetValue<string>("author")); // author is now Francis again
            Assert.AreEqual(version3, rollback.VersionId); // same version but with edits

            // props and name have rolled back
            Assert.AreEqual("Francis Doe", rollback.GetValue<string>("author"));
            Assert.AreEqual("Text Page 2 Updated", rollback.Name);

            // published props and name are still there
            Assert.AreEqual("Jane Doe", rollback.GetValue<string>("author", published: true));
            Assert.AreEqual("Text Page 2 ReReUpdated", rollback.PublishName);

            // rollback all values to current version
            // special because... current has edits... this really is equivalent to rolling back to version2
            var rollback2 = contentService.GetById(NodeDto.NodeIdSeed + 4);
            var rollto2 = contentService.GetVersion(version3);
            rollback2.CopyFrom(rollto2);
            rollback2.Name = rollto2.PublishName; // must do it explicitely AND must pick the publish one!
            contentService.Save(rollback2);

            Assert.IsTrue(rollback2.Published);
            Assert.IsTrue(rollback2.Edited); // Still edited, change of behaviour

            Assert.AreEqual("Jane Doe", rollback2.GetValue<string>("author"));
            Assert.AreEqual("Text Page 2 ReReUpdated", rollback2.Name);

            // test rollback to self, again
            content = contentService.GetById(content.Id);
            Assert.AreEqual("Text Page 2 ReReUpdated", content.Name);
            Assert.AreEqual("Jane Doe", content.GetValue<string>("author"));
            contentService.SaveAndPublish(content);
            Assert.IsFalse(content.Edited);
            content.Name = "Xxx";
            content.SetValue("author", "Bob Doe");
            contentService.Save(content);
            Assert.IsTrue(content.Edited);
            rollto = contentService.GetVersion(content.VersionId);
            content.CopyFrom(rollto);
            content.Name = rollto.PublishName; // must do it explicitely AND must pick the publish one!
            contentService.Save(content);
            Assert.IsTrue(content.Edited); // Still edited, change of behaviour
            Assert.AreEqual("Text Page 2 ReReUpdated", content.Name);
            Assert.AreEqual("Jane Doe", content.GetValue("author"));
        }

        [Test]
        public void Can_Rollback_Version_On_Multilingual()
        {
            var langFr = new Language("fr");
            var langDa = new Language("da");
            ServiceContext.LocalizationService.Save(langFr);
            ServiceContext.LocalizationService.Save(langDa);

            var contentType = MockedContentTypes.CreateSimpleContentType("multi", "Multi");
            contentType.Key = new Guid("45FF9A70-9C5F-448D-A476-DCD23566BBF8");
            contentType.Variations = ContentVariation.Culture;
            var p1 = contentType.PropertyTypes.First();
            p1.Variations = ContentVariation.Culture;
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
            ServiceContext.ContentTypeService.Save(contentType);

            var page = new Content("Page", Constants.System.Root, contentType)
            {
                Level = 1,
                SortOrder = 1,
                CreatorId = 0,
                WriterId = 0,
                Key = new Guid("D7B84CC9-14AE-4D92-A042-023767AD3304")
            };

            page.SetCultureName("fr1", "fr");
            page.SetCultureName("da1", "da");
            ServiceContext.ContentService.Save(page);
            var versionId0 = page.VersionId;

            page.SetValue(p1.Alias, "v1fr", "fr");
            page.SetValue(p1.Alias, "v1da", "da");
            ServiceContext.ContentService.SaveAndPublish(page);
            var versionId1 = page.VersionId;

            Thread.Sleep(250);

            page.SetCultureName("fr2", "fr");
            page.SetValue(p1.Alias, "v2fr", "fr");
            ServiceContext.ContentService.SaveAndPublish(page, "fr");
            var versionId2 = page.VersionId;

            Thread.Sleep(250);

            page.SetCultureName("da2", "da");
            page.SetValue(p1.Alias, "v2da", "da");
            ServiceContext.ContentService.SaveAndPublish(page, "da");
            var versionId3 = page.VersionId;

            Thread.Sleep(250);

            page.SetCultureName("fr3", "fr");
            page.SetCultureName("da3", "da");
            page.SetValue(p1.Alias, "v3fr", "fr");
            page.SetValue(p1.Alias, "v3da", "da");
            ServiceContext.ContentService.SaveAndPublish(page);
            var versionId4 = page.VersionId;

            // now get all versions

            var versions = ServiceContext.ContentService.GetVersions(page.Id).ToArray();

            Assert.AreEqual(5, versions.Length);

            // current version
            Assert.AreEqual(versionId4, versions[0].VersionId);
            Assert.AreEqual(versionId3, versions[0].PublishedVersionId);
            // published version
            Assert.AreEqual(versionId3, versions[1].VersionId);
            Assert.AreEqual(versionId3, versions[1].PublishedVersionId);
            // previous version
            Assert.AreEqual(versionId2, versions[2].VersionId);
            Assert.AreEqual(versionId3, versions[2].PublishedVersionId);
            // previous version
            Assert.AreEqual(versionId1, versions[3].VersionId);
            Assert.AreEqual(versionId3, versions[3].PublishedVersionId);
            // previous version
            Assert.AreEqual(versionId0, versions[4].VersionId);
            Assert.AreEqual(versionId3, versions[4].PublishedVersionId);

            Assert.AreEqual("fr3", versions[4].GetPublishName("fr"));
            Assert.AreEqual("fr3", versions[3].GetPublishName("fr"));
            Assert.AreEqual("fr3", versions[2].GetPublishName("fr"));
            Assert.AreEqual("fr3", versions[1].GetPublishName("fr"));
            Assert.AreEqual("fr3", versions[0].GetPublishName("fr"));

            Assert.AreEqual("fr1", versions[4].GetCultureName("fr"));
            Assert.AreEqual("fr2", versions[3].GetCultureName("fr"));
            Assert.AreEqual("fr2", versions[2].GetCultureName("fr"));
            Assert.AreEqual("fr3", versions[1].GetCultureName("fr"));
            Assert.AreEqual("fr3", versions[0].GetCultureName("fr"));

            Assert.AreEqual("da3", versions[4].GetPublishName("da"));
            Assert.AreEqual("da3", versions[3].GetPublishName("da"));
            Assert.AreEqual("da3", versions[2].GetPublishName("da"));
            Assert.AreEqual("da3", versions[1].GetPublishName("da"));
            Assert.AreEqual("da3", versions[0].GetPublishName("da"));

            Assert.AreEqual("da1", versions[4].GetCultureName("da"));
            Assert.AreEqual("da1", versions[3].GetCultureName("da"));
            Assert.AreEqual("da2", versions[2].GetCultureName("da"));
            Assert.AreEqual("da3", versions[1].GetCultureName("da"));
            Assert.AreEqual("da3", versions[0].GetCultureName("da"));

            // all versions have the same publish infos
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(versions[0].PublishDate, versions[i].PublishDate);
                Assert.AreEqual(versions[0].GetPublishDate("fr"), versions[i].GetPublishDate("fr"));
                Assert.AreEqual(versions[0].GetPublishDate("da"), versions[i].GetPublishDate("da"));
            }

            for (var i = 0; i < 5; i++)
            {
                Console.Write("[{0}] ", i);
                Console.WriteLine(versions[i].UpdateDate.ToString("O").Substring(11));
                Console.WriteLine("    fr: {0}", versions[i].GetUpdateDate("fr")?.ToString("O").Substring(11));
                Console.WriteLine("    da: {0}", versions[i].GetUpdateDate("da")?.ToString("O").Substring(11));
            }
            Console.WriteLine("-");

            // for all previous versions, UpdateDate is the published date

            Assert.AreEqual(versions[4].UpdateDate, versions[4].GetUpdateDate("fr"));
            Assert.AreEqual(versions[4].UpdateDate, versions[4].GetUpdateDate("da"));

            Assert.AreEqual(versions[3].UpdateDate, versions[3].GetUpdateDate("fr"));
            Assert.AreEqual(versions[4].UpdateDate, versions[3].GetUpdateDate("da"));

            Assert.AreEqual(versions[3].UpdateDate, versions[2].GetUpdateDate("fr"));
            Assert.AreEqual(versions[2].UpdateDate, versions[2].GetUpdateDate("da"));

            // for the published version, UpdateDate is the published date

            Assert.AreEqual(versions[1].UpdateDate, versions[1].GetUpdateDate("fr"));
            Assert.AreEqual(versions[1].UpdateDate, versions[1].GetUpdateDate("da"));
            Assert.AreEqual(versions[1].PublishDate, versions[1].UpdateDate);

            // for the current version, things are different
            // UpdateDate is the date it was last saved

            Assert.AreEqual(versions[0].UpdateDate, versions[0].GetUpdateDate("fr"));
            Assert.AreEqual(versions[0].UpdateDate, versions[0].GetUpdateDate("da"));

            // so if we save again...

            page.SetCultureName("fr4", "fr");
            //page.SetCultureName("da4", "da");
            page.SetValue(p1.Alias, "v4fr", "fr");
            page.SetValue(p1.Alias, "v4da", "da");
            ServiceContext.ContentService.Save(page);
            var versionId5 = page.VersionId;

            versions = ServiceContext.ContentService.GetVersions(page.Id).ToArray();

            // we just update the current version
            Assert.AreEqual(5, versions.Length);
            Assert.AreEqual(versionId4, versionId5);

            for (var i = 0; i < 5; i++)
            {
                Console.Write("[{0}] ", i);
                Console.WriteLine(versions[i].UpdateDate.ToString("O").Substring(11));
                Console.WriteLine("    fr: {0}", versions[i].GetUpdateDate("fr")?.ToString("O").Substring(11));
                Console.WriteLine("    da: {0}", versions[i].GetUpdateDate("da")?.ToString("O").Substring(11));
            }
            Console.WriteLine("-");

            var versionsSlim = ServiceContext.ContentService.GetVersionsSlim(page.Id, 0, 50).ToArray();
            Assert.AreEqual(5, versionsSlim.Length);

            for (var i = 0; i < 5; i++)
            {
                Console.Write("[{0}] ", i);
                Console.WriteLine(versionsSlim[i].UpdateDate.ToString("O").Substring(11));
                Console.WriteLine("    fr: {0}", versionsSlim[i].GetUpdateDate("fr")?.ToString("O").Substring(11));
                Console.WriteLine("    da: {0}", versionsSlim[i].GetUpdateDate("da")?.ToString("O").Substring(11));
            }
            Console.WriteLine("-");

            // what we do in the controller to get rollback versions
            var versionsSlimFr = versionsSlim.Where(x => x.UpdateDate == x.GetUpdateDate("fr")).ToArray();
            Assert.AreEqual(3, versionsSlimFr.Length);

            // alas, at the moment we do *not* properly track 'dirty' for cultures, meaning
            // that we cannot synchronize dates the way we do with publish dates - and so this
            // would fail - the version UpdateDate is greater than the cultures'.
            //Assert.AreEqual(versions[0].UpdateDate, versions[0].GetUpdateDate("fr"));
            //Assert.AreEqual(versions[0].UpdateDate, versions[0].GetUpdateDate("da"));

            // now roll french back to its very first version
            page.CopyFrom(versions[4], "fr"); // only the pure FR values
            page.CopyFrom(versions[4], null); // so, must explicitly do the INVARIANT values too
            page.SetCultureName(versions[4].GetPublishName("fr"), "fr");
            ServiceContext.ContentService.Save(page);

            // and voila, rolled back!
            Assert.AreEqual(versions[4].GetPublishName("fr"), page.GetCultureName("fr"));
            Assert.AreEqual(versions[4].GetValue(p1.Alias, "fr"), page.GetValue(p1.Alias, "fr"));

            // note that rolling back invariant values means we also rolled back... DA... at least partially
            // bah?
        }

        [Test]
        public void Can_Save_Lazy_Content()
        {
            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");
            var root = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 2);

            var c = new Lazy<IContent>(() => MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Page", root.Id));
            var c2 = new Lazy<IContent>(() => MockedContent.CreateSimpleContent(contentType, "Hierarchy Simple Text Subpage", c.Value.Id));
            var list = new List<Lazy<IContent>> {c, c2};

            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider, out _);

                foreach (var content in list)
                {
                    repository.Save(content.Value);
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
        public void Can_Verify_Property_Types_On_Content()
        {
            // Arrange
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateAllTypesContentType("allDataTypes", "All DataTypes");
            contentTypeService.Save(contentType);
            var contentService = ServiceContext.ContentService;
            var content = MockedContent.CreateAllTypesContent(contentType, "Random Content", Constants.System.Root);
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
            Assert.That(sut.GetValue("mediapicker3"), Is.EqualTo("[{\"key\": \"8f78ce9e-8fe0-4500-a52d-4c4f35566ba9\",\"mediaKey\": \"44CB39C8-01E5-45EB-9CF8-E70AAF2D1691\",\"crops\": [],\"focalPoint\": {\"left\": 0.5,\"top\": 0.5}}]"));
            Assert.That(sut.GetValue<Udi>("memberPicker"), Is.EqualTo(Udi.Create(Constants.UdiEntityType.Member, new Guid("9A50A448-59C0-4D42-8F93-4F1D55B0F47D"))));
            Assert.That(sut.GetValue<string>("multiUrlPicker"), Is.EqualTo("[{\"name\":\"https://test.com\",\"url\":\"https://test.com\"}]"));
            Assert.That(sut.GetValue<string>("tags"), Is.EqualTo("this,is,tags"));
        }

        [Test]
        public void Can_Delete_Previous_Versions_Not_Latest()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var content = contentService.GetById(NodeDto.NodeIdSeed + 5);
            var version = content.VersionId;

            // Act
            contentService.DeleteVersion(NodeDto.NodeIdSeed + 5, version, true, Constants.Security.SuperUserId);
            var sut = contentService.GetById(NodeDto.NodeIdSeed + 5);

            // Assert
            Assert.That(sut.VersionId, Is.EqualTo(version));
        }

        [Test]
        public void Ensure_Content_Xml_Created()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.Create("Home US", Constants.System.Root, "umbTextpage", Constants.Security.SuperUserId);
            content.SetValue("author", "Barack Obama");

            contentService.Save(content);

            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.IsFalse(scope.Database.Exists<ContentXmlDto>(content.Id));
            }

            contentService.SaveAndPublish(content);

            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.IsTrue(scope.Database.Exists<ContentXmlDto>(content.Id));
            }
        }

        [Test]
        public void Ensure_Preview_Xml_Created()
        {
            var contentService = ServiceContext.ContentService;
            var content = contentService.Create("Home US", Constants.System.Root, "umbTextpage", Constants.Security.SuperUserId);
            content.SetValue("author", "Barack Obama");

            contentService.Save(content);

            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.IsTrue(scope.Database.SingleOrDefault<PreviewXmlDto>("WHERE nodeId=@nodeId", new{nodeId = content.Id}) != null);
            }
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
            var entities = service.GetPagedChildren(Constants.System.Root, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedChildren(Constants.System.Root, 1, 6, out total).ToArray();
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
            var entities = service.GetPagedChildren(Constants.System.Root, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedChildren(Constants.System.Root, 1, 6, out total).ToArray();
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

        [Test]
        public void PublishingTest()
        {
            var contentType = new ContentType(Constants.System.Root)
            {
                Alias = "foo",
                Name = "Foo"
            };

            var properties = new PropertyTypeCollection(true)
            {
                new PropertyType("test", ValueStorageType.Ntext) { Alias = "title", Name = "Title", Mandatory = false, DataTypeId = -88 },
            };

            contentType.PropertyGroups.Add(new PropertyGroup(properties) { Name = "content" });

            contentType.SetDefaultTemplate(new Template("Textpage", "textpage"));
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
            ServiceContext.ContentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;
            var content = contentService.Create("foo", Constants.System.Root, "foo");
            contentService.Save(content);

            Assert.IsFalse(content.Published);
            Assert.IsTrue(content.Edited);

            content = contentService.GetById(content.Id);
            Assert.IsFalse(content.Published);
            Assert.IsTrue(content.Edited);

            content.SetValue("title", "foo");
            Assert.IsTrue(content.Edited);

            contentService.Save(content);

            Assert.IsFalse(content.Published);
            Assert.IsTrue(content.Edited);

            content = contentService.GetById(content.Id);
            Assert.IsFalse(content.Published);
            Assert.IsTrue(content.Edited);

            var versions = contentService.GetVersions(content.Id);
            Assert.AreEqual(1, versions.Count());

            // publish content
            // becomes Published, !Edited
            // creates a new version
            // can get published property values
            contentService.SaveAndPublish(content);

            Assert.IsTrue(content.Published);
            Assert.IsFalse(content.Edited);

            content = contentService.GetById(content.Id);
            Assert.IsTrue(content.Published);
            Assert.IsFalse(content.Edited);

            versions = contentService.GetVersions(content.Id);
            Assert.AreEqual(2, versions.Count());

            Assert.AreEqual("foo", content.GetValue("title", published: true));
            Assert.AreEqual("foo", content.GetValue("title"));

            // unpublish content
            // becomes !Published, Edited
            contentService.Unpublish(content);

            Assert.IsFalse(content.Published);
            Assert.IsTrue(content.Edited);

            Assert.AreEqual("foo", content.GetValue("title", published: true));
            Assert.AreEqual("foo", content.GetValue("title"));

            var vpk = ((Content) content).VersionId;
            var ppk = ((Content) content).PublishedVersionId;

            content = contentService.GetById(content.Id);
            Assert.IsFalse(content.Published);
            Assert.IsTrue(content.Edited);

            // FIXME: depending on 1 line in ContentBaseFactory.BuildEntity
            // the published infos can be gone or not
            // if gone, it's not consistent with above
            Assert.AreEqual(vpk, ((Content) content).VersionId);
            Assert.AreEqual(ppk, ((Content) content).PublishedVersionId); // still there

            // FIXME: depending on 1 line in ContentRepository.MapDtoToContent
            // the published values can be null or not
            // if null, it's not consistent with above
            //Assert.IsNull(content.GetValue("title", published:  true));
            Assert.AreEqual("foo", content.GetValue("title", published: true)); // still there
            Assert.AreEqual("foo", content.GetValue("title"));

            versions = contentService.GetVersions(content.Id);
            Assert.AreEqual(2, versions.Count());

            // ah - we have a problem here - since we're not published we don't have published values
            // and therefore we cannot "just" republish the content - we need to publish some values
            // so... that's not really an option
            //
            //contentService.SaveAndPublish(content);

            // FIXME: what shall we do of all this?
            /*
            // this basically republishes a content
            // what if it never was published?
            // what if it has changes?
            // do we want to "publish" only some variants, or the entire content?
            contentService.Publish(content);

            Assert.IsTrue(content.Published);
            Assert.IsFalse(content.Edited);

            // FIXME: should it be 2 or 3
            versions = contentService.GetVersions(content.Id);
            Assert.AreEqual(2, versions.Count());

            // FIXME: now test rollbacks
            var version = contentService.GetByVersion(content.Id); // test that it gets a version - should be GetVersion
            var previousVersion = contentService.GetVersions(content.Id).Skip(1).FirstOrDefault(); // need an optimized way to do this
            content.CopyValues(version); // copies the edited value - always
            content.Template = version.Template;
            content.Name = version.Name;
            contentService.Save(content); // this is effectively a rollback?
            contentService.Rollback(content); // just kill the method and offer options on values + template + name...
            */
        }

        [Test]
        public void Ensure_Invariant_Name()
        {
            var languageService = ServiceContext.LocalizationService;

            var langUk = new Language("en-GB") { IsDefault = true };
            var langFr = new Language("fr-FR");

            languageService.Save(langFr);
            languageService.Save(langUk);

            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = contentTypeService.Get("umbTextpage");
            contentType.Variations = ContentVariation.Culture;
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar, "prop") { Variations = ContentVariation.Culture });
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;
            var content = new Content(null, Constants.System.Root, contentType);

            content.SetCultureName("name-us", langUk.IsoCode);
            content.SetCultureName("name-fr", langFr.IsoCode);
            contentService.Save(content);

            //the name will be set to the default culture variant name
            Assert.AreEqual("name-us", content.Name);

            // FIXME: should we always sync the invariant name even on update? see EnsureInvariantNameValues
            ////updating the default culture variant name should also update the invariant name so they stay in sync
            //content.SetName("name-us-2", langUk.IsoCode);
            //contentService.Save(content);
            //Assert.AreEqual("name-us-2", content.Name);
        }

        [Test]
        public void Ensure_Unique_Culture_Names()
        {
            var languageService = ServiceContext.LocalizationService;

            var langUk = new Language("en-GB") { IsDefault = true };
            var langFr = new Language("fr-FR");

            languageService.Save(langFr);
            languageService.Save(langUk);

            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = contentTypeService.Get("umbTextpage");
            contentType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;

            var content = new Content(null, Constants.System.Root, contentType);
            content.SetCultureName("root", langUk.IsoCode);
            contentService.Save(content);

            for (var i = 0; i < 5; i++)
            {
                var child = new Content(null, content, contentType);
                child.SetCultureName("child", langUk.IsoCode);
                contentService.Save(child);

                Assert.AreEqual("child" + (i == 0 ? "" : " (" + i + ")"), child.GetCultureName(langUk.IsoCode));

                //Save it again to ensure that the unique check is not performed again against it's own name
                contentService.Save(child);
                Assert.AreEqual("child" + (i == 0 ? "" : " (" + i + ")"), child.GetCultureName(langUk.IsoCode));
            }
        }

        [Test]
        public void Can_Get_Paged_Children_WithFilterAndOrder()
        {
            var languageService = ServiceContext.LocalizationService;

            var langUk = new Language("en-GB") { IsDefault = true };
            var langFr = new Language("fr-FR");
            var langDa = new Language("da-DK");

            languageService.Save(langFr);
            languageService.Save(langUk);
            languageService.Save(langDa);

            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = contentTypeService.Get("umbTextpage");
            contentType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;

            var o = new[] { 2, 1, 3, 0, 4 }; // randomly different
            for (var i = 0; i < 5; i++)
            {
                var contentA = new Content(null, Constants.System.Root, contentType);
                contentA.SetCultureName("contentA" + i + "uk", langUk.IsoCode);
                contentA.SetCultureName("contentA" + o[i] + "fr", langFr.IsoCode);
                contentA.SetCultureName("contentX" + i + "da", langDa.IsoCode);
                contentService.Save(contentA);

                var contentB = new Content(null, Constants.System.Root, contentType);
                contentB.SetCultureName("contentB" + i + "uk", langUk.IsoCode);
                contentB.SetCultureName("contentB" + o[i] + "fr", langFr.IsoCode);
                contentB.SetCultureName("contentX" + i + "da", langDa.IsoCode);
                contentService.Save(contentB);
            }

            // get all
            var list = contentService.GetPagedChildren(Constants.System.Root, 0, 100, out var total).ToList();

            Console.WriteLine("ALL");
            WriteList(list);

            // 10 items (there's already a Home content in there...)
            Assert.AreEqual(11, total);
            Assert.AreEqual(11, list.Count);

            // filter
            list = contentService.GetPagedChildren(Constants.System.Root, 0, 100, out total,
                SqlContext.Query<IContent>().Where(x => x.Name.Contains("contentX")),
                Ordering.By("name", culture: langFr.IsoCode)).ToList();

            Assert.AreEqual(0, total);
            Assert.AreEqual(0, list.Count);

            // filter
            list = contentService.GetPagedChildren(Constants.System.Root, 0, 100, out total,
                SqlContext.Query<IContent>().Where(x => x.Name.Contains("contentX")),
                Ordering.By("name", culture: langDa.IsoCode)).ToList();

            Console.WriteLine("FILTER BY NAME da:'contentX'");
            WriteList(list);

            Assert.AreEqual(10, total);
            Assert.AreEqual(10, list.Count);

            // filter
            list = contentService.GetPagedChildren(Constants.System.Root, 0, 100, out total,
                SqlContext.Query<IContent>().Where(x => x.Name.Contains("contentA")),
                Ordering.By("name", culture: langFr.IsoCode)).ToList();

            Console.WriteLine("FILTER BY NAME fr:'contentA', ORDER ASC");
            WriteList(list);

            Assert.AreEqual(5, total);
            Assert.AreEqual(5, list.Count);

            for (var i = 0; i < 5; i++)
                Assert.AreEqual("contentA" + i + "fr", list[i].GetCultureName(langFr.IsoCode));

            list = contentService.GetPagedChildren(Constants.System.Root, 0, 100, out total,
                SqlContext.Query<IContent>().Where(x => x.Name.Contains("contentA")),
                Ordering.By("name", direction: Direction.Descending, culture: langFr.IsoCode)).ToList();

            Console.WriteLine("FILTER BY NAME fr:'contentA', ORDER DESC");
            WriteList(list);

            Assert.AreEqual(5, total);
            Assert.AreEqual(5, list.Count);

            for (var i = 0; i < 5; i++)
                Assert.AreEqual("contentA" + (4-i) + "fr", list[i].GetCultureName(langFr.IsoCode));
        }

        private void WriteList(List<IContent> list)
        {
            foreach (var content in list)
                Console.WriteLine("[{0}] {1} {2} {3} {4}", content.Id, content.Name, content.GetCultureName("en-GB"), content.GetCultureName("fr-FR"), content.GetCultureName("da-DK"));
            Console.WriteLine("-");
        }

        [Test]
        public void Can_SaveRead_Variations()
        {
            var languageService = ServiceContext.LocalizationService;

            //var langFr = new Language("fr-FR") { IsDefaultVariantLanguage = true };
            var langXx = new Language("pt-PT") { IsDefault = true };
            var langFr = new Language("fr-FR");
            var langUk = new Language("en-GB");
            var langDe = new Language("de-DE");

            languageService.Save(langFr);
            languageService.Save(langUk);
            languageService.Save(langDe);

            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = contentTypeService.Get("umbTextpage");
            contentType.Variations = ContentVariation.Culture;
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar, "prop") { Variations = ContentVariation.Culture });
            // FIXME: add test w/ an invariant prop
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;
            var content = contentService.Create("Home US", Constants.System.Root, "umbTextpage");

            // creating content with a name but no culture - will set the invariant name
            // but, because that content is variant, as soon as we save, we'll need to
            // replace the invariant name with whatever we have in cultures - always
            //
            // in fact, that would throw, because there is no name
            //contentService.Save(content);

            // act

            content.SetValue("author", "Barack Obama");
            content.SetValue("prop", "value-fr1", langFr.IsoCode);
            content.SetValue("prop", "value-uk1", langUk.IsoCode);
            content.SetCultureName("name-fr", langFr.IsoCode); // and then we can save
            content.SetCultureName("name-uk", langUk.IsoCode);
            contentService.Save(content);

            // content has been saved,
            // it has names, but no publishNames, and no published cultures

            var content2 = contentService.GetById(content.Id);

            Assert.AreEqual("name-fr", content2.Name); // got the default culture name when saved
            Assert.AreEqual("name-fr", content2.GetCultureName(langFr.IsoCode));
            Assert.AreEqual("name-uk", content2.GetCultureName(langUk.IsoCode));

            Assert.AreEqual("value-fr1", content2.GetValue("prop", langFr.IsoCode));
            Assert.AreEqual("value-uk1", content2.GetValue("prop", langUk.IsoCode));
            Assert.IsNull(content2.GetValue("prop", langFr.IsoCode, published: true));
            Assert.IsNull(content2.GetValue("prop", langUk.IsoCode, published: true));

            Assert.IsNull(content2.PublishName);
            Assert.IsNull(content2.GetPublishName(langFr.IsoCode));
            Assert.IsNull(content2.GetPublishName(langUk.IsoCode));

            // only fr and uk have a name, and are available
            AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));

            // nothing has been published yet
            AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, false), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, false), (langDe, false));

            // not published => must be edited, if available
            AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

            // act

            contentService.SaveAndPublish(content, new[]{ langFr.IsoCode, langUk.IsoCode });

            // both FR and UK have been published,
            // and content has been published,
            // it has names, publishNames, and published cultures

            content2 = contentService.GetById(content.Id);

            Assert.AreEqual("name-fr", content2.Name); // got the default culture name when saved
            Assert.AreEqual("name-fr", content2.GetCultureName(langFr.IsoCode));
            Assert.AreEqual("name-uk", content2.GetCultureName(langUk.IsoCode));

            // we haven't published InvariantNeutral, but a document cannot be published without an invariant name,
            // so when we tried and published for the first time above the french culture, the french name was used
            // to populate the invariant name
            Assert.AreEqual("name-fr", content2.PublishName);

            Assert.AreEqual("name-fr", content2.GetPublishName(langFr.IsoCode));
            Assert.AreEqual("name-uk", content2.GetPublishName(langUk.IsoCode));

            Assert.AreEqual("value-fr1", content2.GetValue("prop", langFr.IsoCode));
            Assert.AreEqual("value-uk1", content2.GetValue("prop", langUk.IsoCode));
            Assert.AreEqual("value-fr1", content2.GetValue("prop", langFr.IsoCode, published: true));
            Assert.AreEqual("value-uk1", content2.GetValue("prop", langUk.IsoCode, published: true));

            // no change
            AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));

            // fr and uk have been published now
            AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, true), (langUk, true), (langDe, false));

            // fr and uk, published without changes, not edited
            AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, false), (langUk, false), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, false), (langUk, false), (langDe, false));

            AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langFr, false), (langUk, false)); // DE would throw
            AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langFr, false), (langUk, false)); // DE would throw

            // note that content and content2 culture published dates might be slightly different due to roundtrip to database


            // act

            contentService.SaveAndPublish(content);

            // now it has publish name for invariant neutral

            content2 = contentService.GetById(content.Id);

            Assert.AreEqual("name-fr", content2.PublishName);


            // act

            content.SetCultureName("Home US2", null);
            content.SetCultureName("name-fr2", langFr.IsoCode);
            content.SetCultureName("name-uk2", langUk.IsoCode);
            content.SetValue("author", "Barack Obama2");
            content.SetValue("prop", "value-fr2", langFr.IsoCode);
            content.SetValue("prop", "value-uk2", langUk.IsoCode);
            contentService.Save(content);

            // content has been saved,
            // it has updated names, unchanged publishNames, and published cultures

            content2 = contentService.GetById(content.Id);

            Assert.AreEqual("name-fr2", content2.Name); // got the default culture name when saved
            Assert.AreEqual("name-fr2", content2.GetCultureName(langFr.IsoCode));
            Assert.AreEqual("name-uk2", content2.GetCultureName(langUk.IsoCode));

            Assert.AreEqual("name-fr", content2.PublishName);
            Assert.AreEqual("name-fr", content2.GetPublishName(langFr.IsoCode));
            Assert.AreEqual("name-uk", content2.GetPublishName(langUk.IsoCode));

            Assert.AreEqual("Barack Obama2", content2.GetValue("author"));
            Assert.AreEqual("Barack Obama", content2.GetValue("author", published: true));

            Assert.AreEqual("value-fr2", content2.GetValue("prop", langFr.IsoCode));
            Assert.AreEqual("value-uk2", content2.GetValue("prop", langUk.IsoCode));
            Assert.AreEqual("value-fr1", content2.GetValue("prop", langFr.IsoCode, published: true));
            Assert.AreEqual("value-uk1", content2.GetValue("prop", langUk.IsoCode, published: true));

            // no change
            AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));

            // no change
            AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, true), (langUk, true), (langDe, false));

            // we have changed values so now fr and uk are edited
            AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

            AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langFr, false), (langUk, false)); // DE would throw
            AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langFr, false), (langUk, false)); // DE would throw


            // act
            // cannot just 'save' since we are changing what's published!

            contentService.Unpublish(content, langFr.IsoCode);

            // content has been published,
            // the french culture is gone
            // (only if french is not mandatory, else everything would be gone!)

            content2 = contentService.GetById(content.Id);

            Assert.AreEqual("name-fr2", content2.Name); // got the default culture name when saved
            Assert.AreEqual("name-fr2", content2.GetCultureName(langFr.IsoCode));
            Assert.AreEqual("name-uk2", content2.GetCultureName(langUk.IsoCode));

            Assert.AreEqual("name-fr2", content2.PublishName);
            Assert.IsNull(content2.GetPublishName(langFr.IsoCode));
            Assert.AreEqual("name-uk", content2.GetPublishName(langUk.IsoCode));

            Assert.AreEqual("value-fr2", content2.GetValue("prop", langFr.IsoCode));
            Assert.AreEqual("value-uk2", content2.GetValue("prop", langUk.IsoCode));
            Assert.IsNull(content2.GetValue("prop", langFr.IsoCode, published: true));
            Assert.AreEqual("value-uk1", content2.GetValue("prop", langUk.IsoCode, published: true));

            Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
            Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

            // no change
            AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));

            // fr is not published anymore
            AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true), (langDe, false));

            // and so, fr has to be edited
            AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

            AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw
            AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw


            // act

            contentService.Unpublish(content);

            // content has been unpublished,
            // but properties, names, etc. retain their 'published' values so the content
            // can be re-published in its exact original state (before being unpublished)
            //
            // BEWARE!
            // in order for a content to be unpublished as a whole, and then republished in
            // its exact previous state, properties and names etc. retain their published
            // values even though the content is not published - hence many things being
            // non-null or true below - always check against content.Published to be sure

            content2 = contentService.GetById(content.Id);

            Assert.IsFalse(content2.Published);

            Assert.AreEqual("name-fr2", content2.Name); // got the default culture name when saved
            Assert.AreEqual("name-fr2", content2.GetCultureName(langFr.IsoCode));
            Assert.AreEqual("name-uk2", content2.GetCultureName(langUk.IsoCode));

            Assert.AreEqual("name-fr2", content2.PublishName); // not null, see note above
            Assert.IsNull(content2.GetPublishName(langFr.IsoCode));
            Assert.AreEqual("name-uk", content2.GetPublishName(langUk.IsoCode)); // not null, see note above

            Assert.AreEqual("value-fr2", content2.GetValue("prop", langFr.IsoCode));
            Assert.AreEqual("value-uk2", content2.GetValue("prop", langUk.IsoCode));
            Assert.IsNull(content2.GetValue("prop", langFr.IsoCode, published: true));
            Assert.AreEqual("value-uk1", content2.GetValue("prop", langUk.IsoCode, published: true));  // has value, see note above

            // no change
            AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));

            // fr is not published anymore - uk still is, see note above
            AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true), (langDe, false));

            // and so, fr has to be edited - uk still is
            AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

            AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw
            AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw


            // act

            // that HAS to be SavePublishing, because SaveAndPublish would just republish everything!
            //TODO: This is using an internal API - the test can't pass without this but we want to keep the test here
            // will need stephane to have a look at this test at some stage since there is a lot of logic here that we
            // want to keep on testing but don't need the public API to do these more complicated things.
            ((ContentService)contentService).CommitDocumentChanges(content);

            // content has been re-published,
            // everything is back to what it was before being unpublished

            content2 = contentService.GetById(content.Id);

            Assert.IsTrue(content2.Published);

            Assert.AreEqual("name-fr2", content2.Name); // got the default culture name when saved
            Assert.AreEqual("name-fr2", content2.GetCultureName(langFr.IsoCode));
            Assert.AreEqual("name-uk2", content2.GetCultureName(langUk.IsoCode));

            Assert.AreEqual("name-fr2", content2.PublishName);
            Assert.IsNull(content2.GetPublishName(langFr.IsoCode));
            Assert.AreEqual("name-uk", content2.GetPublishName(langUk.IsoCode));

            Assert.AreEqual("value-fr2", content2.GetValue("prop", langFr.IsoCode));
            Assert.AreEqual("value-uk2", content2.GetValue("prop", langUk.IsoCode));
            Assert.IsNull(content2.GetValue("prop", langFr.IsoCode, published: true));
            Assert.AreEqual("value-uk1", content2.GetValue("prop", langUk.IsoCode, published: true));

            // no change
            AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));

            // no change, back to published
            AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true), (langDe, false));

            // no change, back to published
            AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

            AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw
            AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw


            // act

            contentService.SaveAndPublish(content, langUk.IsoCode);

            content2 = contentService.GetById(content.Id);

            // no change
            AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true), (langDe, false));

            // no change
            AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true), (langDe, false));

            // now, uk is no more edited
            AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, false), (langDe, false));
            AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, false), (langDe, false));

            AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw
            AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw


            // act

            content.SetCultureName("name-uk3", langUk.IsoCode);
            contentService.Save(content);

            content2 = contentService.GetById(content.Id);

            // note that the 'edited' flags only change once saved - not immediately
            // but they change, on what's being saved, and when getting it back

            // changing the name = edited!

            Assert.IsTrue(content.IsCultureEdited(langUk.IsoCode));
            Assert.IsTrue(content2.IsCultureEdited(langUk.IsoCode));
        }

        private void AssertPerCulture<T>(IContent item, Func<IContent, string, T> getter, params (ILanguage Language, bool Result)[] testCases)
        {
            foreach (var testCase in testCases)
            {
                var value = getter(item, testCase.Language.IsoCode);
                Assert.AreEqual(testCase.Result, value, $"Expected {testCase.Result} and got {value} for culture {testCase.Language.IsoCode}.");
            }
        }

        private IEnumerable<IContent> CreateContentHierarchy()
        {
            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");
            var root = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 2);

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

        private DocumentRepository CreateRepository(IScopeProvider provider, out ContentTypeRepository contentTypeRepository)
        {
            var accessor = (IScopeAccessor) provider;
            var templateRepository = new TemplateRepository(accessor, AppCaches.Disabled, Logger, TestObjects.GetFileSystemsMock());
            var tagRepository = new TagRepository(accessor, AppCaches.Disabled, Logger);
            var commonRepository = new ContentTypeCommonRepository(accessor, templateRepository, AppCaches);
            var languageRepository = new LanguageRepository(accessor, AppCaches.Disabled, Logger);
            contentTypeRepository = new ContentTypeRepository(accessor, AppCaches.Disabled, Logger, commonRepository, languageRepository);
            var relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Logger);
            var entityRepository = new EntityRepository(accessor);
            var relationRepository = new RelationRepository(accessor, Logger, relationTypeRepository, entityRepository);
            var propertyEditors = new Lazy<PropertyEditorCollection>(() => new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<IDataEditor>())));
            var dataValueReferences = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>());
            var repository = new DocumentRepository(accessor, AppCaches.Disabled, Logger, contentTypeRepository, templateRepository, tagRepository, languageRepository, relationRepository, relationTypeRepository, propertyEditors, dataValueReferences);
            return repository;
        }

        private void CreateEnglishAndFrenchDocumentType(out Language langUk, out Language langFr, out ContentType contentType)
        {
            langUk = new Language("en-GB") { IsDefault = true };
            langFr = new Language("fr-FR");
            ServiceContext.LocalizationService.Save(langFr);
            ServiceContext.LocalizationService.Save(langUk);

            contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);
        }

        private IContent CreateEnglishAndFrenchDocument(out Language langUk, out Language langFr, out ContentType contentType)
        {
            CreateEnglishAndFrenchDocumentType(out langUk, out langFr, out contentType);

            IContent content = new Content("content", Constants.System.Root, contentType);
            content.SetCultureName("content-fr", langFr.IsoCode);
            content.SetCultureName("content-en", langUk.IsoCode);

            return content;
        }
    }
}
