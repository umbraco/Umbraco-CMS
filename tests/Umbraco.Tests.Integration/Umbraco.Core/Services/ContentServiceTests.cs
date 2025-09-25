// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Language = Umbraco.Cms.Core.Models.Language;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering all methods in the ContentService class.
/// </summary>
[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
internal sealed class ContentServiceTests : UmbracoIntegrationTestWithContent
{
    [SetUp]
    public void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;
    // TODO: Add test to verify there is only ONE newest document/content in {Constants.DatabaseSchema.Tables.Document} table after updating.
    // TODO: Add test to delete specific version (with and without deleting prior versions) and versions by date.


    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();
    private ILocalizedTextService LocalizedTextService => GetRequiredService<ILocalizedTextService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IAuditService AuditService => GetRequiredService<IAuditService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IRelationService RelationService => GetRequiredService<IRelationService>();

    private ITagService TagService => GetRequiredService<ITagService>();

    private IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();

    private IDomainService DomainService => GetRequiredService<IDomainService>();

    private INotificationService NotificationService => GetRequiredService<INotificationService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IDocumentRepository DocumentRepository => GetRequiredService<IDocumentRepository>();

    private IJsonSerializer Serializer => GetRequiredService<IJsonSerializer>();

    private IValueEditorCache ValueEditorCache => GetRequiredService<IValueEditorCache>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentPublishingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentCopyingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentCopiedNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentSavingNotification, ContentNotificationHandler>();

    [Test]
    public void Create_Blueprint()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var blueprint = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
        blueprint.SetValue("title", "blueprint 1");
        blueprint.SetValue("bodyText", "blueprint 2");
        blueprint.SetValue("keywords", "blueprint 3");
        blueprint.SetValue("description", "blueprint 4");

        ContentService.SaveBlueprint(blueprint, null);

        var found = ContentService.GetBlueprintsForContentTypes().ToArray();
        Assert.AreEqual(1, found.Length);

        // ensures it's not found by normal content
        var contentFound = ContentService.GetById(found[0].Id);
        Assert.IsNull(contentFound);
    }

    [Test]
    public void Delete_Blueprint()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var blueprint = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
        blueprint.SetValue("title", "blueprint 1");
        blueprint.SetValue("bodyText", "blueprint 2");
        blueprint.SetValue("keywords", "blueprint 3");
        blueprint.SetValue("description", "blueprint 4");

        ContentService.SaveBlueprint(blueprint, null);

        ContentService.DeleteBlueprint(blueprint);

        var found = ContentService.GetBlueprintsForContentTypes().ToArray();
        Assert.AreEqual(0, found.Length);
    }

    [Test]
    public void Create_Blueprint_From_Content()
    {
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
            ContentTypeService.Save(contentType);

            var originalPage = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
            originalPage.SetValue("title", "blueprint 1");
            originalPage.SetValue("bodyText", "blueprint 2");
            originalPage.SetValue("keywords", "blueprint 3");
            originalPage.SetValue("description", "blueprint 4");
            ContentService.Save(originalPage);

            var fromContent = ContentService.CreateBlueprintFromContent(originalPage, "hello world");
            ContentService.SaveBlueprint(fromContent, originalPage);

            Assert.IsTrue(fromContent.HasIdentity);
            Assert.AreEqual("blueprint 1", fromContent.Properties["title"]?.GetValue());
            Assert.AreEqual("blueprint 2", fromContent.Properties["bodyText"]?.GetValue());
            Assert.AreEqual("blueprint 3", fromContent.Properties["keywords"]?.GetValue());
            Assert.AreEqual("blueprint 4", fromContent.Properties["description"]?.GetValue());
        }
    }

    [Test]
    [LongRunning]
    public void Get_All_Blueprints()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var ct1 = ContentTypeBuilder.CreateTextPageContentType("ct1", defaultTemplateId: template.Id);
        FileService.SaveTemplate(ct1.DefaultTemplate);
        ContentTypeService.Save(ct1);
        var ct2 = ContentTypeBuilder.CreateTextPageContentType("ct2", defaultTemplateId: template.Id);
        FileService.SaveTemplate(ct2.DefaultTemplate);
        ContentTypeService.Save(ct2);

        for (var i = 0; i < 10; i++)
        {
            var blueprint =
                ContentBuilder.CreateTextpageContent(i % 2 == 0 ? ct1 : ct2, "hello" + i, Constants.System.Root);
            ContentService.SaveBlueprint(blueprint, null);
        }

        var found = ContentService.GetBlueprintsForContentTypes().ToArray();
        Assert.AreEqual(10, found.Length);

        found = ContentService.GetBlueprintsForContentTypes(ct1.Id).ToArray();
        Assert.AreEqual(5, found.Length);

        found = ContentService.GetBlueprintsForContentTypes(ct2.Id).ToArray();
        Assert.AreEqual(5, found.Length);
    }

    [Test]
    [LongRunning]
    public async Task Perform_Scheduled_Publishing()
    {
        var langUk = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .Build();
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();

        await LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langUk, Constants.Security.SuperUserKey);

        var ctInvariant = ContentTypeBuilder.CreateBasicContentType("invariantPage");
        ContentTypeService.Save(ctInvariant);

        var ctVariant = ContentTypeBuilder.CreateBasicContentType("variantPage");
        ctVariant.Variations = ContentVariation.Culture;
        ContentTypeService.Save(ctVariant);

        var now = DateTime.UtcNow;

        // 10x invariant content, half is scheduled to be published in 5 seconds, the other half is scheduled to be unpublished in 5 seconds
        var invariant = new List<IContent>();
        for (var i = 0; i < 10; i++)
        {
            var c = ContentBuilder.CreateBasicContent(ctInvariant);
            c.Name = "name" + i;
            if (i % 2 == 0)
            {
                var contentSchedule =
                    ContentScheduleCollection.CreateWithEntry(now.AddSeconds(5), null); // release in 5 seconds
                var r = ContentService.Save(c, contentSchedule: contentSchedule);
                Assert.IsTrue(r.Success, r.Result.ToString());
            }
            else
            {
                ContentService.Save(c);
                var r = ContentService.Publish(c, c.AvailableCultures.ToArray());

                var contentSchedule =
                    ContentScheduleCollection.CreateWithEntry(null, now.AddSeconds(5)); // expire in 5 seconds
                ContentService.PersistContentSchedule(c, contentSchedule);

                Assert.IsTrue(r.Success, r.Result.ToString());
            }

            invariant.Add(c);
        }

        // 10x variant content, half is scheduled to be published in 5 seconds, the other half is scheduled to be unpublished in 5 seconds
        var variant = new List<IContent>();
        var alternatingCulture = langFr.IsoCode;
        for (var i = 0; i < 10; i++)
        {
            var c = ContentBuilder.CreateBasicContent(ctVariant);
            c.SetCultureName("name-uk" + i, langUk.IsoCode);
            c.SetCultureName("name-fr" + i, langFr.IsoCode);

            if (i % 2 == 0)
            {
                var contentSchedule =
                    ContentScheduleCollection.CreateWithEntry(alternatingCulture, now.AddSeconds(5),
                        null); // release in 5 seconds
                var r = ContentService.Save(c, contentSchedule: contentSchedule);
                Assert.IsTrue(r.Success, r.Result.ToString());

                alternatingCulture = alternatingCulture == langFr.IsoCode ? langUk.IsoCode : langFr.IsoCode;
            }
            else
            {
                ContentService.Save(c);
                var r = ContentService.Publish(c, c.AvailableCultures.ToArray());

                var contentSchedule =
                    ContentScheduleCollection.CreateWithEntry(alternatingCulture, null,
                        now.AddSeconds(5)); // expire in 5 seconds
                ContentService.PersistContentSchedule(c, contentSchedule);

                Assert.IsTrue(r.Success, r.Result.ToString());
            }

            variant.Add(c);
        }

        var runSched = ContentService.PerformScheduledPublish(
            now.AddMinutes(1)).ToList(); // process anything scheduled before a minute from now

        // this is 21 because the test data installed before this test runs has a scheduled item!
        Assert.AreEqual(21, runSched.Count);
        Assert.AreEqual(
            20,
            runSched.Count(x => x.Success),
            string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));

        Assert.AreEqual(
            5,
            runSched.Count(x => x.Result == PublishResultType.SuccessPublish),
            string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));
        Assert.AreEqual(
            5,
            runSched.Count(x => x.Result == PublishResultType.SuccessUnpublish),
            string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));
        Assert.AreEqual(
            5,
            runSched.Count(x => x.Result == PublishResultType.SuccessPublishCulture),
            string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));
        Assert.AreEqual(
            5,
            runSched.Count(x => x.Result == PublishResultType.SuccessUnpublishCulture),
            string.Join(Environment.NewLine, runSched.Select(x => $"{x.Entity.Name} - {x.Result}")));

        // re-run the scheduled publishing, there should be no results
        runSched = ContentService.PerformScheduledPublish(
            now.AddMinutes(1)).ToList();

        Assert.AreEqual(0, runSched.Count);
    }

    [Test]
    public void Remove_Scheduled_Publishing_Date()
    {
        // Arrange

        // Act
        var content = ContentService.CreateAndSave("Test", Constants.System.Root, "umbTextpage");

        var contentSchedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddHours(2));
        ContentService.Save(content, Constants.Security.SuperUserId, contentSchedule);
        Assert.AreEqual(1, contentSchedule.FullSchedule.Count);

        contentSchedule = ContentService.GetContentScheduleByContentId(content.Id);
        var sched = contentSchedule.FullSchedule;
        Assert.AreEqual(1, sched.Count);
        Assert.AreEqual(1, sched.Count(x => x.Culture == Constants.System.InvariantCulture));
        contentSchedule.Clear(ContentScheduleAction.Expire);
        ContentService.Save(content, Constants.Security.SuperUserId, contentSchedule);

        // Assert
        contentSchedule = ContentService.GetContentScheduleByContentId(content.Id);
        sched = contentSchedule.FullSchedule;
        Assert.AreEqual(0, sched.Count);
        Assert.IsTrue(ContentService.Publish(content, content.AvailableCultures.ToArray()).Success);
    }

    [Test]
    [LongRunning]
    public void Get_Top_Version_Ids()
    {
        // Arrange
        // Act
        var content = ContentService.CreateAndSave("Test", Constants.System.Root, "umbTextpage");
        for (var i = 0; i < 20; i++)
        {
            content.SetValue("bodyText", "hello world " + Guid.NewGuid());
            ContentService.Save(content);
            ContentService.Publish(content, content.AvailableCultures.ToArray());
        }

        // Assert
        var allVersions = ContentService.GetVersionIds(content.Id, int.MaxValue);
        Assert.AreEqual(21, allVersions.Count());

        var topVersions = ContentService.GetVersionIds(content.Id, 4);
        Assert.AreEqual(4, topVersions.Count());
    }

    [Test]
    [LongRunning]
    public void Get_By_Ids_Sorted()
    {
        // Arrange
        // Act
        var results = new List<IContent>();
        for (var i = 0; i < 20; i++)
        {
            results.Add(ContentService.CreateAndSave("Test", Constants.System.Root, "umbTextpage", -1));
        }

        var sortedGet = ContentService.GetByIds(new[] { results[10].Id, results[5].Id, results[12].Id })
            .ToArray();

        // Assert
        Assert.AreEqual(sortedGet[0].Id, results[10].Id);
        Assert.AreEqual(sortedGet[1].Id, results[5].Id);
        Assert.AreEqual(sortedGet[2].Id, results[12].Id);
    }

    [Test]
    public void Count_All()
    {
        // Arrange
        // Act
        for (var i = 0; i < 20; i++)
        {
            ContentService.CreateAndSave("Test", Constants.System.Root, "umbTextpage");
        }

        // Assert
        Assert.AreEqual(25, ContentService.Count());
    }

    [Test]
    public void Count_By_Content_Type()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbBlah", "test Doc Type", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        // Act
        for (var i = 0; i < 20; i++)
        {
            ContentService.CreateAndSave("Test", Constants.System.Root, "umbBlah");
        }

        // Assert
        Assert.AreEqual(20, ContentService.Count("umbBlah"));
    }

    [Test]
    public void Count_Children()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbBlah", "test Doc Type", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        var parent = ContentService.CreateAndSave("Test", Constants.System.Root, "umbBlah");

        // Act
        for (var i = 0; i < 20; i++)
        {
            ContentService.CreateAndSave("Test", parent, "umbBlah");
        }

        // Assert
        Assert.AreEqual(20, ContentService.CountChildren(parent.Id));
    }

    [Test]
    public void Count_Descendants()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbBlah", "test Doc Type", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        var parent = ContentService.CreateAndSave("Test", Constants.System.Root, "umbBlah");

        // Act
        var current = parent;
        for (var i = 0; i < 20; i++)
        {
            current = ContentService.CreateAndSave("Test", current, "umbBlah");
        }

        // Assert
        Assert.AreEqual(20, ContentService.CountDescendants(parent.Id));
    }

    [Test]
    public void GetAncestors_Returns_Empty_List_When_Path_Is_Null()
    {
        // Arrange
        // Act
        var current = new Mock<IContent>();
        var res = ContentService.GetAncestors(current.Object);

        // Assert
        Assert.IsEmpty(res);
    }

    [Test]
    public void Can_Remove_Property_Type()
    {
        // Arrange
        // Act
        var content = ContentService.Create("Test", Constants.System.Root, "umbTextpage");

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.HasIdentity, Is.False);
    }

    [Test]
    public void Can_Create_Content()
    {
        // Arrange
        // Act
        var content = ContentService.Create("Test", Constants.System.Root, "umbTextpage");

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.HasIdentity, Is.False);
    }

    public void Can_Create_Content_Without_Explicitly_Set_User()
    {
        // Arrange
        // Act
        var content = ContentService.Create("Test", Constants.System.Root, "umbTextpage");

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.HasIdentity, Is.False);
        Assert.That(content.CreatorId,
            Is.EqualTo(Constants.Security
                .SuperUserId)); // Default to -1 aka SuperUser (unknown) since we didn't explicitly set this in the Create call
    }

    [Test]
    public void Can_Save_New_Content_With_Explicit_User()
    {
        var user = new UserBuilder().Build();
        UserService.Save(user);
        var content = new Content("Test", Constants.System.Root, ContentTypeService.Get("umbTextpage"));

        // Act
        ContentService.Save(content, user.Id);

        // Assert
        Assert.That(content.CreatorId, Is.EqualTo(user.Id));
        Assert.That(content.WriterId, Is.EqualTo(user.Id));
    }

    [Test]
    public void Cannot_Create_Content_With_Non_Existing_ContentType_Alias() =>
        Assert.Throws<Exception>(() => ContentService.Create("Test", Constants.System.Root, "umbAliasDoesntExist"));

    [Test]
    public void Cannot_Save_Content_With_Empty_Name()
    {
        // Arrange
        var content = new Content(string.Empty, Constants.System.Root, ContentTypeService.Get("umbTextpage"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ContentService.Save(content));
    }

    [Test]
    public void Can_Get_Content_By_Id()
    {
        // Arrange
        // Act
        var content = ContentService.GetById(Textpage.Id);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Id, Is.EqualTo(Textpage.Id));
    }

    [Test]
    public void Can_Get_Content_By_Guid_Key()
    {
        // Arrange
        // Act
        var content = ContentService.GetById(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Id, Is.EqualTo(Textpage.Id));
    }

    [Test]
    public void Can_Get_Content_By_Level()
    {
        // Arrange
        // Act
        var contents = ContentService.GetByLevel(2).ToList();

        // Assert
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Any(), Is.True);
        Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    [LongRunning]
    public void Can_Get_All_Versions_Of_Content()
    {
        var parent = ContentService.GetById(Textpage.Id);
        Assert.IsFalse(parent.Published);
        ContentService.Save(parent); // publishing parent, so Text Page 2 can be updated.
        ContentService.Publish(parent, parent.AvailableCultures.ToArray());

        var content = ContentService.GetById(Subpage.Id);
        Assert.IsFalse(content.Published);
        var versions = ContentService.GetVersions(Subpage.Id).ToList();
        Assert.AreEqual(1, versions.Count);

        var version1 = content.VersionId;
        Console.WriteLine($"1 e={content.VersionId} p={content.PublishedVersionId}");

        content.Name = "Text Page 2 Updated";
        content.SetValue("author", "Jane Doe");
        ContentService.Save(content); // publishes the current version, creates a version
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        var version2 = content.VersionId;
        Console.WriteLine($"2 e={content.VersionId} p={content.PublishedVersionId}");

        content.Name = "Text Page 2 ReUpdated";
        content.SetValue("author", "Bob Hope");
        ContentService.Save(content); // publishes again, creates a version
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        var version3 = content.VersionId;
        Console.WriteLine($"3 e={content.VersionId} p={content.PublishedVersionId}");

        var content1 = ContentService.GetById(content.Id);
        Assert.AreEqual("Bob Hope", content1.GetValue("author"));
        Assert.AreEqual("Bob Hope", content1.GetValue("author", published: true));

        content.Name = "Text Page 2 ReReUpdated";
        content.SetValue("author", "John Farr");
        ContentService.Save(content); // no new version

        content1 = ContentService.GetById(content.Id);
        Assert.AreEqual("John Farr", content1.GetValue("author"));
        Assert.AreEqual("Bob Hope", content1.GetValue("author", published: true));

        versions = ContentService.GetVersions(Subpage.Id).ToList();
        Assert.AreEqual(3, versions.Count);

        // versions come with most recent first
        Assert.AreEqual(version3, versions[0].VersionId); // the edited version
        Assert.AreEqual(version2, versions[1].VersionId); // the published version
        Assert.AreEqual(version1, versions[2].VersionId); // the previously published version

        // p is always the same, published version
        // e is changing, actual version we're loading
        Console.WriteLine();
        foreach (var version in ((IEnumerable<IContent>)versions).Reverse())
        {
            Console.WriteLine($"+ e={((Content)version).VersionId} p={((Content)version).PublishedVersionId}");
        }

        // and proper values
        // first, the current (edited) version, with edited and published versions
        Assert.AreEqual("John Farr", versions[0].GetValue("author")); // current version has the edited value
        Assert.AreEqual(
            "Bob Hope",
            versions[0].GetValue("author", published: true)); // and the published published value

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
        // Act
        var contents = ContentService.GetRootContent().ToList();

        // Assert
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Any(), Is.True);
        Assert.That(contents.Count(), Is.EqualTo(1));
    }

    [Test]
    [LongRunning]
    public void Can_Get_Content_For_Expiration()
    {
        // Arrange
        var root = ContentService.GetById(Textpage.Id);
        ContentService.Publish(root!, root!.AvailableCultures.ToArray());
        var content = ContentService.GetById(Subpage.Id);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddSeconds(1));
        ContentService.PersistContentSchedule(content!, contentSchedule);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // Act
        Thread.Sleep(new TimeSpan(0, 0, 0, 2));
        var contents = ContentService.GetContentForExpiration(DateTime.Now).ToList();

        // Assert
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Any(), Is.True);
        Assert.That(contents.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Can_Get_Content_Schedules_By_Keys()
    {
        // Arrange
        var root = ContentService.GetById(Textpage.Id);
        ContentService.Publish(root!, root!.AvailableCultures.ToArray());
        var content = ContentService.GetById(Subpage.Id);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddDays(1), null);
        ContentService.PersistContentSchedule(content!, contentSchedule);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // Act
        var keys = ContentService.GetContentSchedulesByIds([Textpage.Key, Subpage.Key, Subpage2.Key]).ToList();

        // Assert
        Assert.AreEqual(1, keys.Count);
        Assert.AreEqual(keys[0].Key, Subpage.Id);
        Assert.AreEqual(keys[0].Value.First().Id, contentSchedule.FullSchedule.First().Id);
    }

    [Test]
    public void Can_Get_Content_For_Release()
    {
        // Arrange
        // Act
        var contents = ContentService.GetContentForRelease(DateTime.Now).ToList();

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
        // Act
        var contents = ContentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var _).ToList();

        // Assert
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Any(), Is.True);
        Assert.That(contents.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Can_Unpublish_Content()
    {
        // Arrange
        var content = ContentService.GetById(Textpage.Id);
        Assert.IsNotNull(content);
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: -1);

        // Act
        var unpublished = ContentService.Unpublish(content, userId: -1);

        // Assert
        Assert.That(published.Success, Is.True);
        Assert.That(unpublished.Success, Is.True);
        Assert.That(content.Published, Is.False);
        Assert.AreEqual(PublishResultType.SuccessUnpublish, unpublished.Result);
    }

    [Test]
    public void Can_Unpublish_Content_Variation()
    {
        var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr, out var contentType);

        var saved = ContentService.Save(content);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

        var unpublished = ContentService.Unpublish(content, langFr.IsoCode);
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
    }

    [Test]
    [LongRunning]
    public void Can_Publish_Culture_After_Last_Culture_Unpublished()
    {
        var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr,
            out var contentType);

        ContentService.Save(content);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = ContentService.GetById(content.Id);

        var unpublished = ContentService.Unpublish(content, langUk.IsoCode); // first culture
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));

        content = ContentService.GetById(content.Id);

        unpublished = ContentService.Unpublish(content, langFr.IsoCode); // last culture
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishLastCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

        content = ContentService.GetById(content.Id);

        published = ContentService.Publish(content, new[] { langUk.IsoCode });
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));

        content = ContentService.GetById(content.Id); // reget
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
    }

    [Test]
    public void Unpublish_All_Cultures_Has_Unpublished_State()
    {
        var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr,
            out var contentType);

        var saved = ContentService.Save(content);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        var unpublished = ContentService.Unpublish(content, langFr.IsoCode); // first culture
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.AreEqual(PublishedState.Published, content.PublishedState); // still published

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

        unpublished = ContentService.Unpublish(content, langUk.IsoCode); // last culture
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishLastCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
        Assert.AreEqual(PublishedState.Unpublished,
            content.PublishedState); // the last culture was unpublished so the document should also reflect this

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.AreEqual(PublishedState.Unpublished, content.PublishedState); // just double checking
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
    }

    [Test]
    public async Task Unpublishing_Mandatory_Language_Unpublishes_Document()
    {
        var langUk = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .WithIsMandatory(true)
            .Build();
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();

        await LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langUk, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-fr", langFr.IsoCode);
        content.SetCultureName("content-en", langUk.IsoCode);

        var saved = ContentService.Save(content);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = ContentService.GetById(content.Id);

        var unpublished = ContentService.Unpublish(content, langUk.IsoCode); // unpublish mandatory lang
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishMandatoryCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode)); // remains published
        Assert.AreEqual(PublishedState.Unpublished, content.PublishedState);
    }

    [Test]
    public void Unpublishing_Already_Unpublished_Culture()
    {
        var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr,
            out var contentType);

        var saved = ContentService.Save(content);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = ContentService.GetById(content.Id);

        var unpublished = ContentService.Unpublish(content, langUk.IsoCode);
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

        content = ContentService.GetById(content.Id);

        // Change some data since Unpublish should always Save
        content.SetCultureName("content-en-updated", langUk.IsoCode);

        unpublished = ContentService.Unpublish(content, langUk.IsoCode); // unpublish again
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishAlready, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

        content = ContentService.GetById(content.Id);

        // ensure that even though the culture was already unpublished that the data was still persisted
        Assert.AreEqual("content-en-updated", content.GetCultureName(langUk.IsoCode));
    }

    [Test]
    public void Publishing_No_Cultures_Still_Saves()
    {
        var content = CreateEnglishAndFrenchDocument(out var langUk, out var langFr,
            out var contentType);

        var saved = ContentService.Save(content);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = ContentService.GetById(content.Id);

        // Change some data since SaveAndPublish should always Save
        content.SetCultureName("content-en-updated", langUk.IsoCode);

        saved = ContentService.Save(content);
        published = ContentService.Publish(content, new string[] { }); // publish without cultures
        Assert.IsTrue(saved.Success);
        Assert.AreEqual(PublishResultType.FailedPublishNothingToPublish, published.Result);

        // re-get
        content = ContentService.GetById(content.Id);

        // ensure that even though nothing was published that the data was still persisted
        Assert.AreEqual("content-en-updated", content.GetCultureName(langUk.IsoCode));
    }

    [Test]
    public async Task Pending_Invariant_Property_Changes_Affect_Default_Language_Edited_State()
    {
        // Arrange
        var langGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .Build();
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();

        await LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langGb, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateMetaContentType();
        contentType.Variations = ContentVariation.Culture;
        foreach (var prop in contentType.PropertyTypes)
        {
            prop.Variations = ContentVariation.Culture;
        }

        var keywordsProp = contentType.PropertyTypes.Single(x => x.Alias == "metakeywords");
        keywordsProp.Variations = ContentVariation.Nothing; // this one is invariant

        ContentTypeService.Save(contentType);

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-en", langGb.IsoCode);
        content.SetCultureName("content-fr", langFr.IsoCode);

        Assert.IsTrue(ContentService.Save(content).Success);
        Assert.IsTrue(ContentService.Publish(content, new[] { langGb.IsoCode, langFr.IsoCode }).Success);

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
        Assert.IsTrue(content.IsCulturePublished(langGb.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCultureEdited(langGb.IsoCode));
        Assert.IsFalse(content.IsCultureEdited(langFr.IsoCode));

        // update the invariant property and save a pending version
        content.SetValue("metakeywords", "hello");
        ContentService.Save(content);

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
        Assert.IsTrue(content.IsCulturePublished(langGb.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCultureEdited(langGb.IsoCode));
        Assert.IsFalse(content.IsCultureEdited(langFr.IsoCode));
    }

    [Test]
    public void Can_Publish_Content_Variation_And_Detect_Changed_Cultures()
    {
        CreateEnglishAndFrenchDocumentType(out var langUk, out var langFr, out var contentType);

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-fr", langFr.IsoCode);
        ContentService.Save(content);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode });

        // audit log will only show that french was published
        var lastLog = AuditService.GetLogs(content.Id).Last();
        Assert.AreEqual("Published languages: fr-FR", lastLog.Comment);

        // re-get
        content = ContentService.GetById(content.Id);
        content.SetCultureName("content-en", langUk.IsoCode);
        ContentService.Save(content);
        published = ContentService.Publish(content, new[] { langUk.IsoCode });

        // audit log will only show that english was published
        lastLog = AuditService.GetLogs(content.Id).Last();
        Assert.AreEqual("Published languages: en-GB", lastLog.Comment);
    }

    [Test]
    public async Task Can_Unpublish_Content_Variation_And_Detect_Changed_Cultures()
    {
        // Arrange
        var langGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .WithIsMandatory(true)
            .Build();
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();

        await LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langGb, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-fr", langFr.IsoCode);
        content.SetCultureName("content-gb", langGb.IsoCode);
        var saved = ContentService.Save(content);
        var published = ContentService.Publish(content, new[] { langGb.IsoCode, langFr.IsoCode });
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);

        // re-get
        content = ContentService.GetById(content.Id);
        var unpublished = ContentService.Unpublish(content, langFr.IsoCode);

        // audit log will only show that french was unpublished
        var lastLog = AuditService.GetLogs(content.Id).Last();
        Assert.AreEqual("Unpublished languages: fr-FR", lastLog.Comment);

        // re-get
        content = ContentService.GetById(content.Id);
        content.SetCultureName("content-en", langGb.IsoCode);
        unpublished = ContentService.Unpublish(content, langGb.IsoCode);

        // audit log will only show that english was published
        var logs = AuditService.GetLogs(content.Id).ToList();
        Assert.AreEqual("Unpublished languages: en-GB", logs[^2].Comment);
        Assert.AreEqual("Unpublished (mandatory language unpublished)", logs[^1].Comment);
    }

    [Test]
    public void Can_Publish_Content_1()
    {
        // Arrange
        var content = ContentService.GetById(Textpage.Id);
        Assert.IsNotNull(content);

        // Act
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

        // Assert
        Assert.That(published.Success, Is.True);
        Assert.That(content.Published, Is.True);
    }

    [Test]
    public void Can_Publish_Content_2()
    {
        // Arrange
        var content = ContentService.GetById(Textpage.Id);
        Assert.IsNotNull(content);

        // Act
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: -1);

        // Assert
        Assert.That(published.Success, Is.True);
        Assert.That(content.Published, Is.True);
    }

    [Test]
    public void IsPublishable()
    {
        // Arrange
        var parent = ContentService.Create("parent", Constants.System.Root, "umbTextpage");

        ContentService.Save(parent);
        ContentService.Publish(parent, parent.AvailableCultures.ToArray());
        var content = ContentService.Create("child", parent, "umbTextpage");
        ContentService.Save(content);

        Assert.IsTrue(ContentService.IsPathPublishable(content));
        ContentService.Unpublish(parent);
        Assert.IsFalse(ContentService.IsPathPublishable(content));
    }

    [Test]
    public void Can_Publish_Content_WithEvents()
    {
        var savingWasCalled = false;
        var publishingWasCalled = false;

        ContentNotificationHandler.SavingContent = notification =>
        {
            Assert.AreEqual(1, notification.SavedEntities.Count());
            var entity = notification.SavedEntities.First();
            Assert.AreEqual("foo", entity.Name);

            var e = ContentService.GetById(entity.Id);
            Assert.AreEqual("Textpage", e.Name);

            savingWasCalled = true;
        };

        ContentNotificationHandler.PublishingContent = notification =>
        {
            Assert.AreEqual(1, notification.PublishedEntities.Count());
            var entity = notification.PublishedEntities.First();
            Assert.AreEqual("foo", entity.Name);

            publishingWasCalled = true;
        };

        try
        {
            var content = ContentService.GetById(Textpage.Id);
            Assert.AreEqual("Textpage", content.Name);

            content.Name = "foo";
            ContentService.Save(content);
            var published =
                ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

            Assert.That(published.Success, Is.True);
            Assert.That(content.Published, Is.True);

            var e = ContentService.GetById(content.Id);
            Assert.AreEqual("foo", e.Name);

            Assert.IsTrue(savingWasCalled);
            Assert.IsTrue(publishingWasCalled);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
            ContentNotificationHandler.PublishingContent = null;
        }
    }

    [Test]
    public void Can_Not_Publish_Invalid_Cultures_For_Variant_Content()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("Name for en-US", "en-US");
        ContentService.Save(content);

        Assert.Throws<ArgumentNullException>(() => ContentService.Publish(content, null!));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new string[] { null }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new [] { string.Empty }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "*", null }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "en-US", "*" }));
    }

    [Test]
    public void Can_Not_Publish_Invalid_Cultures_For_Invariant_Content()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.Name = "Content name";
        ContentService.Save(content);

        Assert.Throws<ArgumentNullException>(() => ContentService.Publish(content, null!));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new string[] { null }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "*", null }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "en-US" }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "en-US", "*" }));
    }

    [Test]
    public void Can_Publish_Only_Valid_Content()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var parentId = Textpage.Id;

        var parent = ContentService.GetById(parentId);

        ContentService.Save(parent);
        var parentPublished = ContentService.Publish(parent, parent.AvailableCultures.ToArray());

        // parent can publish values
        // and therefore can be published
        Assert.IsTrue(parentPublished.Success);
        Assert.IsTrue(parent.Published);

        var content = ContentBuilder.CreateSimpleContent(contentType, "Invalid Content", parentId);
        content.SetValue("author", string.Empty);
        Assert.IsFalse(content.HasIdentity);

        // content cannot publish values because they are invalid
        var propertyValidationService = new PropertyValidationService(PropertyEditorCollection, DataTypeService, LocalizedTextService, ValueEditorCache, Mock.Of<ICultureDictionary>());
        var isValid = propertyValidationService.IsPropertyDataValid(content, out var invalidProperties,
            CultureImpact.Invariant);
        Assert.IsFalse(isValid);
        Assert.IsNotEmpty(invalidProperties);

        // and therefore cannot be published,
        // because it did not have a published version at all
        ContentService.Save(content);
        var contentPublished = ContentService.Publish(content, content.AvailableCultures.ToArray());
        Assert.IsFalse(contentPublished.Success);
        Assert.AreEqual(PublishResultType.FailedPublishContentInvalid, contentPublished.Result);
        Assert.IsFalse(content.Published);

        // Ensure it saved though
        Assert.Greater(content.Id, 0);
        Assert.IsTrue(content.HasIdentity);
    }

    [Test]
    public async Task Can_Publish_And_Unpublish_Cultures_In_Single_Operation()
    {
        // TODO: This is using an internal API - we aren't exposing this publicly (at least for now) but we'll keep the test around
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr")
            .Build();
        var langDa = new LanguageBuilder()
            .WithCultureInfo("da")
            .Build();
        await LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var ct = ContentTypeBuilder.CreateBasicContentType();
        ct.Variations = ContentVariation.Culture;
        ContentTypeService.Save(ct);

        IContent content = ContentBuilder.CreateBasicContent(ct);
        content.SetCultureName("name-fr", langFr.IsoCode);
        content.SetCultureName("name-da", langDa.IsoCode);

        content.PublishCulture(CultureImpact.Explicit(langFr.IsoCode, langFr.IsDefault), DateTime.Now, PropertyEditorCollection);
        var result = ContentService.CommitDocumentChanges(content);
        Assert.IsTrue(result.Success);
        content = ContentService.GetById(content.Id);
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langDa.IsoCode));

        content.UnpublishCulture(langFr.IsoCode);
        content.PublishCulture(CultureImpact.Explicit(langDa.IsoCode, langDa.IsDefault), DateTime.Now, PropertyEditorCollection);

        result = ContentService.CommitDocumentChanges(content);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(PublishResultType.SuccessMixedCulture, result.Result);

        content = ContentService.GetById(content.Id);
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
        var parentId = Textpage.Id;

        var parent = ContentService.GetById(parentId);

        Console.WriteLine(" " + parent.Id);
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            var descendants =
                ContentService.GetPagedDescendants(parent.Id, page++, pageSize, out total);
            foreach (var x in descendants)
            {
                Console.WriteLine("          "[..x.Level] + x.Id);
            }
        }

        Console.WriteLine();

        // publish parent & its branch
        // only those that are not already published
        // only invariant/neutral values
        var parentPublished = ContentService.PublishBranch(parent, PublishBranchFilter.IncludeUnpublished, parent.AvailableCultures.ToArray());

        foreach (var result in parentPublished)
        {
            Console.WriteLine("          "[..result.Content.Level] +
                              $"{result.Content.Id}: {result.Result}");
        }

        // everything should be successful
        Assert.IsTrue(parentPublished.All(x => x.Success));
        Assert.IsTrue(parent.Published);

        var
            children = ContentService.GetPagedChildren(parentId, 0, 500,
                out var totalChildren); // we only want the first so page size, etc.. is abitrary

        // children are published including ... that was released 5 mins ago
        Assert.IsTrue(children.First(x => x.Id == Subpage.Id).Published);
    }

    [Test]
    public void Cannot_Publish_Expired_Content()
    {
        // Arrange
        var content = ContentService.GetById(Subpage.Id); // This Content expired 5min ago
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddMinutes(-5));
        ContentService.Save(content, contentSchedule: contentSchedule);

        var parent = ContentService.GetById(Textpage.Id);
        Assert.IsNotNull(parent);
        var parentPublished =
            ContentService.Publish(parent,
                parent.AvailableCultures.ToArray(),
                userId: Constants.Security
                    .SuperUserId); // Publish root Home node to enable publishing of 'Subpage.Id'

        // Act
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

        // Assert
        Assert.That(parentPublished.Success, Is.True);
        Assert.That(published.Success, Is.False);
        Assert.That(content.Published, Is.False);
        Assert.AreEqual(PublishResultType.FailedPublishHasExpired, published.Result);
    }

    [Test]
    public void Cannot_Publish_Expired_Culture()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("Hello", "en-US");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry("en-US", null, DateTime.UtcNow.AddMinutes(-5));
        ContentService.Save(content, contentSchedule: contentSchedule);

        var published = ContentService.Publish(content, new[] { "en-US" });

        Assert.IsFalse(published.Success);
        Assert.AreEqual(PublishResultType.FailedPublishCultureHasExpired, published.Result);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public void Cannot_Publish_Content_Awaiting_Release()
    {
        // Arrange
        var content = ContentService.GetById(Subpage.Id);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddHours(2), null);
        ContentService.Save(content, Constants.Security.SuperUserId, contentSchedule);

        var parent = ContentService.GetById(Textpage.Id);
        Assert.IsNotNull(parent);
        var parentPublished =
            ContentService.Publish(parent,
                parent.AvailableCultures.ToArray(),
                userId: Constants.Security
                    .SuperUserId); // Publish root Home node to enable publishing of 'Subpage.Id'

        // Act
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

        // Assert
        Assert.That(parentPublished.Success, Is.True);
        Assert.That(published.Success, Is.False);
        Assert.That(content.Published, Is.False);
        Assert.AreEqual(PublishResultType.FailedPublishAwaitingRelease, published.Result);
    }

    [Test]
    [LongRunning]
    public void Failed_Publish_Should_Not_Update_Edited_State_When_Edited_True()
    {
        // Arrange
        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias("header")
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("header")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Home")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("header", "Cool header")
            .Done()
            .Build();

        contentService.Save(content);
        contentService.Publish(content, Array.Empty<string>());

        content.Properties[0].SetValue("Foo", string.Empty);
        contentService.Save(content);
        contentService.PersistContentSchedule(content,
            ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddHours(2), null));

        // Act
        var result = contentService.Publish(content, Array.Empty<string>(), userId: Constants.Security.SuperUserId);

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
    [LongRunning]
    public void Failed_Publish_Should_Not_Update_Edited_State_When_Edited_False()
    {
        // Arrange
        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias("header")
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("header")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Home")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("header", "Cool header")
            .Done()
            .Build();

        contentService.Save(content);
        contentService.Publish(content, Array.Empty<string>());

        contentService.PersistContentSchedule(content,
            ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddHours(2), null));
        contentService.Save(content);

        // Act
        var result = contentService.Publish(content, Array.Empty<string>(), userId: Constants.Security.SuperUserId);

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
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("Hello", "en-US");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry("en-US", DateTime.UtcNow.AddHours(2), null);
        ContentService.Save(content, contentSchedule: contentSchedule);

        var published = ContentService.Publish(content, new[] { "en-US" });

        Assert.IsFalse(published.Success);
        Assert.AreEqual(PublishResultType.FailedPublishCultureAwaitingRelease, published.Result);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public void Cannot_Publish_Content_Where_Parent_Is_Unpublished()
    {
        // Arrange
        var content = ContentService.Create("Subpage with Unpublished Parent", Textpage.Id, "umbTextpage");
        ContentService.Save(content);

        // Act
        var published = ContentService.PublishBranch(content, PublishBranchFilter.IncludeUnpublished, content.AvailableCultures.ToArray());

        // Assert
        Assert.That(published.All(x => x.Success), Is.False);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public void Cannot_Publish_Trashed_Content()
    {
        // Arrange
        var content = ContentService.GetById(Trashed.Id);
        Assert.IsNotNull(content);

        // Act
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

        // Assert
        Assert.That(published.Success, Is.False);
        Assert.That(content.Published, Is.False);
        Assert.That(content.Trashed, Is.True);
    }

    [Test]
    public void Can_Save_And_Publish_Content()
    {
        // Arrange
        var content = ContentService.Create("Home US", -1, "umbTextpage");
        content.SetValue("author", "Barack Obama");

        // Act
        var saved = ContentService.Save(content, userId: Constants.Security.SuperUserId);
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

        // Assert
        Assert.That(content.HasIdentity, Is.True);
        Assert.That(content.Published, Is.True);
        Assert.IsTrue(published.Success);
        Assert.IsTrue(saved.Success);
    }

    /// <summary>
    ///     Try to immitate a new child content item being created through the UI.
    ///     This content item will have no Id, Path or Identity.
    ///     It seems like this is wiped somewhere in the process when creating an item through the UI
    ///     and we need to make sure we handle nullchecks for these properties when creating content.
    ///     This is unfortunately not caught by the normal ContentService tests.
    /// </summary>
    [Test]
    public void Can_Save_And_Publish_Content_And_Child_Without_Identity()
    {
        // Arrange
        var content = ContentService.Create("Home US", Constants.System.Root, "umbTextpage");
        content.SetValue("author", "Barack Obama");

        // Act
        var saved = ContentService.Save(content, userId: Constants.Security.SuperUserId);
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);
        var childContent = ContentService.Create("Child", content.Id, "umbTextpage");

        // Reset all identity properties
        childContent.Id = 0;
        childContent.Path = null;
        ((Content)childContent).ResetIdentity();
        var childSaved = ContentService.Save(childContent, userId: Constants.Security.SuperUserId);
        var childPublished =
            ContentService.Publish(childContent, childContent.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

        // Assert
        Assert.That(content.HasIdentity, Is.True);
        Assert.That(content.Published, Is.True);
        Assert.That(childContent.HasIdentity, Is.True);
        Assert.That(childContent.Published, Is.True);
        Assert.That(published.Success, Is.True);
        Assert.That(childPublished.Success, Is.True);
        Assert.That(saved.Success, Is.True);
        Assert.That(childSaved.Success, Is.True);
    }

    [Test]
    [LongRunning]
    public void Can_Get_Published_Descendant_Versions()
    {
        // Arrange
        var root = ContentService.GetById(Textpage.Id)!;
        var rootPublished = ContentService.Publish(root, root.AvailableCultures.ToArray());

        var content = ContentService.GetById(Subpage.Id);
        content.Properties["title"].SetValue(content.Properties["title"].GetValue() + " Published");
        ContentService.Save(content);
        var contentPublished = ContentService.Publish(content, content.AvailableCultures.ToArray());
        var publishedVersion = content.VersionId;

        content.Properties["title"].SetValue(content.Properties["title"].GetValue() + " Saved");
        ContentService.Save(content);
        Assert.AreEqual(publishedVersion, content.VersionId);

        // Act
        var publishedDescendants = ContentService.GetPublishedDescendants(root).ToList();
        Assert.AreNotEqual(0, publishedDescendants.Count);

        // Assert
        Assert.IsTrue(rootPublished.Success);
        Assert.IsTrue(contentPublished.Success);

        // Console.WriteLine(publishedVersion);
        // foreach (var d in publishedDescendants) Console.WriteLine(d.Version);
        Assert.IsTrue(publishedDescendants.Any(x => x.VersionId == publishedVersion));

        // Ensure that the published content version has the correct property value and is marked as published
        var publishedContentVersion = publishedDescendants.First(x => x.VersionId == publishedVersion);
        Assert.That(publishedContentVersion.Published, Is.True);
        Assert.That(publishedContentVersion.Properties["title"].GetValue(published: true),
            Contains.Substring("Published"));

        // and has the correct draft properties
        Assert.That(publishedContentVersion.Properties["title"].GetValue(), Contains.Substring("Saved"));

        // Ensure that the latest version of the content is ok
        var currentContent = ContentService.GetById(Subpage.Id);
        Assert.That(currentContent.Published, Is.True);
        Assert.That(currentContent.Properties["title"].GetValue(published: true), Contains.Substring("Published"));
        Assert.That(currentContent.Properties["title"].GetValue(), Contains.Substring("Saved"));
        Assert.That(currentContent.VersionId, Is.EqualTo(publishedContentVersion.VersionId));
    }

    [Test]
    public void Can_Save_Content()
    {
        // Arrange
        var content = ContentService.Create("Home US", -1, "umbTextpage");
        content.SetValue("author", "Barack Obama");

        // Act
        ContentService.Save(content);

        // Assert
        Assert.That(content.HasIdentity, Is.True);
    }

    [Test]
    public void Can_Update_Content_Property_Values()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        IContentType contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        IContent content = ContentBuilder.CreateSimpleContent(contentType, "hello");
        content.SetValue("title", "title of mine");
        content.SetValue("bodyText", "hello world");
        ContentService.Save(content);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // re-get
        content = ContentService.GetById(content.Id);
        content.SetValue("title", "another title of mine"); // Change a value
        content.SetValue("bodyText", null); // Clear a value
        content.SetValue("author", "new author"); // Add a value
        ContentService.Save(content);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.AreEqual("another title of mine", content.GetValue("title"));
        Assert.IsNull(content.GetValue("bodyText"));
        Assert.AreEqual("new author", content.GetValue("author"));

        content.SetValue("title", "new title");
        content.SetValue("bodyText", "new body text");
        content.SetValue("author", "new author text");
        ContentService.Save(content); // new non-published version

        // re-get
        content = ContentService.GetById(content.Id);
        content.SetValue("title", null); // Clear a value
        content.SetValue("bodyText", null); // Clear a value
        ContentService.Save(content); // saving non-published version

        // re-get
        content = ContentService.GetById(content.Id);
        Assert.IsNull(content.GetValue("title")); // Test clearing the value worked with the non-published version
        Assert.IsNull(content.GetValue("bodyText"));
        Assert.AreEqual("new author text", content.GetValue("author"));

        // make sure that the published version remained the same
        var publishedContent = ContentService.GetVersion(content.PublishedVersionId);
        Assert.AreEqual("another title of mine", publishedContent.GetValue("title"));
        Assert.IsNull(publishedContent.GetValue("bodyText"));
        Assert.AreEqual("new author", publishedContent.GetValue("author"));
    }

    [Test]
    public void Can_Bulk_Save_Content()
    {
        // Arrange
        var contentType = ContentTypeService.Get("umbTextpage");
        var subpage = ContentBuilder.CreateSimpleContent(contentType, "Text Subpage 1", Textpage.Id);
        var subpage2 = ContentBuilder.CreateSimpleContent(contentType, "Text Subpage 2", Textpage.Id);
        var list = new List<IContent> { subpage, subpage2 };

        // Act
        ContentService.Save(list);

        // Assert
        Assert.That(list.Any(x => !x.HasIdentity), Is.False);
    }

    [Test]
    public void Can_Bulk_Save_New_Hierarchy_Content()
    {
        // Arrange
        var hierarchy = CreateContentHierarchy().ToList();

        // Act
        ContentService.Save(hierarchy);

        Assert.That(hierarchy.Any(), Is.True);
        Assert.That(hierarchy.Any(x => x.HasIdentity == false), Is.False);

        // all parent id's should be ok, they are lazy and if they equal zero an exception will be thrown
        Assert.DoesNotThrow(() => hierarchy.Any(x => x.ParentId != 0));
    }

    [Test]
    public void Can_Delete_Content_Of_Specific_ContentType()
    {
        // Arrange
        var contentType = ContentTypeService.Get("umbTextpage");

        // Act
        ContentService.DeleteOfType(contentType.Id);
        var rootContent = ContentService.GetRootContent();
        var contents = ContentService.GetPagedOfType(contentType.Id, 0, int.MaxValue, out var _);

        // Assert
        Assert.That(rootContent.Any(), Is.False);
        Assert.That(contents.Any(x => !x.Trashed), Is.False);
    }

    [Test]
    public void Can_Delete_Content()
    {
        // Arrange
        var content = ContentService.GetById(Textpage.Id);

        // Act
        ContentService.Delete(content);
        var deleted = ContentService.GetById(Textpage.Id);

        // Assert
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public void Can_Move_Content_To_RecycleBin()
    {
        // Arrange
        var content = ContentService.GetById(Textpage.Id);

        // Act
        ContentService.MoveToRecycleBin(content);

        // Assert
        Assert.That(content.ParentId, Is.EqualTo(-20));
        Assert.That(content.Trashed, Is.True);
    }

    [Test]
    [LongRunning]
    public void Can_Move_Content_Structure_To_RecycleBin_And_Empty_RecycleBin()
    {
        var contentType = ContentTypeService.Get("umbTextpage");

        var subsubpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 3", Subpage.Id);
        ContentService.Save(subsubpage);

        var content = ContentService.GetById(Textpage.Id);
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        var descendants = new List<IContent>();
        while (page * pageSize < total)
        {
            descendants.AddRange(ContentService.GetPagedDescendants(content.Id, page++, pageSize, out total));
        }

        Assert.AreNotEqual(-20, content.ParentId);
        Assert.IsFalse(content.Trashed);
        Assert.AreEqual(4, descendants.Count);
        Assert.IsFalse(descendants.Any(x => x.Path.StartsWith("-1,-20,")));
        Assert.IsFalse(descendants.Any(x => x.Trashed));

        ContentService.MoveToRecycleBin(content);

        descendants.Clear();
        page = 0;
        while (page * pageSize < total)
        {
            descendants.AddRange(ContentService.GetPagedDescendants(content.Id, page++, pageSize, out total));
        }

        Assert.AreEqual(-20, content.ParentId);
        Assert.IsTrue(content.Trashed);
        Assert.AreEqual(4, descendants.Count);
        Assert.IsTrue(descendants.All(x => x.Path.StartsWith("-1,-20,")));
        Assert.True(descendants.All(x => x.Trashed));

        ContentService.EmptyRecycleBin();
        var trashed = ContentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var _).ToList();
        Assert.IsEmpty(trashed);
    }

    [Test]
    public void Can_Empty_RecycleBin()
    {
        // Arrange
        // Act
        ContentService.EmptyRecycleBin();
        var contents = ContentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var _).ToList();

        // Assert
        Assert.That(contents.Any(), Is.False);
    }

    [Test]
    [LongRunning]
    public async Task Ensures_Permissions_Are_Retained_For_Copied_Descendants_With_Explicit_Permissions()
    {
        // Arrange
        var userGroup = UserGroupBuilder.CreateUserGroup("1");
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);
        contentType.AllowedContentTypes = new List<ContentTypeSort>
        {
            new(contentType.Key, 0, contentType.Alias)
        };
        ContentTypeService.Save(contentType);

        var parentPage = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parentPage);

        var childPage = ContentBuilder.CreateSimpleContent(contentType, "child", parentPage);
        ContentService.Save(childPage);

        // assign explicit permissions to the child
        ContentService.SetPermission(childPage, "A", new[] { userGroup.Id });

        // Ok, now copy, what should happen is the childPage will retain it's own permissions
        var parentPage2 = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parentPage2);

        var copy = ContentService.Copy(childPage, parentPage2.Id, false, true);

        // get the permissions and verify
        var permissions = UserService.GetPermissionsForPath(userGroup, copy.Path, true);
        var allPermissions = permissions.GetAllPermissions().ToArray();
        Assert.AreEqual(1, allPermissions.Length);
        Assert.AreEqual("A", allPermissions[0]);
    }

    [Test]
    [LongRunning]
    public async Task Ensures_Permissions_Are_Inherited_For_Copied_Descendants()
    {
        // Arrange
        var userGroup = UserGroupBuilder.CreateUserGroup("1");
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);
        contentType.AllowedContentTypes = new List<ContentTypeSort>
        {
            new(contentType.Key, 0, contentType.Alias)
        };
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var parentPage = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parentPage);
        ContentService.SetPermission(parentPage, "A", new[] { userGroup.Id });

        var childPage1 = ContentBuilder.CreateSimpleContent(contentType, "child1", parentPage);
        ContentService.Save(childPage1);
        var childPage2 = ContentBuilder.CreateSimpleContent(contentType, "child2", childPage1);
        ContentService.Save(childPage2);
        var childPage3 = ContentBuilder.CreateSimpleContent(contentType, "child3", childPage2);
        ContentService.Save(childPage3);

        // Verify that the children have the inherited permissions
        var descendants = new List<IContent>();
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            descendants.AddRange(ContentService.GetPagedDescendants(parentPage.Id, page++, pageSize, out total));
        }

        Assert.AreEqual(3, descendants.Count);

        foreach (var descendant in descendants)
        {
            var permissions = UserService.GetPermissionsForPath(userGroup, descendant.Path, true);
            var allPermissions = permissions.GetAllPermissions().ToArray();
            Assert.AreEqual(1, allPermissions.Length);
            Assert.AreEqual("A", allPermissions[0]);
        }

        // create a new parent with a new permission structure
        var parentPage2 = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parentPage2);
        ContentService.SetPermission(parentPage2, "B", new[] { userGroup.Id });

        // Now copy, what should happen is the child pages will now have permissions inherited from the new parent
        var copy = ContentService.Copy(childPage1, parentPage2.Id, false, true);

        descendants.Clear();
        page = 0;
        while (page * pageSize < total)
        {
            descendants.AddRange(ContentService.GetPagedDescendants(parentPage2.Id, page++, pageSize, out total));
        }

        Assert.AreEqual(3, descendants.Count);

        foreach (var descendant in descendants)
        {
            var permissions = UserService.GetPermissionsForPath(userGroup, descendant.Path, true);
            var allPermissions = permissions.GetAllPermissions().ToArray();
            Assert.AreEqual(1, allPermissions.Length);
            Assert.AreEqual("B", allPermissions[0]);
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Empty_RecycleBin_With_Content_That_Has_All_Related_Data()
    {
        // Arrange
        // need to:
        // * add relations
        // * add permissions
        // * add notifications
        // * public access
        // * tags
        // * domain
        // * published & preview data
        // * multiple versions
        var contentType = ContentTypeBuilder.CreateAllTypesContentType("test", "test");
        ContentTypeService.Save(contentType);

        object obj =
            new { tags = "[\"Hello\",\"World\"]" };
        var content1 = ContentBuilder.CreateBasicContent(contentType);
        content1.PropertyValues(obj);
        content1.ResetDirtyProperties(false);
        ContentService.Save(content1);
        Assert.IsTrue(ContentService.Publish(content1, content1.AvailableCultures.ToArray(), userId: -1).Success);
        var content2 = ContentBuilder.CreateBasicContent(contentType);
        content2.PropertyValues(obj);
        content2.ResetDirtyProperties(false);
        ContentService.Save(content2);
        Assert.IsTrue(ContentService.Publish(content2, content2.AvailableCultures.ToArray(), userId: -1).Success);

        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);
        editorGroup.StartContentId = content1.Id;
        await UserGroupService.UpdateAsync(editorGroup, Constants.Security.SuperUserKey);

        var admin = await UserService.GetAsync(Constants.Security.SuperUserKey);
        admin.StartContentIds = new[] { content1.Id };
        UserService.Save(admin);

        RelationService.Save(new RelationType("test", "test", false, Constants.ObjectTypes.Document,
            Constants.ObjectTypes.Document, false));
        Assert.IsNotNull(RelationService.Relate(content1, content2, "test"));

        PublicAccessService.Save(new PublicAccessEntry(content1, content2, content2,
            new List<PublicAccessRule> { new() { RuleType = "test", RuleValue = "test" } }));
        Assert.IsTrue(PublicAccessService.AddRule(content1, "test2", "test2").Success);

        var user = await UserService.GetAsync(Constants.Security.SuperUserKey);
        var userGroup = await UserGroupService.GetAsync(user.Groups.First().Alias);
        Assert.IsNotNull(NotificationService.CreateNotification(user, content1, "X"));

        ContentService.SetPermission(content1, "A", new[] { userGroup.Id });
        var updateDomainResult = await DomainService.UpdateDomainsAsync(
            content1.Key,
            new DomainsUpdateModel
            {
                Domains = new[] { new DomainModel { DomainName = "www.test.com", IsoCode = "en-US" } }
            });
        Assert.IsTrue(updateDomainResult.Success);

        // Act
        ContentService.MoveToRecycleBin(content1);
        ContentService.EmptyRecycleBin();
        var contents = ContentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var _).ToList();

        // Assert
        Assert.That(contents.Any(), Is.False);
    }

    [Test]
    public void Can_Move_Content()
    {
        // Arrange
        var content = ContentService.GetById(Trashed.Id);

        // Act - moving out of recycle bin
        ContentService.Move(content, Textpage.Id);

        // Assert
        Assert.That(content.ParentId, Is.EqualTo(Textpage.Id));
        Assert.That(content.Trashed, Is.False);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public void Can_Copy_Content()
    {
        // Arrange
        var temp = ContentService.GetById(Subpage.Id);

        // Act
        var copy = ContentService.Copy(temp, temp.ParentId, false);
        var content = ContentService.GetById(Subpage.Id);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
        Assert.AreNotSame(content, copy);
        foreach (var property in copy.Properties)
        {
            Assert.AreEqual(property.GetValue(), content.Properties[property.Alias].GetValue());
        }

        // Assert.AreNotEqual(content.Name, copy.Name);
    }

    [Test]
    public void Can_Copy_And_Modify_Content_With_Events()
    {
        // see https://github.com/umbraco/Umbraco-CMS/issues/5513

        var copyingWasCalled = false;
        var copiedWasCalled = false;

        ContentNotificationHandler.CopyingContent = notification =>
        {
            notification.Copy.SetValue("title", "1");
            notification.Original.SetValue("title", "2");

            copyingWasCalled = true;
        };

        ContentNotificationHandler.CopiedContent = notification =>
        {
            var copyVal = notification.Copy.GetValue<string>("title");
            var origVal = notification.Original.GetValue<string>("title");

            Assert.AreEqual("1", copyVal);
            Assert.AreEqual("2", origVal);

            copiedWasCalled = true;
        };

        try
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
            ContentTypeService.Save(contentType);
            var content = ContentBuilder.CreateSimpleContent(contentType);
            content.SetValue("title", "New Value");
            ContentService.Save(content);

            var copy = ContentService.Copy(content, content.ParentId, false);
            Assert.AreEqual("1", copy.GetValue("title"));

            Assert.IsTrue(copyingWasCalled);
            Assert.IsTrue(copiedWasCalled);
        }
        finally
        {
            ContentNotificationHandler.CopyingContent = null;
            ContentNotificationHandler.CopiedContent = null;
        }
    }

    [Test]
    public void Can_Copy_Recursive()
    {
        // Arrange
        var temp = ContentService.GetById(Textpage.Id);
        Assert.AreEqual("Textpage", temp.Name);
        Assert.AreEqual(3, ContentService.CountChildren(temp.Id));

        // Act
        var copy = ContentService.Copy(temp, temp.ParentId, false, true);
        var content = ContentService.GetById(Textpage.Id);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
        Assert.AreNotSame(content, copy);
        Assert.AreEqual(3, ContentService.CountChildren(copy.Id));

        var child = ContentService.GetById(Subpage.Id);
        var childCopy = ContentService.GetPagedChildren(copy.Id, 0, 500, out var total).First();
        Assert.AreEqual(childCopy.Name, child.Name);
        Assert.AreNotEqual(childCopy.Id, child.Id);
        Assert.AreNotEqual(childCopy.Key, child.Key);
    }

    [Test]
    public void Can_Copy_NonRecursive()
    {
        // Arrange
        var temp = ContentService.GetById(Textpage.Id);
        Assert.AreEqual("Textpage", temp.Name);
        Assert.AreEqual(3, ContentService.CountChildren(temp.Id));

        // Act
        var copy = ContentService.Copy(temp, temp.ParentId, false, false);
        var content = ContentService.GetById(Textpage.Id);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
        Assert.AreNotSame(content, copy);
        Assert.AreEqual(0, ContentService.CountChildren(copy.Id));
    }

    [Test]
    public void Can_Copy_Content_With_Tags()
    {
        const string propAlias = "tags";

        // create a content type that has a 'tags' property
        // the property needs to support tags, else nothing works of course!
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType =
            ContentTypeBuilder.CreateSimpleTagsContentType("umbTagsPage", "TagsPage",
                defaultTemplateId: template.Id);
        contentType.Key = new Guid("78D96D30-1354-4A1E-8450-377764200C58");
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType, "Simple Tags Page");
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, propAlias,
            new[] { "hello", "world" });
        ContentService.Save(content);

        // value has been set but no tags have been created (not published)
        Assert.AreEqual("[\"hello\",\"world\"]", content.GetValue(propAlias));
        var contentTags = TagService.GetTagsForEntity(content.Id).ToArray();
        Assert.AreEqual(0, contentTags.Length);

        // reloading the content yields the same result
        content = (Content)ContentService.GetById(content.Id);
        Assert.AreEqual("[\"hello\",\"world\"]", content.GetValue(propAlias));
        contentTags = TagService.GetTagsForEntity(content.Id).ToArray();
        Assert.AreEqual(0, contentTags.Length);

        // publish
        ContentService.Publish(content, new []{ "*" });

        // now tags have been set (published)
        Assert.AreEqual("[\"hello\",\"world\"]", content.GetValue(propAlias));
        contentTags = TagService.GetTagsForEntity(content.Id).ToArray();
        Assert.AreEqual(2, contentTags.Length);

        // copy
        var copy = ContentService.Copy(content, content.ParentId, false);

        // copy is not published, so property has value, but no tags have been created
        Assert.AreEqual("[\"hello\",\"world\"]", copy.GetValue(propAlias));
        var copiedTags = TagService.GetTagsForEntity(copy.Id).ToArray();
        Assert.AreEqual(0, copiedTags.Length);

        // publish
        ContentService.Publish(copy, new []{ "*" });

        // now tags have been set (published)
        copiedTags = TagService.GetTagsForEntity(copy.Id).ToArray();

        Assert.AreEqual(2, copiedTags.Length);
        Assert.AreEqual("hello", copiedTags[0].Text);
        Assert.AreEqual("world", copiedTags[1].Text);
    }

    [Test]
    public void Can_Rollback_Version_On_Content()
    {
        // Arrange
        var parent = ContentService.GetById(Textpage.Id);
        Assert.IsFalse(parent.Published);
        ContentService.Save(parent);
        ContentService.Publish(parent, parent.AvailableCultures.ToArray()); // publishing parent, so Text Page 2 can be updated.

        var content = ContentService.GetById(Subpage.Id);
        Assert.IsFalse(content.Published);

        var versions = ContentService.GetVersions(Subpage.Id).ToList();
        Assert.AreEqual(1, versions.Count);

        var version1 = content.VersionId;

        content.Name = "Text Page 2 Updated";
        content.SetValue("author", "Francis Doe");

        // non published = edited
        Assert.IsTrue(content.Edited);

        ContentService.Save(content);
        ContentService.Publish(content, content.AvailableCultures.ToArray()); // new version
        var version2 = content.VersionId;
        Assert.AreNotEqual(version1, version2);

        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);
        Assert.AreEqual("Francis Doe",
            ContentService.GetById(content.Id).GetValue<string>("author")); // version2 author is Francis

        Assert.AreEqual("Text Page 2 Updated", content.Name);
        Assert.AreEqual("Text Page 2 Updated", content.PublishName);

        content.Name = "Text Page 2 ReUpdated";
        content.SetValue("author", "Jane Doe");

        // is not actually 'edited' until changes have been saved
        Assert.IsFalse(content.Edited);
        ContentService.Save(content); // just save changes
        Assert.IsTrue(content.Edited);

        Assert.AreEqual("Text Page 2 ReUpdated", content.Name);
        Assert.AreEqual("Text Page 2 Updated", content.PublishName);

        content.Name = "Text Page 2 ReReUpdated";

        ContentService.Save(content);
        ContentService.Publish(content, content.AvailableCultures.ToArray()); // new version
        var version3 = content.VersionId;
        Assert.AreNotEqual(version2, version3);

        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);
        Assert.AreEqual("Jane Doe",
            ContentService.GetById(content.Id).GetValue<string>("author")); // version3 author is Jane

        Assert.AreEqual("Text Page 2 ReReUpdated", content.Name);
        Assert.AreEqual("Text Page 2 ReReUpdated", content.PublishName);

        // here we have
        // version1, first published version
        // version2, second published version
        // version3, third and current published version

        // rollback all values to version1
        var rollback = ContentService.GetById(Subpage.Id);
        var rollto = ContentService.GetVersion(version1);
        rollback.CopyFrom(rollto);
        rollback.Name = rollto.Name; // must do it explicitly
        ContentService.Save(rollback);

        Assert.IsNotNull(rollback);
        Assert.IsTrue(rollback.Published);
        Assert.IsTrue(rollback.Edited);
        Assert.AreEqual("Francis Doe",
            ContentService.GetById(content.Id).GetValue<string>("author")); // author is now Francis again
        Assert.AreEqual(version3, rollback.VersionId); // same version but with edits

        // props and name have rolled back
        Assert.AreEqual("Francis Doe", rollback.GetValue<string>("author"));
        Assert.AreEqual("Text Page 2 Updated", rollback.Name);

        // published props and name are still there
        Assert.AreEqual("Jane Doe", rollback.GetValue<string>("author", published: true));
        Assert.AreEqual("Text Page 2 ReReUpdated", rollback.PublishName);

        // rollback all values to current version
        // special because... current has edits... this really is equivalent to rolling back to version2
        var rollback2 = ContentService.GetById(Subpage.Id);
        var rollto2 = ContentService.GetVersion(version3);
        rollback2.CopyFrom(rollto2);
        rollback2.Name = rollto2.PublishName; // must do it explicitely AND must pick the publish one!
        ContentService.Save(rollback2);

        Assert.IsTrue(rollback2.Published);
        Assert.IsTrue(rollback2.Edited); // Still edited, change of behaviour

        Assert.AreEqual("Jane Doe", rollback2.GetValue<string>("author"));
        Assert.AreEqual("Text Page 2 ReReUpdated", rollback2.Name);

        // test rollback to self, again
        content = ContentService.GetById(content.Id);
        Assert.AreEqual("Text Page 2 ReReUpdated", content.Name);
        Assert.AreEqual("Jane Doe", content.GetValue<string>("author"));
        ContentService.Save(content);
        ContentService.Publish(content, content.AvailableCultures.ToArray());
        Assert.IsFalse(content.Edited);
        content.Name = "Xxx";
        content.SetValue("author", "Bob Doe");
        ContentService.Save(content);
        Assert.IsTrue(content.Edited);
        rollto = ContentService.GetVersion(content.VersionId);
        content.CopyFrom(rollto);
        content.Name = rollto.PublishName; // must do it explicitely AND must pick the publish one!
        ContentService.Save(content);
        Assert.IsTrue(content.Edited); //Still edited, change of behaviour
        Assert.AreEqual("Text Page 2 ReReUpdated", content.Name);
        Assert.AreEqual("Jane Doe", content.GetValue("author"));
    }

    [Test]
    [LongRunning]
    public async Task Can_Rollback_Version_On_Multilingual()
    {
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr")
            .Build();
        var langDa = new LanguageBuilder()
            .WithCultureInfo("da")
            .Build();
        await LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("multi", "Multi", defaultTemplateId: template.Id);
        contentType.Key = new Guid("45FF9A70-9C5F-448D-A476-DCD23566BBF8");
        contentType.Variations = ContentVariation.Culture;
        var p1 = contentType.PropertyTypes.First();
        p1.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        var page = new Content("Page", Constants.System.Root, contentType)
        {
            Level = 1,
            SortOrder = 1,
            CreatorId = 0,
            WriterId = 0,
            Key = new Guid("D7B84CC9-14AE-4D92-A042-023767AD3304")
        };

        page.SetCultureName("fr1", langFr.IsoCode);
        page.SetCultureName("da1", langDa.IsoCode);
        Thread.Sleep(1);
        ContentService.Save(page);
        var versionId0 = page.VersionId;

        page.SetValue(p1.Alias, "v1fr", langFr.IsoCode);
        page.SetValue(p1.Alias, "v1da", langDa.IsoCode);
        Thread.Sleep(1);
        ContentService.Save(page);
        ContentService.Publish(page, page.AvailableCultures.ToArray());
        var versionId1 = page.VersionId;

        Thread.Sleep(10);

        page.SetCultureName("fr2", langFr.IsoCode);
        page.SetValue(p1.Alias, "v2fr", langFr.IsoCode);
        Thread.Sleep(1);
        ContentService.Save(page);
        ContentService.Publish(page, new[] { langFr.IsoCode });
        var versionId2 = page.VersionId;

        Thread.Sleep(10);

        page.SetCultureName("da2", langDa.IsoCode);
        page.SetValue(p1.Alias, "v2da", langDa.IsoCode);
        Thread.Sleep(1);
        ContentService.Save(page);
        ContentService.Publish(page, new[] { langDa.IsoCode });
        var versionId3 = page.VersionId;

        Thread.Sleep(10);

        page.SetCultureName("fr3", langFr.IsoCode);
        page.SetCultureName("da3", langDa.IsoCode);
        page.SetValue(p1.Alias, "v3fr", langFr.IsoCode);
        page.SetValue(p1.Alias, "v3da", langDa.IsoCode);
        Thread.Sleep(1);
        ContentService.Save(page);
        ContentService.Publish(page, page.AvailableCultures.ToArray());
        var versionId4 = page.VersionId;

        // now get all versions
        var versions = ContentService.GetVersions(page.Id).ToArray();

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

        Assert.AreEqual("fr3", versions[4].GetPublishName(langFr.IsoCode));
        Assert.AreEqual("fr3", versions[3].GetPublishName(langFr.IsoCode));
        Assert.AreEqual("fr3", versions[2].GetPublishName(langFr.IsoCode));
        Assert.AreEqual("fr3", versions[1].GetPublishName(langFr.IsoCode));
        Assert.AreEqual("fr3", versions[0].GetPublishName(langFr.IsoCode));

        Assert.AreEqual("fr1", versions[4].GetCultureName(langFr.IsoCode));
        Assert.AreEqual("fr2", versions[3].GetCultureName(langFr.IsoCode));
        Assert.AreEqual("fr2", versions[2].GetCultureName(langFr.IsoCode));
        Assert.AreEqual("fr3", versions[1].GetCultureName(langFr.IsoCode));
        Assert.AreEqual("fr3", versions[0].GetCultureName(langFr.IsoCode));

        Assert.AreEqual("da3", versions[4].GetPublishName(langDa.IsoCode));
        Assert.AreEqual("da3", versions[3].GetPublishName(langDa.IsoCode));
        Assert.AreEqual("da3", versions[2].GetPublishName(langDa.IsoCode));
        Assert.AreEqual("da3", versions[1].GetPublishName(langDa.IsoCode));
        Assert.AreEqual("da3", versions[0].GetPublishName(langDa.IsoCode));

        Assert.AreEqual("da1", versions[4].GetCultureName(langDa.IsoCode));
        Assert.AreEqual("da1", versions[3].GetCultureName(langDa.IsoCode));
        Assert.AreEqual("da2", versions[2].GetCultureName(langDa.IsoCode));
        Assert.AreEqual("da3", versions[1].GetCultureName(langDa.IsoCode));
        Assert.AreEqual("da3", versions[0].GetCultureName(langDa.IsoCode));

        // all versions have the same publish infos
        for (var i = 0; i < 5; i++)
        {
            Assert.AreEqual(versions[0].PublishDate, versions[i].PublishDate);
            Assert.AreEqual(versions[0].GetPublishDate(langFr.IsoCode), versions[i].GetPublishDate(langFr.IsoCode));
            Assert.AreEqual(versions[0].GetPublishDate(langDa.IsoCode), versions[i].GetPublishDate(langDa.IsoCode));
        }

        for (var i = 0; i < 5; i++)
        {
            Console.Write("[{0}] ", i);
            Console.WriteLine(versions[i].UpdateDate.ToString("O")[11..]);
            Console.WriteLine("    fr: {0}", versions[i].GetUpdateDate(langFr.IsoCode)?.ToString("O")[11..]);
            Console.WriteLine("    da: {0}", versions[i].GetUpdateDate(langDa.IsoCode)?.ToString("O")[11..]);
        }

        Console.WriteLine("-");

        // for all previous versions, UpdateDate is the published date
        Assert.AreEqual(versions[4].UpdateDate, versions[4].GetUpdateDate(langFr.IsoCode));
        Assert.AreEqual(versions[4].UpdateDate, versions[4].GetUpdateDate(langDa.IsoCode));

        Assert.AreEqual(versions[3].UpdateDate, versions[3].GetUpdateDate(langFr.IsoCode));
        Assert.AreEqual(versions[4].UpdateDate, versions[3].GetUpdateDate(langDa.IsoCode));

        Assert.AreEqual(versions[3].UpdateDate, versions[2].GetUpdateDate(langFr.IsoCode));
        Assert.AreEqual(versions[2].UpdateDate, versions[2].GetUpdateDate(langDa.IsoCode));

        // for the published version, UpdateDate is the published date
        Assert.AreEqual(versions[1].UpdateDate, versions[1].GetUpdateDate(langFr.IsoCode));
        Assert.AreEqual(versions[1].UpdateDate, versions[1].GetUpdateDate(langDa.IsoCode));
        Assert.AreEqual(versions[1].PublishDate, versions[1].UpdateDate);

        // for the current version, things are different
        // UpdateDate is the date it was last saved
        Assert.AreEqual(versions[0].UpdateDate, versions[0].GetUpdateDate(langFr.IsoCode));
        Assert.AreEqual(versions[0].UpdateDate, versions[0].GetUpdateDate(langDa.IsoCode));

        // so if we save again...
        page.SetCultureName("fr4", langFr.IsoCode);

        // page.SetCultureName("da4", langDa.IsoCode);
        page.SetValue(p1.Alias, "v4fr", langFr.IsoCode);
        page.SetValue(p1.Alias, "v4da", langDa.IsoCode);

        // This sleep ensures the save is called on later ticks then the SetValue and SetCultureName. Therefore
        // we showcase the currect lack of handling dirty on variants on save. When this is implemented the sleep
        // helps showcase the functionality is actually working
        Thread.Sleep(5);
        ContentService.Save(page);
        var versionId5 = page.VersionId;

        versions = ContentService.GetVersions(page.Id).ToArray();

        // we just update the current version
        Assert.AreEqual(5, versions.Length);
        Assert.AreEqual(versionId4, versionId5);

        for (var i = 0; i < 5; i++)
        {
            Console.Write("[{0}] ", i);
            Console.WriteLine(versions[i].UpdateDate.ToString("O")[11..]);
            Console.WriteLine("    fr: {0}", versions[i].GetUpdateDate(langFr.IsoCode)?.ToString("O")[11..]);
            Console.WriteLine("    da: {0}", versions[i].GetUpdateDate(langDa.IsoCode)?.ToString("O")[11..]);
        }

        Console.WriteLine("-");

        var versionsSlim = ContentService.GetVersionsSlim(page.Id, 0, 50).ToArray();
        Assert.AreEqual(5, versionsSlim.Length);

        for (var i = 0; i < 5; i++)
        {
            Console.Write("[{0}]     ", i);
            Console.WriteLine(versionsSlim[i].UpdateDate.Ticks);
            Console.WriteLine("    fr: {0}", versionsSlim[i].GetUpdateDate(langFr.IsoCode)?.Ticks);
            Console.WriteLine("    da: {0}", versionsSlim[i].GetUpdateDate(langDa.IsoCode)?.Ticks);
        }

        Console.WriteLine("-");

        // what we do in the controller to get rollback versions
        var versionsSlimFr =
            versionsSlim.Where(x => x.UpdateDate == x.GetUpdateDate(langFr.IsoCode)).ToArray();

        Assert.AreEqual(4, versionsSlimFr.Length);

        // alas, at the moment we do *not* properly track 'dirty' for cultures, meaning
        // that we cannot synchronize dates the way we do with publish dates - and so this
        // would fail - the version UpdateDate is greater than the cultures'.
        Assert.AreEqual(versions[0].UpdateDate, versions[0].GetUpdateDate(langFr.IsoCode));
        Assert.AreEqual(versions[0].UpdateDate, versions[0].GetUpdateDate(langDa.IsoCode));

        // now roll french back to its very first version
        page.CopyFrom(versions[4], langFr.IsoCode); // only the pure FR values
        page.CopyFrom(versions[4], null); // so, must explicitly do the INVARIANT values too
        page.SetCultureName(versions[4].GetPublishName(langFr.IsoCode), langFr.IsoCode);
        ContentService.Save(page);

        // and voila, rolled back!
        Assert.AreEqual(versions[4].GetPublishName(langFr.IsoCode), page.GetCultureName(langFr.IsoCode));
        Assert.AreEqual(versions[4].GetValue(p1.Alias, langFr.IsoCode), page.GetValue(p1.Alias, langFr.IsoCode));

        // note that rolling back invariant values means we also rolled back... DA... at least partially
        // bah?
    }

    [Test]
    public void Can_Save_Lazy_Content()
    {
        var contentType = ContentTypeService.Get("umbTextpage");
        var root = ContentService.GetById(Textpage.Id);

        var c = new Lazy<IContent>(() =>
            ContentBuilder.CreateSimpleContent(contentType, "Hierarchy Simple Text Page", root.Id));
        var c2 = new Lazy<IContent>(() =>
            ContentBuilder.CreateSimpleContent(contentType, "Hierarchy Simple Text Subpage", c.Value.Id));
        var list = new List<Lazy<IContent>> { c, c2 };

        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = DocumentRepository;

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
        var contentTypeService = ContentTypeService;
        var contentType = ContentTypeBuilder.CreateAllTypesContentType("allDataTypes", "All DataTypes");
        ContentTypeService.Save(contentType);
        var content =
            ContentBuilder.CreateAllTypesContent(contentType, "Random Content", Constants.System.Root);
        ContentService.Save(content);
        var id = content.Id;

        // Act
        var sut = ContentService.GetById(id);

        // Arrange
        Assert.That(sut.GetValue<bool>("isTrue"), Is.True);
        Assert.That(sut.GetValue<int>("number"), Is.EqualTo(42));
        Assert.That(sut.GetValue<string>("bodyText"), Is.EqualTo("Lorem Ipsum Body Text Test"));
        Assert.That(sut.GetValue<string>("singleLineText"), Is.EqualTo("Single Line Text Test"));
        Assert.That(sut.GetValue<string>("multilineText"), Is.EqualTo("Multiple lines \n in one box"));
        Assert.That(sut.GetValue<string>("upload"), Is.EqualTo("/media/1234/koala.jpg"));
        Assert.That(sut.GetValue<string>("label"), Is.EqualTo("Non-editable label"));

        // SD: This is failing because the 'content' call to GetValue<DateTime> always has empty milliseconds
        // MCH: I'm guessing this is an issue because of the format the date is actually stored as, right? Cause we don't do any formatting when saving or loading
        Assert.That(sut.GetValue<DateTime>("dateTime").ToString("G"),
            Is.EqualTo(content.GetValue<DateTime>("dateTime").ToString("G")));
        Assert.That(sut.GetValue<string>("colorPicker"), Is.EqualTo("black"));
        Assert.That(sut.GetValue<string>("ddlMultiple"), Is.EqualTo("1234,1235"));
        Assert.That(sut.GetValue<string>("rbList"), Is.EqualTo("random"));
        Assert.That(sut.GetValue<DateTime>("date").ToString("G"),
            Is.EqualTo(content.GetValue<DateTime>("date").ToString("G")));
        Assert.That(sut.GetValue<string>("ddl"), Is.EqualTo("1234"));
        Assert.That(sut.GetValue<string>("chklist"), Is.EqualTo("randomc"));
        Assert.That(sut.GetValue<Udi>("contentPicker"),
            Is.EqualTo(Udi.Create(Constants.UdiEntityType.Document,
                new Guid("74ECA1D4-934E-436A-A7C7-36CC16D4095C"))));
        Assert.That(sut.GetValue<Udi>("memberPicker"),
            Is.EqualTo(Udi.Create(Constants.UdiEntityType.Member,
                new Guid("9A50A448-59C0-4D42-8F93-4F1D55B0F47D"))));
        Assert.That(sut.GetValue<string>("multiUrlPicker"),
            Is.EqualTo("[{\"name\":\"https://test.com\",\"url\":\"https://test.com\"}]"));
        Assert.That(sut.GetValue<string>("tags"), Is.EqualTo("this,is,tags"));
    }

    [Test]
    [LongRunning]
    public void Can_Delete_Previous_Versions_Not_Latest()
    {
        // Arrange
        var content = ContentService.GetById(Trashed.Id);
        var version = content.VersionId;

        // Act
        ContentService.DeleteVersion(Trashed.Id, version, true);
        var sut = ContentService.GetById(Trashed.Id);

        // Assert
        Assert.That(sut.VersionId, Is.EqualTo(version));
    }

    [Test]
    [LongRunning]
    public void Can_Get_Paged_Children()
    {
        // Start by cleaning the "db"
        var umbTextPage = ContentService.GetById(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
        ContentService.Delete(umbTextPage);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        for (var i = 0; i < 10; i++)
        {
            var c1 = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(c1);
        }

        var entities = ContentService.GetPagedChildren(Constants.System.Root, 0, 6, out var total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(total, Is.EqualTo(10));
        entities = ContentService.GetPagedChildren(Constants.System.Root, 1, 6, out total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(total, Is.EqualTo(10));
    }

    [Test]
    [LongRunning]
    public void Can_Get_Paged_Children_Dont_Get_Descendants()
    {
        // Start by cleaning the "db"
        var umbTextPage = ContentService.GetById(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
        ContentService.Delete(umbTextPage);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        // Only add 9 as we also add a content with children
        for (var i = 0; i < 9; i++)
        {
            var c1 = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(c1);
        }

        var willHaveChildren = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(willHaveChildren);
        for (var i = 0; i < 10; i++)
        {
            var c1 = ContentBuilder.CreateSimpleContent(contentType, "Content" + i, willHaveChildren.Id);
            ContentService.Save(c1);
        }

        // children in root including the folder - not the descendants in the folder
        var entities = ContentService.GetPagedChildren(Constants.System.Root, 0, 6, out var total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(total, Is.EqualTo(10));
        entities = ContentService.GetPagedChildren(Constants.System.Root, 1, 6, out total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(total, Is.EqualTo(10));

        // children in folder
        entities = ContentService.GetPagedChildren(willHaveChildren.Id, 0, 6, out total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(total, Is.EqualTo(10));
        entities = ContentService.GetPagedChildren(willHaveChildren.Id, 1, 6, out total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(total, Is.EqualTo(10));
    }

    [Test]
    public void PublishingTest()
    {
        var contentType = new ContentType(ShortStringHelper, Constants.System.Root) { Alias = "foo", Name = "Foo" };

        var properties = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext)
            {
                Alias = "title", Name = "Title", Mandatory = false, DataTypeId = -88
            }
        };

        contentType.PropertyGroups.Add(new PropertyGroup(properties) { Alias = "content", Name = "content" });

        contentType.SetDefaultTemplate(new Template(ShortStringHelper, "Textpage", "textpage"));
        FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
        ContentTypeService.Save(contentType);

        var content = ContentService.Create("foo", Constants.System.Root, "foo");
        ContentService.Save(content);

        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        content = ContentService.GetById(content.Id);
        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        content.SetValue("title", "foo");
        Assert.IsTrue(content.Edited);

        ContentService.Save(content);

        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        content = ContentService.GetById(content.Id);
        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        var versions = ContentService.GetVersions(content.Id);
        Assert.AreEqual(1, versions.Count());

        // publish content
        // becomes Published, !Edited
        // creates a new version
        // can get published property values
        ContentService.Publish(content, new []{ "*" });

        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);

        content = ContentService.GetById(content.Id);
        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);

        versions = ContentService.GetVersions(content.Id);
        Assert.AreEqual(2, versions.Count());

        Assert.AreEqual("foo", content.GetValue("title", published: true));
        Assert.AreEqual("foo", content.GetValue("title"));

        // unpublish content
        // becomes !Published, Edited
        ContentService.Unpublish(content);

        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        Assert.IsNull(content.GetValue("title", published: true));
        Assert.AreEqual("foo", content.GetValue("title"));

        var vpk = ((Content)content).VersionId;
        var ppk = ((Content)content).PublishedVersionId;

        content = ContentService.GetById(content.Id);
        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        // TODO: depending on 1 line in ContentBaseFactory.BuildEntity
        // the published infos can be gone or not
        // if gone, it's not consistent with above
        Assert.AreEqual(vpk, ((Content)content).VersionId);
        Assert.AreEqual(ppk, ((Content)content).PublishedVersionId); // still there

        // TODO: depending on 1 line in ContentRepository.MapDtoToContent
        // the published values can be null or not
        // if null, it's not consistent with above
        // Assert.IsNull(content.GetValue("title", published:  true));
        Assert.AreEqual("foo", content.GetValue("title", published: true)); // still there
        Assert.AreEqual("foo", content.GetValue("title"));

        versions = ContentService.GetVersions(content.Id);
        Assert.AreEqual(2, versions.Count());

        // ah - we have a problem here - since we're not published we don't have published values
        // and therefore we cannot "just" republish the content - we need to publish some values
        // so... that's not really an option
        //
        // ContentService.Publish(content, new []{ "*" });

        // TODO: what shall we do of all this?
        /*
        // this basically republishes a content
        // what if it never was published?
        // what if it has changes?
        // do we want to "publish" only some variants, or the entire content?
        ContentService.Publish(content);

        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);

        // TODO: should it be 2 or 3
        versions = ContentService.GetVersions(content.Id);
        Assert.AreEqual(2, versions.Count());

        // TODO: now test rollbacks
        var version = ContentService.GetByVersion(content.Id); // test that it gets a version - should be GetVersion
        var previousVersion = ContentService.GetVersions(content.Id).Skip(1).FirstOrDefault(); // need an optimized way to do this
        content.CopyValues(version); // copies the edited value - always
        content.Template = version.Template;
        content.Name = version.Name;
        ContentService.Save(content); // this is effectively a rollback?
        ContentService.Rollback(content); // just kill the method and offer options on values + template + name...
        */
    }

    [Test]
    [LongRunning]
    public async Task Ensure_Invariant_Name()
    {
        var languageService = LanguageService;

        var langUk = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .Build();
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();

        await languageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await languageService.CreateAsync(langUk, Constants.Security.SuperUserKey);

        var contentTypeService = ContentTypeService;

        var contentType = ContentTypeService.Get("umbTextpage");
        contentType.Variations = ContentVariation.Culture;
        contentType.AddPropertyType(new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox,
            ValueStorageType.Nvarchar, "prop")
        { Variations = ContentVariation.Culture });
        ContentTypeService.Save(contentType);

        var content = new Content(null, Constants.System.Root, contentType);

        content.SetCultureName("name-us", langUk.IsoCode);
        content.SetCultureName("name-fr", langFr.IsoCode);
        ContentService.Save(content);

        // the name will be set to the default culture variant name
        Assert.AreEqual("name-us", content.Name);

        // TODO: should we always sync the invariant name even on update? see EnsureInvariantNameValues
        ////updating the default culture variant name should also update the invariant name so they stay in sync
        // content.SetName("name-us-2", langUk.IsoCode);
        // ContentService.Save(content);
        // Assert.AreEqual("name-us-2", content.Name);
    }

    [Test]
    public async Task Ensure_Unique_Culture_Names()
    {
        var languageService = LanguageService;

        var langUk = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .Build();
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();

        await languageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await languageService.CreateAsync(langUk, Constants.Security.SuperUserKey);

        var contentTypeService = ContentTypeService;

        var contentType = ContentTypeService.Get("umbTextpage");
        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        var content = new Content(null, Constants.System.Root, contentType);
        content.SetCultureName("root", langUk.IsoCode);
        ContentService.Save(content);

        for (var i = 0; i < 5; i++)
        {
            var child = new Content(null, content, contentType);
            child.SetCultureName("child", langUk.IsoCode);
            ContentService.Save(child);

            Assert.AreEqual("child" + (i == 0 ? string.Empty : " (" + i + ")"),
                child.GetCultureName(langUk.IsoCode));

            // Save it again to ensure that the unique check is not performed again against it's own name
            ContentService.Save(child);
            Assert.AreEqual("child" + (i == 0 ? string.Empty : " (" + i + ")"),
                child.GetCultureName(langUk.IsoCode));
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_Paged_Children_WithFilterAndOrder()
    {
        var languageService = LanguageService;

        var langUk = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .WithIsMandatory(true)
            .Build();
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        var langDa = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();

        await languageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await languageService.CreateAsync(langUk, Constants.Security.SuperUserKey);
        await languageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var contentTypeService = ContentTypeService;

        var contentType = ContentTypeService.Get("umbTextpage");
        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        int[] o = { 2, 1, 3, 0, 4 }; // randomly different
        for (var i = 0; i < 5; i++)
        {
            var contentA = new Content(null, Constants.System.Root, contentType);
            contentA.SetCultureName("contentA" + i + "uk", langUk.IsoCode);
            contentA.SetCultureName("contentA" + o[i] + "fr", langFr.IsoCode);
            contentA.SetCultureName("contentX" + i + "da", langDa.IsoCode);
            ContentService.Save(contentA);

            var contentB = new Content(null, Constants.System.Root, contentType);
            contentB.SetCultureName("contentB" + i + "uk", langUk.IsoCode);
            contentB.SetCultureName("contentB" + o[i] + "fr", langFr.IsoCode);
            contentB.SetCultureName("contentX" + i + "da", langDa.IsoCode);
            ContentService.Save(contentB);
        }

        // get all
        var list = ContentService.GetPagedChildren(Constants.System.Root, 0, 100, out var total).ToList();

        Console.WriteLine("ALL");
        WriteList(list);

        // 10 items (there's already a Home content in there...)
        Assert.AreEqual(11, total);
        Assert.AreEqual(11, list.Count);

        var sqlContext = GetRequiredService<ISqlContext>();

        // filter
        list = ContentService.GetPagedChildren(
            Constants.System.Root,
            0,
            100,
            out total,
            sqlContext.Query<IContent>().Where(x => x.Name.Contains("contentX")),
            Ordering.By("name", culture: langFr.IsoCode)).ToList();

        Assert.AreEqual(0, total);
        Assert.AreEqual(0, list.Count);

        // filter
        list = ContentService.GetPagedChildren(
            Constants.System.Root,
            0,
            100,
            out total,
            sqlContext.Query<IContent>().Where(x => x.Name.Contains("contentX")),
            Ordering.By("name", culture: langDa.IsoCode)).ToList();

        Console.WriteLine("FILTER BY NAME da:'contentX'");
        WriteList(list);

        Assert.AreEqual(10, total);
        Assert.AreEqual(10, list.Count);

        // filter
        list = ContentService.GetPagedChildren(
            Constants.System.Root,
            0,
            100,
            out total,
            sqlContext.Query<IContent>().Where(x => x.Name.Contains("contentA")),
            Ordering.By("name", culture: langFr.IsoCode)).ToList();

        Console.WriteLine("FILTER BY NAME fr:'contentA', ORDER ASC");
        WriteList(list);

        Assert.AreEqual(5, total);
        Assert.AreEqual(5, list.Count);

        for (var i = 0; i < 5; i++)
        {
            Assert.AreEqual("contentA" + i + "fr", list[i].GetCultureName(langFr.IsoCode));
        }

        list = ContentService.GetPagedChildren(
            Constants.System.Root,
            0,
            100,
            out total,
            sqlContext.Query<IContent>().Where(x => x.Name.Contains("contentA")),
            Ordering.By("name", Direction.Descending, langFr.IsoCode)).ToList();

        Console.WriteLine("FILTER BY NAME fr:'contentA', ORDER DESC");
        WriteList(list);

        Assert.AreEqual(5, total);
        Assert.AreEqual(5, list.Count);

        for (var i = 0; i < 5; i++)
        {
            Assert.AreEqual("contentA" + (4 - i) + "fr", list[i].GetCultureName(langFr.IsoCode));
        }
    }

    private void WriteList(List<IContent> list)
    {
        foreach (var content in list)
        {
            Console.WriteLine("[{0}] {1} {2} {3} {4}", content.Id, content.Name, content.GetCultureName("en-GB"),
                content.GetCultureName("fr-FR"), content.GetCultureName("da-DK"));
        }

        Console.WriteLine("-");
    }

    [Test]
    [LongRunning]
    public async Task Can_SaveRead_Variations()
    {
        var languageService = LanguageService;
        var langPt = new LanguageBuilder()
            .WithCultureInfo("pt-PT")
            .WithIsDefault(true)
            .Build();
        var langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        var langUk = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .Build();
        var langDe = new LanguageBuilder()
            .WithCultureInfo("de-DE")
            .Build();

        await languageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await languageService.CreateAsync(langUk, Constants.Security.SuperUserKey);
        await languageService.CreateAsync(langDe, Constants.Security.SuperUserKey);

        var contentTypeService = ContentTypeService;

        var contentType = ContentTypeService.Get("umbTextpage");
        contentType.Variations = ContentVariation.Culture;
        contentType.AddPropertyType(new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox,
            ValueStorageType.Nvarchar, "prop")
        { Variations = ContentVariation.Culture });

        // TODO: add test w/ an invariant prop
        ContentTypeService.Save(contentType);

        var content = ContentService.Create("Home US", Constants.System.Root, "umbTextpage");

        // creating content with a name but no culture - will set the invariant name
        // but, because that content is variant, as soon as we save, we'll need to
        // replace the invariant name with whatever we have in cultures - always
        //
        // in fact, that would throw, because there is no name
        // ContentService.Save(content);

        // Act
        content.SetValue("author", "Barack Obama");
        content.SetValue("prop", "value-fr1", langFr.IsoCode);
        content.SetValue("prop", "value-uk1", langUk.IsoCode);
        content.SetCultureName("name-fr", langFr.IsoCode); // and then we can save
        content.SetCultureName("name-uk", langUk.IsoCode);
        ContentService.Save(content);

        // content has been saved,
        // it has names, but no publishNames, and no published cultures
        var content2 = ContentService.GetById(content.Id);

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
        AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));

        // nothing has been published yet
        AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, false),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, false),
            (langDe, false));

        // not published => must be edited, if available
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

        // Act
        ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });

        // both FR and UK have been published,
        // and content has been published,
        // it has names, publishNames, and published cultures
        content2 = ContentService.GetById(content.Id);

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
        AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));

        // fr and uk have been published now
        AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, true), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, true), (langUk, true),
            (langDe, false));

        // fr and uk, published without changes, not edited
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, false), (langUk, false),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, false), (langUk, false),
            (langDe, false));

        AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langFr, false),
            (langUk, false)); // DE would throw
        AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langFr, false),
            (langUk, false)); // DE would throw

        // note that content and content2 culture published dates might be slightly different due to roundtrip to database

        // Act
        ContentService.Publish(content, new []{ "*" });

        // now it has publish name for invariant neutral
        content2 = ContentService.GetById(content.Id);

        Assert.AreEqual("name-fr", content2.PublishName);

        content.SetCultureName("Home US2", null);
        content.SetCultureName("name-fr2", langFr.IsoCode);
        content.SetCultureName("name-uk2", langUk.IsoCode);
        content.SetValue("author", "Barack Obama2");
        content.SetValue("prop", "value-fr2", langFr.IsoCode);
        content.SetValue("prop", "value-uk2", langUk.IsoCode);
        ContentService.Save(content);

        // content has been saved,
        // it has updated names, unchanged publishNames, and published cultures
        content2 = ContentService.GetById(content.Id);

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
        AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));

        // no change
        AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, true), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, true), (langUk, true),
            (langDe, false));

        // we have changed values so now fr and uk are edited
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

        AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langFr, false),
            (langUk, false)); // DE would throw
        AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langFr, false),
            (langUk, false)); // DE would throw

        // Act
        // cannot just 'save' since we are changing what's published!
        ContentService.Unpublish(content, langFr.IsoCode);

        // content has been published,
        // the french culture is gone
        // (only if french is not mandatory, else everything would be gone!)
        content2 = ContentService.GetById(content.Id);

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
        AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));

        // fr is not published anymore
        AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true),
            (langDe, false));

        // and so, fr has to be edited
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

        AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langUk, false)); // FR, DE would throw
        AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langUk, false)); // FR, DE would throw

        // Act
        ContentService.Unpublish(content);

        // content has been unpublished,
        // but properties, names, etc. retain their 'published' values so the content
        // can be re-published in its exact original state (before being unpublished)
        //
        // BEWARE!
        // in order for a content to be unpublished as a whole, and then republished in
        // its exact previous state, properties and names etc. retain their published
        // values even though the content is not published - hence many things being
        // non-null or true below - always check against content.Published to be sure
        content2 = ContentService.GetById(content.Id);

        Assert.IsFalse(content2.Published);

        Assert.AreEqual("name-fr2", content2.Name); // got the default culture name when saved
        Assert.AreEqual("name-fr2", content2.GetCultureName(langFr.IsoCode));
        Assert.AreEqual("name-uk2", content2.GetCultureName(langUk.IsoCode));

        Assert.IsNull(content2.PublishName);
        Assert.IsNull(content2.GetPublishName(langFr.IsoCode));
        Assert.IsNull(content2.GetPublishName(langUk.IsoCode));

        Assert.AreEqual("value-fr2", content2.GetValue("prop", langFr.IsoCode));
        Assert.AreEqual("value-uk2", content2.GetValue("prop", langUk.IsoCode));
        Assert.IsNull(content2.GetValue("prop", langFr.IsoCode, published: true));
        Assert.AreEqual("value-uk1",
            content2.GetValue("prop", langUk.IsoCode, published: true)); // has value, see note above

        // no change
        AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));

        // Everything should be unpublished
        AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, false), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, false), (langDe, false));

        // and so, fr has to be edited - uk still is
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

        AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw
        AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue, (langUk, false)); // FR, DE would throw

        // Act
        ContentService.Publish(content, new[] { langUk.IsoCode });

        content2 = ContentService.GetById(content.Id);

        // no change
        AssertPerCulture(content, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureAvailable(c), (langFr, true), (langUk, true),
            (langDe, false));

        // no change
        AssertPerCulture(content, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true),
            (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCulturePublished(c), (langFr, false), (langUk, true),
            (langDe, false));

        // now, uk is no more edited
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, false), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, false),
            (langDe, false));

        AssertPerCulture(content, (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langUk, false)); // FR, DE would throw
        AssertPerCulture(content2, (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langUk, false)); // FR, DE would throw

        // Act
        content.SetCultureName("name-uk3", langUk.IsoCode);
        ContentService.Save(content);

        content2 = ContentService.GetById(content.Id);

        // note that the 'edited' flags only change once saved - not immediately
        // but they change, on what's being saved, and when getting it back

        // changing the name = edited!
        Assert.IsTrue(content.IsCultureEdited(langUk.IsoCode));
        Assert.IsTrue(content2.IsCultureEdited(langUk.IsoCode));
    }

    [Test]
    public void Cannot_Publish_Newly_Created_Unsaved_Content()
    {
        var content = ContentService.Create("Test", Constants.System.Root, "umbTextpage");
        var publishResult = ContentService.Publish(content, new[] { "*" });
        Assert.AreEqual(PublishResultType.FailedPublishUnsavedChanges, publishResult.Result);
    }

    [Test]
    public void Cannot_Publish_Unsaved_Content()
    {
        var content = ContentService.Create("Test", Constants.System.Root, "umbTextpage");
        ContentService.Save(content);
        content.Name = "Test2";

        var publishResult = ContentService.Publish(content, new[] { "*" });
        Assert.AreEqual(PublishResultType.FailedPublishUnsavedChanges, publishResult.Result);
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Variant_Content()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(content);

        // reset any state and attempt a publish
        content = ContentService.GetById(content.Key)!;
        var result = ContentService.Publish(content, new[] { "*" });

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PublishResultType.FailedPublishContentInvalid, result.Result);

        // verify saved state
        content = ContentService.GetById(content.Key)!;
        Assert.IsEmpty(content.PublishedCultures);
    }

    [Test]
    public async Task Can_Publish_Culture_With_Other_Culture_Invalid()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(content);

        // reset any state and attempt a publish
        content = ContentService.GetById(content.Key)!;
        var result = ContentService.Publish(content, new[] { langEn.IsoCode });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(PublishResultType.SuccessPublishCulture, result.Result);

        // verify saved state
        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
        Assert.AreEqual(langEn.IsoCode, content.PublishedCultures.First());
    }

    private void AssertPerCulture<T>(IContent item, Func<IContent, string, T> getter,
        params (ILanguage Language, bool Result)[] testCases)
    {
        foreach (var testCase in testCases)
        {
            var value = getter(item, testCase.Language.IsoCode);
            Assert.AreEqual(testCase.Result, value,
                $"Expected {testCase.Result} and got {value} for culture {testCase.Language.IsoCode}.");
        }
    }

    private IEnumerable<IContent> CreateContentHierarchy()
    {
        var contentType = ContentTypeService.Get("umbTextpage");
        var root = ContentService.GetById(Textpage.Id);

        var list = new List<IContent>();

        for (var i = 0; i < 10; i++)
        {
            var content =
                ContentBuilder.CreateSimpleContent(contentType, "Hierarchy Simple Text Page " + i, root);

            list.Add(content);
            list.AddRange(CreateChildrenOf(contentType, content, 4));

            Debug.Print("Created: 'Hierarchy Simple Text Page {0}'", i);
        }

        return list;
    }

    private IEnumerable<IContent> CreateChildrenOf(IContentType contentType, IContent content, int depth)
    {
        var list = new List<IContent>();
        for (var i = 0; i < depth; i++)
        {
            var c = ContentBuilder.CreateSimpleContent(contentType, "Hierarchy Simple Text Subpage " + i,
                content);
            list.Add(c);

            Debug.Print("Created: 'Hierarchy Simple Text Subpage {0}' - Depth: {1}", i, depth);
        }

        return list;
    }

    private void CreateEnglishAndFrenchDocumentType(out Language langUk, out Language langFr,
        out ContentType contentType)
    {
        langUk = (Language)new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .Build();
        langFr = (Language)new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
        LanguageService.CreateAsync(langUk, Constants.Security.SuperUserKey).GetAwaiter().GetResult();

        contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);
    }

    private IContent CreateEnglishAndFrenchDocument(out Language langUk, out Language langFr,
        out ContentType contentType)
    {
        CreateEnglishAndFrenchDocumentType(out langUk, out langFr, out contentType);

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-fr", langFr.IsoCode);
        content.SetCultureName("content-en", langUk.IsoCode);

        return content;
    }

    public class ContentNotificationHandler :
        INotificationHandler<ContentCopyingNotification>,
        INotificationHandler<ContentCopiedNotification>,
        INotificationHandler<ContentPublishingNotification>,
        INotificationHandler<ContentSavingNotification>
    {
        public static Action<ContentPublishingNotification> PublishingContent { get; set; }

        public static Action<ContentCopyingNotification> CopyingContent { get; set; }

        public static Action<ContentCopiedNotification> CopiedContent { get; set; }

        public static Action<ContentSavingNotification> SavingContent { get; set; }

        public void Handle(ContentCopiedNotification notification) => CopiedContent?.Invoke(notification);

        public void Handle(ContentCopyingNotification notification) => CopyingContent?.Invoke(notification);
        public void Handle(ContentPublishingNotification notification) => PublishingContent?.Invoke(notification);

        public void Handle(ContentSavingNotification notification) => SavingContent?.Invoke(notification);
    }

    private async Task<(ILanguage LangEn, ILanguage LangDa, IContentType contentType)> SetupVariantTest()
    {
        var langEn = (await LanguageService.GetAsync("en-US"))!;
        var langDa = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = new ContentTypeBuilder()
            .WithAlias("variantContent")
            .WithName("Variant Content")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithVariations(ContentVariation.Culture)
            .WithMandatory(true)
            .Done()
            .Done()
            .Build();

        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        return (langEn, langDa, contentType);
    }
}
