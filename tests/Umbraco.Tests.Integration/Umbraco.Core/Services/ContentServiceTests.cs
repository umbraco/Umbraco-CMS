// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
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
internal sealed partial class ContentServiceTests : UmbracoIntegrationTestWithContent
{
    [SetUp]
    public new void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;
    // TODO: Add test to verify there is only ONE newest document/content in {Constants.DatabaseSchema.Tables.Document} table after updating.
    // TODO: Add test to delete specific version (with and without deleting prior versions) and versions by date.

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

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder
            .AddNotificationHandler<ContentPublishingNotification, ContentNotificationHandler>()
            .AddNotificationHandler<ContentCopyingNotification, ContentNotificationHandler>()
            .AddNotificationHandler<ContentCopiedNotification, ContentNotificationHandler>()
            .AddNotificationHandler<ContentSavingNotification, ContentNotificationHandler>();

        builder.Services.AddUnique<IIdKeyMap>(services => new SpyIdKeyMap(ActivatorUtilities.CreateInstance<IdKeyMap>(services)));
    }

    /// <summary>
    ///     Wraps a real <see cref="IIdKeyMap" /> so tests can assert whether it was actually consulted - a read
    ///     path that's supposed to resolve the parent via an already-populated <see cref="IContentBase.ParentKey" />
    ///     could regress to calling <see cref="IIdKeyMap" /> unconditionally (e.g. via this service's own
    ///     constructor-injected fallback for callers that pass an entity whose <see cref="IContentBase.ParentKey" />
    ///     isn't populated) while still returning the correct value, so a test that only asserts the final value
    ///     wouldn't catch that regression.
    /// </summary>
    private sealed class SpyIdKeyMap : IIdKeyMap
    {
        private readonly IIdKeyMap _inner;

        public SpyIdKeyMap(IIdKeyMap inner) => _inner = inner;

        public int GetKeyForIdAsyncCallCount { get; private set; }

        public Task<Attempt<int>> GetIdForKeyAsync(Guid key, UmbracoObjectTypes umbracoObjectType) => _inner.GetIdForKeyAsync(key, umbracoObjectType);

        public Task<Attempt<int>> GetIdForUdiAsync(Udi udi) => _inner.GetIdForUdiAsync(udi);

        public Task<Attempt<Udi?>> GetUdiForIdAsync(int id, UmbracoObjectTypes umbracoObjectType) => _inner.GetUdiForIdAsync(id, umbracoObjectType);

        public Task<Attempt<Guid>> GetKeyForIdAsync(int id, UmbracoObjectTypes umbracoObjectType)
        {
            GetKeyForIdAsyncCallCount++;
            return _inner.GetKeyForIdAsync(id, umbracoObjectType);
        }

        public void ClearCache() => _inner.ClearCache();

        public void ClearCache(int id) => _inner.ClearCache(id);

        public void ClearCache(Guid key) => _inner.ClearCache(key);
    }

    [Test]
    public async Task GetByIdAsync_ExistingContent_ReturnsContent()
    {
        IContent? content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        Assert.That(content, Is.Not.Null);
        Assert.That(content!.Id, Is.EqualTo(Textpage.Id));
    }

    [Test]
    public async Task GetByIdAsync_UnknownKey_ReturnsNull()
    {
        IContent? content = await ContentService.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(content, Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Sort_Preserves_Template_And_Property_Data_When_Items_Loaded_Without_Them(bool useSortChildren)
    {
        // The fixture's children share the content type's default template; assign it so we can verify it survives.
        var templateId = ContentType.DefaultTemplateId;
        Assert.That(templateId, Is.GreaterThan(0), "Test setup expects a default template on the content type.");

        var childKeys = new[] { SubPageKey, SubPage2Key, SubPage3Key };
        foreach (var key in childKeys)
        {
            IContent child = ContentService.GetByIdAsync(new Guid(key), CancellationToken.None).GetAwaiter().GetResult();
            child.TemplateId = templateId;
            await ContentService.SaveAsync(child, null, null, CancellationToken.None);
        }

        // Load the children the way a collection view does: without templates or property data (#23120).
        List<IContent> partialChildren = (await ContentService.GetChildrenWithoutTemplatesAsync(Textpage.Key, 0, 100, propertyAliases: [], ordering: null, CancellationToken.None))
            .Items
            .ToList();
        Assert.That(partialChildren, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            // Precondition for the bug: the loaded instances really are partial.
            Assert.That(partialChildren.All(x => x.TemplateId is null), Is.True, "Expected templates not to be loaded.");
            Assert.That(partialChildren.All(x => x.Properties.Count == 0), Is.True, "Expected properties not to be loaded.");
        });

        // Rotate the order so every item's sort order changes (and is therefore re-saved).
        Dictionary<Guid, IContent> byKey = partialChildren.ToDictionary(x => x.Key, x => x);
        if (useSortChildren)
        {
            Guid[] rotated =
            [
                byKey[new Guid(SubPage2Key)].Key,
                byKey[new Guid(SubPage3Key)].Key,
                byKey[new Guid(SubPageKey)].Key,
            ];

            var result = await ContentService.SortChildrenAsync(Textpage.Key, rotated, Constants.Security.SuperUserKey, CancellationToken.None);
            Assert.That(result.Success, Is.True);
        }
        else
        {
            IContent[] rotated =
            [
                byKey[new Guid(SubPage2Key)],
                byKey[new Guid(SubPage3Key)],
                byKey[new Guid(SubPageKey)],
            ];

            OperationResult result = ContentService.Sort(rotated);
            Assert.That(result.Success, Is.True);
        }

        // Every sorted child must retain its template and property data.
        foreach (var key in childKeys)
        {
            IContent reloaded = ContentService.GetByIdAsync(new Guid(key), CancellationToken.None).GetAwaiter().GetResult();
            Assert.Multiple(() =>
            {
                Assert.That(reloaded.TemplateId, Is.EqualTo(templateId), $"Template lost for {key}.");
                Assert.That(reloaded.GetValue<string>("title"), Is.Not.Null.And.Not.Empty, $"Property data lost for {key}.");
            });
        }

        // And the requested order must have been applied.
        Assert.Multiple(() =>
        {
            Assert.That(ContentService.GetByIdAsync(new Guid(SubPage2Key), CancellationToken.None).GetAwaiter().GetResult().SortOrder, Is.EqualTo(0));
            Assert.That(ContentService.GetByIdAsync(new Guid(SubPage3Key), CancellationToken.None).GetAwaiter().GetResult().SortOrder, Is.EqualTo(1));
            Assert.That(ContentService.GetByIdAsync(new Guid(SubPageKey), CancellationToken.None).GetAwaiter().GetResult().SortOrder, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Create_Blueprint()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var blueprint = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
        blueprint.SetValue("title", "blueprint 1");
        blueprint.SetValue("bodyText", "blueprint 2");
        blueprint.SetValue("keywords", "blueprint 3");
        blueprint.SetValue("description", "blueprint 4");

        await ContentService.SaveBlueprintAsync(blueprint, null, Constants.Security.SuperUserKey, CancellationToken.None);

        var found = (await ContentService.GetBlueprintsForContentTypesAsync(CancellationToken.None)).ToArray();
        Assert.AreEqual(1, found.Length);

        // ensures it's not found by normal content
        var contentFound = await ContentService.GetByIdAsync(found[0].Key, CancellationToken.None);
        Assert.IsNull(contentFound);
    }

    [Test]
    public async Task Delete_Blueprint()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var blueprint = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
        blueprint.SetValue("title", "blueprint 1");
        blueprint.SetValue("bodyText", "blueprint 2");
        blueprint.SetValue("keywords", "blueprint 3");
        blueprint.SetValue("description", "blueprint 4");

        await ContentService.SaveBlueprintAsync(blueprint, null, Constants.Security.SuperUserKey, CancellationToken.None);

        await ContentService.DeleteBlueprintAsync(blueprint, Constants.Security.SuperUserKey, CancellationToken.None);

        var found = (await ContentService.GetBlueprintsForContentTypesAsync(CancellationToken.None)).ToArray();
        Assert.AreEqual(0, found.Length);
    }

    [Test]
    public async Task Move_Blueprint()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var container = ContentBuilder.CreateTextpageContent(contentType, "container", Constants.System.Root);
        await ContentService.SaveAsync(container, null, null, CancellationToken.None);

        var blueprint = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
        await ContentService.SaveBlueprintAsync(blueprint, null, Constants.Security.SuperUserKey, CancellationToken.None);

        blueprint.ParentId = container.Id;
        await ContentService.MoveBlueprintAsync(blueprint, Constants.Security.SuperUserKey, CancellationToken.None);

        IContent? moved = await ContentService.GetBlueprintByIdAsync(blueprint.Key, CancellationToken.None);
        Assert.IsNotNull(moved);
        Assert.AreEqual(container.Id, moved!.ParentId);
    }

    [Test]
    public async Task GetBlueprintByIdAsync_Returns_Blueprint_With_BlueprintFlagSet()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var blueprint = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
        await ContentService.SaveBlueprintAsync(blueprint, null, Constants.Security.SuperUserKey, CancellationToken.None);

        var saved = (await ContentService.GetBlueprintsForContentTypesAsync(CancellationToken.None)).Single();

        var found = await ContentService.GetBlueprintByIdAsync(saved.Key, CancellationToken.None);

        Assert.That(found, Is.Not.Null);
        Assert.That(found.Key, Is.EqualTo(saved.Key));
        Assert.That(found.Blueprint, Is.True);
    }

    [Test]
    public async Task GetBlueprintByIdAsync_Returns_Null_For_Unknown_Key()
    {
        var found = await ContentService.GetBlueprintByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task GetBlueprintByIdAsync_Returns_Null_For_Regular_Document_Key()
    {
        var found = await ContentService.GetBlueprintByIdAsync(Textpage.Key, CancellationToken.None);

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task Create_Blueprint_From_Content()
    {
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

            var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

            var originalPage = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
            originalPage.SetValue("title", "blueprint 1");
            originalPage.SetValue("bodyText", "blueprint 2");
            originalPage.SetValue("keywords", "blueprint 3");
            originalPage.SetValue("description", "blueprint 4");
            await ContentService.SaveAsync(originalPage, null, null, CancellationToken.None);

            var fromContent = await ContentService.CreateBlueprintFromContentAsync(originalPage, "hello world", Constants.Security.SuperUserKey, CancellationToken.None);
            await ContentService.SaveBlueprintAsync(fromContent, originalPage, Constants.Security.SuperUserKey, CancellationToken.None);

            Assert.IsTrue(fromContent.HasIdentity);
            Assert.AreEqual("blueprint 1", fromContent.Properties["title"]?.GetValue());
            Assert.AreEqual("blueprint 2", fromContent.Properties["bodyText"]?.GetValue());
            Assert.AreEqual("blueprint 3", fromContent.Properties["keywords"]?.GetValue());
            Assert.AreEqual("blueprint 4", fromContent.Properties["description"]?.GetValue());
        }
    }

    [Test]
    [LongRunning]
    public async Task Get_All_Blueprints()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var ct1 = ContentTypeBuilder.CreateTextPageContentType("ct1", defaultTemplateId: template.Id);
        await TemplateService.CreateAsync(ct1.DefaultTemplate, Constants.Security.SuperUserKey);
        await ContentTypeService.CreateAsync(ct1, Constants.Security.SuperUserKey);
        var ct2 = ContentTypeBuilder.CreateTextPageContentType("ct2", defaultTemplateId: template.Id);
        await TemplateService.CreateAsync(ct2.DefaultTemplate, Constants.Security.SuperUserKey);
        await ContentTypeService.CreateAsync(ct2, Constants.Security.SuperUserKey);

        for (var i = 0; i < 10; i++)
        {
            var blueprint =
                ContentBuilder.CreateTextpageContent(i % 2 == 0 ? ct1 : ct2, "hello" + i, Constants.System.Root);
            await ContentService.SaveBlueprintAsync(blueprint, null, Constants.Security.SuperUserKey, CancellationToken.None);
        }

        var found = (await ContentService.GetBlueprintsForContentTypesAsync(CancellationToken.None)).ToArray();
        Assert.AreEqual(10, found.Length);

        found = (await ContentService.GetBlueprintsForContentTypesAsync(CancellationToken.None, ct1.Key)).ToArray();
        Assert.AreEqual(5, found.Length);

        found = (await ContentService.GetBlueprintsForContentTypesAsync(CancellationToken.None, ct2.Key)).ToArray();
        Assert.AreEqual(5, found.Length);
    }

    [Test]
    [LongRunning]
    public async Task Delete_Blueprints_Of_Types()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var ct1 = ContentTypeBuilder.CreateTextPageContentType("ct1", defaultTemplateId: template.Id);
        await TemplateService.CreateAsync(ct1.DefaultTemplate, Constants.Security.SuperUserKey);
        await ContentTypeService.CreateAsync(ct1, Constants.Security.SuperUserKey);
        var ct2 = ContentTypeBuilder.CreateTextPageContentType("ct2", defaultTemplateId: template.Id);
        await TemplateService.CreateAsync(ct2.DefaultTemplate, Constants.Security.SuperUserKey);
        await ContentTypeService.CreateAsync(ct2, Constants.Security.SuperUserKey);

        for (var i = 0; i < 10; i++)
        {
            var blueprint =
                ContentBuilder.CreateTextpageContent(i % 2 == 0 ? ct1 : ct2, "hello" + i, Constants.System.Root);
            await ContentService.SaveBlueprintAsync(blueprint, null, Constants.Security.SuperUserKey, CancellationToken.None);
        }

        await ContentService.DeleteBlueprintsOfTypeAsync(ct1.Key, Constants.Security.SuperUserKey, CancellationToken.None);

        var found = (await ContentService.GetBlueprintsForContentTypesAsync(CancellationToken.None)).ToArray();
        Assert.AreEqual(5, found.Length);
        Assert.IsTrue(found.All(x => x.ContentTypeId == ct2.Id));

        await ContentService.DeleteBlueprintsOfTypesAsync(Array.Empty<Guid>(), Constants.Security.SuperUserKey, CancellationToken.None);

        found = (await ContentService.GetBlueprintsForContentTypesAsync(CancellationToken.None)).ToArray();
        Assert.AreEqual(0, found.Length);
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
        await ContentTypeService.CreateAsync(ctInvariant, Constants.Security.SuperUserKey);

        var ctVariant = ContentTypeBuilder.CreateBasicContentType("variantPage");
        ctVariant.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(ctVariant, Constants.Security.SuperUserKey);

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
                var r = await ContentService.SaveAsync(c, null, contentSchedule, CancellationToken.None);
                Assert.IsTrue(r.Success, r.Result.ToString());
            }
            else
            {
                await ContentService.SaveAsync(c, null, null, CancellationToken.None);
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
                    ContentScheduleCollection.CreateWithEntry(
                        alternatingCulture,
                        now.AddSeconds(5),
                        null); // release in 5 seconds
                var r = await ContentService.SaveAsync(c, null, contentSchedule, CancellationToken.None);
                Assert.IsTrue(r.Success, r.Result.ToString());

                alternatingCulture = alternatingCulture == langFr.IsoCode ? langUk.IsoCode : langFr.IsoCode;
            }
            else
            {
                await ContentService.SaveAsync(c, null, null, CancellationToken.None);
                var r = ContentService.Publish(c, c.AvailableCultures.ToArray());

                var contentSchedule =
                    ContentScheduleCollection.CreateWithEntry(
                        alternatingCulture,
                        null,
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
    public async Task Remove_Scheduled_Publishing_Date()
    {
        // Arrange

        // Act
        var content = await ContentService.CreateAndSaveAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        var contentSchedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddHours(2));
        await ContentService.SaveAsync(content, Constants.Security.SuperUserId, contentSchedule, CancellationToken.None);
        Assert.AreEqual(1, contentSchedule.FullSchedule.Count);

        contentSchedule = await ContentService.GetContentScheduleByContentIdAsync(content.Key, CancellationToken.None);
        var sched = contentSchedule.FullSchedule;
        Assert.AreEqual(1, sched.Count);
        Assert.AreEqual(1, sched.Count(x => x.Culture == Constants.System.InvariantCulture));
        contentSchedule.Clear(ContentScheduleAction.Expire);
        await ContentService.SaveAsync(content, Constants.Security.SuperUserId, contentSchedule, CancellationToken.None);

        // Assert
        contentSchedule = await ContentService.GetContentScheduleByContentIdAsync(content.Key, CancellationToken.None);
        sched = contentSchedule.FullSchedule;
        Assert.AreEqual(0, sched.Count);
        Assert.IsTrue(ContentService.Publish(content, content.AvailableCultures.ToArray()).Success);
    }

    [Test]
    public async Task SaveAsync_WithContentSchedule_PersistsThenRemovesSchedule()
    {
        // Arrange
        var content = await ContentService.CreateAndSaveAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        // Act
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddHours(2));
        await ContentService.SaveAsync(content, Constants.Security.SuperUserId, contentSchedule, CancellationToken.None);
        Assert.AreEqual(1, contentSchedule.FullSchedule.Count);

        contentSchedule = await ContentService.GetContentScheduleByContentIdAsync(content.Key, CancellationToken.None);
        var sched = contentSchedule.FullSchedule;
        Assert.AreEqual(1, sched.Count);
        Assert.AreEqual(1, sched.Count(x => x.Culture == Constants.System.InvariantCulture));
        contentSchedule.Clear(ContentScheduleAction.Expire);
        await ContentService.SaveAsync(content, Constants.Security.SuperUserId, contentSchedule, CancellationToken.None);

        // Assert
        contentSchedule = await ContentService.GetContentScheduleByContentIdAsync(content.Key, CancellationToken.None);
        sched = contentSchedule.FullSchedule;
        Assert.AreEqual(0, sched.Count);
        Assert.IsTrue(ContentService.Publish(content, content.AvailableCultures.ToArray()).Success);
    }

    [Test]
    [LongRunning]
    public async Task Get_Top_Version_Ids()
    {
        // Arrange
        // Act
        var content = await ContentService.CreateAndSaveAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        for (var i = 0; i < 20; i++)
        {
            content.SetValue("bodyText", "hello world " + Guid.NewGuid());
            await ContentService.SaveAsync(content, null, null, CancellationToken.None);
            ContentService.Publish(content, content.AvailableCultures.ToArray());
        }

        // Assert
        var allVersions = await ContentService.GetVersionIdsAsync(content.Key, 0, int.MaxValue, CancellationToken.None);
        Assert.AreEqual(21, allVersions.Count());

        var topVersions = await ContentService.GetVersionIdsAsync(content.Key, 0, 4, CancellationToken.None);
        Assert.AreEqual(4, topVersions.Count());
    }

    [Test]
    [LongRunning]
    public async Task Get_By_Ids_Sorted()
    {
        // Arrange
        // Act
        var results = new List<IContent>();
        for (var i = 0; i < 20; i++)
        {
            results.Add(await ContentService.CreateAndSaveAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None));
        }

        var sortedGet = (await ContentService.GetByIdsAsync(new[] { results[10].Key, results[5].Key, results[12].Key }, CancellationToken.None))
            .ToArray();

        // Assert
        Assert.AreEqual(sortedGet[0].Id, results[10].Id);
        Assert.AreEqual(sortedGet[1].Id, results[5].Id);
        Assert.AreEqual(sortedGet[2].Id, results[12].Id);
    }

    [Test]
    public async Task Count_All()
    {
        // Arrange
        // Act
        for (var i = 0; i < 20; i++)
        {
            await ContentService.CreateAndSaveAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        }

        // Assert
        Assert.AreEqual(25, await ContentService.CountAsync(null, CancellationToken.None));
    }

    [Test]
    public async Task Count_By_Content_Type()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbBlah", "test Doc Type", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Act
        for (var i = 0; i < 20; i++)
        {
            await ContentService.CreateAndSaveAsync("Test", (Guid?)null, "umbBlah", Constants.Security.SuperUserKey, CancellationToken.None);
        }

        // Assert
        Assert.AreEqual(20, await ContentService.CountAsync("umbBlah", CancellationToken.None));
    }

    [Test]
    public async Task Count_Children()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbBlah", "test Doc Type", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        var parent = await ContentService.CreateAndSaveAsync("Test", (Guid?)null, "umbBlah", Constants.Security.SuperUserKey, CancellationToken.None);

        // Act
        for (var i = 0; i < 20; i++)
        {
            await ContentService.CreateAndSaveAsync("Test", parent, "umbBlah", Constants.Security.SuperUserKey, CancellationToken.None);
        }

        // Assert
        Assert.AreEqual(20, await ContentService.CountChildrenAsync(parent.Key, null, CancellationToken.None));
    }

    [Test]
    public async Task Count_Descendants()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbBlah", "test Doc Type", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        var parent = await ContentService.CreateAndSaveAsync("Test", (Guid?)null, "umbBlah", Constants.Security.SuperUserKey, CancellationToken.None);

        // Act
        var current = parent;
        for (var i = 0; i < 20; i++)
        {
            current = await ContentService.CreateAndSaveAsync("Test", current, "umbBlah", Constants.Security.SuperUserKey, CancellationToken.None);
        }

        // Assert
        Assert.AreEqual(20, await ContentService.CountDescendantsAsync(parent.Key, null, CancellationToken.None));
    }

    [Test]
    public async Task CountPublishedAsync_ReturnsOnlyPublishedNonTrashedCount()
    {
        // Arrange - nothing is published by default in UmbracoIntegrationTestWithContent's fixture
        Assert.AreEqual(0, await ContentService.CountPublishedAsync(null, CancellationToken.None));

        // Act
        ContentService.Publish(Textpage, ["*"]);
        ContentService.Publish(Subpage, ["*"]);

        // Assert
        Assert.AreEqual(2, await ContentService.CountPublishedAsync(null, CancellationToken.None));
        Assert.AreEqual(2, await ContentService.CountPublishedAsync("umbTextpage", CancellationToken.None));
        Assert.AreEqual(0, await ContentService.CountPublishedAsync("someOtherAliasThatDoesNotExist", CancellationToken.None));
    }

    [Test]
    public async Task GetAncestorsAsync_Returns_Empty_List_When_Path_Is_Null()
    {
        // Arrange
        // Act
        var current = new Mock<IContent>();
        PagedModel<IContent> res = await ContentService.GetAncestorsAsync(current.Object, 0, int.MaxValue, CancellationToken.None);

        // Assert
        Assert.IsEmpty(res.Items);
    }

    [Test]
    public async Task GetAncestorsAsync_Guid_Returns_Ancestors_Of_Content()
    {
        PagedModel<IContent> res = await ContentService.GetAncestorsAsync(Subpage.Key, 0, int.MaxValue, CancellationToken.None);

        Assert.That(res.Items.ToList(), Has.Count.EqualTo(1));
        Assert.That(res.Items.ToList()[0].Key, Is.EqualTo(Textpage.Key));
    }

    [Test]
    public async Task GetParentAsync_Returns_Parent_Of_Content()
    {
        IContent? result = await ContentService.GetParentAsync(Subpage.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Key, Is.EqualTo(Textpage.Key));
    }

    [Test]
    public async Task GetParentAsync_Returns_Null_For_Root_Content()
    {
        IContent? result = await ContentService.GetParentAsync(Textpage.Key, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetParentAsync_Returns_Null_For_Unknown_Key()
    {
        IContent? result = await ContentService.GetParentAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetParentAsync_Uses_Content_ParentKey_Without_Calling_IIdKeyMap()
    {
        var idKeyMapSpy = (SpyIdKeyMap)IdKeyMap;

        IContent? result = await ContentService.GetParentAsync(Subpage.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(Textpage.Key));
        Assert.That(idKeyMapSpy.GetKeyForIdAsyncCallCount, Is.Zero, "GetParentAsync must resolve the parent via the already-populated ParentKey, not via IIdKeyMap");
    }

    [Test]
    public async Task GetParentAsync_ByEntity_Uses_Content_ParentKey_Without_Calling_IIdKeyMap()
    {
        var idKeyMapSpy = (SpyIdKeyMap)IdKeyMap;

        // Fetch through the repository (rather than using the raw fixture instance) so ParentKey is
        // already populated - otherwise GetParentAsync's own catch-block fallback would call IIdKeyMap
        // regardless of whether the already-populated-ParentKey path itself works, masking the assertion.
        IContent? subpage = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        Assert.That(subpage, Is.Not.Null);
        var callCountBeforeGetParent = idKeyMapSpy.GetKeyForIdAsyncCallCount;

        IContent? result = await ContentService.GetParentAsync(subpage!, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(Textpage.Key));
        Assert.That(idKeyMapSpy.GetKeyForIdAsyncCallCount, Is.EqualTo(callCountBeforeGetParent), "GetParentAsync must resolve the parent via the already-populated ParentKey, not via IIdKeyMap");
    }

    [Test]
    public async Task GetChildrenAsync_Returns_Children_Of_Parent()
    {
        PagedModel<IContent> result = await ContentService.GetChildrenAsync(Textpage.Key, 0, 100, propertyAliases: null, ordering: null, CancellationToken.None);

        Assert.That(result.Total, Is.EqualTo(3));
        Assert.That(result.Items.Select(c => c.Key), Is.EquivalentTo(new[] { Subpage.Key, Subpage2.Key, Subpage3.Key }));
        Assert.That(result.Items.All(c => c.TemplateId.HasValue), Is.True);
    }

    [Test]
    public async Task GetChildrenAsync_WithNullParentKey_Returns_Root_Content()
    {
        PagedModel<IContent> result = await ContentService.GetChildrenAsync(null, 0, 100, propertyAliases: null, ordering: null, CancellationToken.None);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.Single().Key, Is.EqualTo(Textpage.Key));
    }

    [Test]
    public async Task GetChildrenWithoutTemplatesAsync_Returns_Children_Of_Parent_With_Null_Templates()
    {
        PagedModel<IContent> result = await ContentService.GetChildrenWithoutTemplatesAsync(Textpage.Key, 0, 100, propertyAliases: null, ordering: null, CancellationToken.None);

        Assert.That(result.Total, Is.EqualTo(3));
        Assert.That(result.Items.Select(c => c.Key), Is.EquivalentTo(new[] { Subpage.Key, Subpage2.Key, Subpage3.Key }));
        Assert.That(result.Items.All(c => c.TemplateId.HasValue), Is.False);
    }

    [Test]
    public async Task GetDescendantsAsync_WithNullOrdering_OrdersAncestorsBeforeTheirOwnDescendants()
    {
        var grandchild = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild", Subpage.Id);
        await ContentService.SaveAsync(grandchild, -1, null, CancellationToken.None);

        // Force the grandchild's SortOrder well ahead of its own ancestor, so a fallback-to-SortOrder
        // implementation would (wrongly) place it first — only real path ordering keeps it after Subpage.
        grandchild.SortOrder = -100;
        await ContentService.SaveAsync(grandchild, -1, null, CancellationToken.None);

        PagedModel<IContent> result = await ContentService.GetDescendantsAsync(Textpage.Key, 0, 100, ordering: null, CancellationToken.None);

        List<IContent> items = result.Items.ToList();
        var subpageIndex = items.FindIndex(c => c.Key == Subpage.Key);
        var grandchildIndex = items.FindIndex(c => c.Key == grandchild.Key);

        Assert.That(subpageIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(grandchildIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(subpageIndex, Is.LessThan(grandchildIndex),
            "Omitting ordering must default to path order, returning an ancestor before its own descendant");
    }

    [Test]
    public async Task Can_Remove_Property_Type()
    {
        // Arrange
        // Act
        var content = await ContentService.CreateAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.HasIdentity, Is.False);
    }

    [Test]
    public async Task Can_Create_Content()
    {
        // Arrange
        // Act
        var content = await ContentService.CreateAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.HasIdentity, Is.False);
    }

    public async Task Can_Create_Content_Without_Explicitly_Set_User()
    {
        // Arrange
        // Act
        var content = await ContentService.CreateAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.HasIdentity, Is.False);
        Assert.That(
            content.CreatorId,
            Is.EqualTo(Constants.Security.SuperUserId)); // Default to -1 aka SuperUser (unknown) since we didn't explicitly set this in the Create call
    }

    [Test]
    public async Task Can_Save_New_Content_With_Explicit_User()
    {
        var user = new UserBuilder().Build();
        UserService.Save(user);
        var content = new Content("Test", Constants.System.Root, await ContentTypeService.GetAsync("umbTextpage"));

        // Act
        await ContentService.SaveAsync(content, user.Id, null, CancellationToken.None);

        // Assert
        Assert.That(content.CreatorId, Is.EqualTo(user.Id));
        Assert.That(content.WriterId, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task SaveAsync_NewContentWithExplicitUser_SetsCreatorAndWriter()
    {
        var user = new UserBuilder().Build();
        UserService.Save(user);
        var content = new Content("Test", Constants.System.Root, await ContentTypeService.GetAsync("umbTextpage"));

        // Act
        await ContentService.SaveAsync(content, user.Id, null, CancellationToken.None);

        // Assert
        Assert.That(content.CreatorId, Is.EqualTo(user.Id));
        Assert.That(content.WriterId, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task SaveAsync_SecondSaveByDifferentUser_PreservesCreatorUpdatesWriter()
    {
        var creator = new UserBuilder().Build();
        UserService.Save(creator);
        var writer = new UserBuilder().Build();
        UserService.Save(writer);
        var content = new Content("Test", Constants.System.Root, await ContentTypeService.GetAsync("umbTextpage"));

        // Act
        await ContentService.SaveAsync(content, creator.Id, null, CancellationToken.None);
        content.Name = "Test Updated";
        await ContentService.SaveAsync(content, writer.Id, null, CancellationToken.None);

        // Assert
        Assert.That(content.CreatorId, Is.EqualTo(creator.Id));
        Assert.That(content.WriterId, Is.EqualTo(writer.Id));
    }

    [Test]
    public void Cannot_Create_Content_With_Non_Existing_ContentType_Alias() =>
        Assert.ThrowsAsync<Exception>(() => ContentService.CreateAsync("Test", (Guid?)null, "umbAliasDoesntExist", Constants.Security.SuperUserKey, CancellationToken.None));

    [Test]
    public async Task Cannot_Save_Content_With_Empty_Name()
    {
        // Arrange
        var content = new Content(string.Empty, Constants.System.Root, await ContentTypeService.GetAsync("umbTextpage"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await ContentService.SaveAsync(content, null, null, CancellationToken.None));
    }

    [Test]
    public async Task SaveAsync_EmptyInvariantName_Throws()
    {
        // Arrange
        var content = new Content(string.Empty, Constants.System.Root, await ContentTypeService.GetAsync("umbTextpage"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => ContentService.SaveAsync(content, null, null, CancellationToken.None));
    }

    [Test]
    public async Task Can_Get_Content_By_Id()
    {
        // Arrange
        // Act
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Id, Is.EqualTo(Textpage.Id));
    }

    [Test]
    public void Can_Get_Content_By_Guid_Key()
    {
        // Arrange
        // Act
        var content = ContentService.GetByIdAsync(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"), CancellationToken.None).GetAwaiter().GetResult();

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Id, Is.EqualTo(Textpage.Id));
    }

    [Test]
    public async Task Can_Get_Content_By_Level()
    {
        // Arrange
        // Act
        PagedModel<IContent> contents = await ContentService.GetByLevelAsync(2, 0, 100, Ordering.By("Path"), CancellationToken.None);

        // Assert
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Items.Any(), Is.True);
        Assert.That(contents.Items.Count(), Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_All_Versions_Of_Content()
    {
        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsFalse(parent.Published);
        await ContentService.SaveAsync(parent, null, null, CancellationToken.None); // publishing parent, so Text Page 2 can be updated.
        ContentService.Publish(parent, parent.AvailableCultures.ToArray());

        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        Assert.IsFalse(content.Published);
        var versions = (await ContentService.GetVersionsAsync(Subpage.Key, CancellationToken.None)).ToList();
        Assert.AreEqual(1, versions.Count);

        var version1 = content.VersionId;
        Console.WriteLine($"1 e={content.VersionId} p={content.PublishedVersionId}");

        content.Name = "Text Page 2 Updated";
        content.SetValue("author", "Jane Doe");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None); // publishes the current version, creates a version
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        var version2 = content.VersionId;
        Console.WriteLine($"2 e={content.VersionId} p={content.PublishedVersionId}");

        content.Name = "Text Page 2 ReUpdated";
        content.SetValue("author", "Bob Hope");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None); // publishes again, creates a version
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        var version3 = content.VersionId;
        Console.WriteLine($"3 e={content.VersionId} p={content.PublishedVersionId}");

        var content1 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.AreEqual("Bob Hope", content1.GetValue("author"));
        Assert.AreEqual("Bob Hope", content1.GetValue("author", published: true));

        content.Name = "Text Page 2 ReReUpdated";
        content.SetValue("author", "John Farr");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None); // no new version

        content1 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.AreEqual("John Farr", content1.GetValue("author"));
        Assert.AreEqual("Bob Hope", content1.GetValue("author", published: true));

        versions = (await ContentService.GetVersionsAsync(Subpage.Key, CancellationToken.None)).ToList();
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
    public async Task Can_Get_Root_Content()
    {
        // Arrange
        // Act
        var contents = (await ContentService.GetRootContentAsync(CancellationToken.None)).ToList();

        // Assert
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Any(), Is.True);
        Assert.That(contents.Count(), Is.EqualTo(1));
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_Content_For_Expiration()
    {
        // Arrange
        var root = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        ContentService.Publish(root!, root!.AvailableCultures.ToArray());
        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddSeconds(1));
        ContentService.PersistContentSchedule(content!, contentSchedule);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // Act
        Thread.Sleep(new TimeSpan(0, 0, 0, 2));
        var contents = (await ContentService.GetContentForExpirationAsync(DateTime.UtcNow, CancellationToken.None)).ToList();

        // Assert
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Any(), Is.True);
        Assert.That(contents.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Can_Get_Content_Schedules_By_Keys()
    {
        // Arrange
        var root = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        ContentService.Publish(root!, root!.AvailableCultures.ToArray());
        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddDays(1), null);
        ContentService.PersistContentSchedule(content!, contentSchedule);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // Act
        var results = (await ContentService.GetContentSchedulesByKeysAsync([Textpage.Key, Subpage.Key, Subpage2.Key], CancellationToken.None)).ToList();

        // Assert
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(Subpage.Key, results[0].Key);
        Assert.AreEqual(contentSchedule.FullSchedule.First().Id, results[0].Value.First().Id);
    }

    [Test]
    public async Task Can_Get_Content_For_Release()
    {
        // Arrange
        // Act
        var contents = (await ContentService.GetContentForReleaseAsync(DateTime.UtcNow, CancellationToken.None)).ToList();

        // Assert
        Assert.That(DateTime.UtcNow.AddMinutes(-5) <= DateTime.UtcNow);
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Any(), Is.True);
        Assert.That(contents.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Can_Get_Content_In_RecycleBin()
    {
        // Arrange
        // Act
        var contents = (await ContentService.GetPagedContentInRecycleBinAsync(0, int.MaxValue, ordering: null, CancellationToken.None)).Items.ToList();

        // Assert
        Assert.That(contents, Is.Not.Null);
        Assert.That(contents.Any(), Is.True);
        Assert.That(contents.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Can_Unpublish_Content()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
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
    public async Task Can_Unpublish_Content_Variation()
    {
        var (content, langUk, langFr, contentType) = await CreateEnglishAndFrenchDocument();

        var saved = await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
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
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
    }

    [Test]
    [LongRunning]
    public async Task Can_Publish_Culture_After_Last_Culture_Unpublished()
    {
        var (content, langUk, langFr, contentType) = await CreateEnglishAndFrenchDocument();

        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        var unpublished = ContentService.Unpublish(content, langUk.IsoCode); // first culture
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        unpublished = ContentService.Unpublish(content, langFr.IsoCode); // last culture
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishLastCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        published = ContentService.Publish(content, new[] { langUk.IsoCode });
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None); // reget
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
    }

    [Test]
    public async Task Unpublish_All_Cultures_Has_Unpublished_State()
    {
        var (content, langUk, langFr, contentType) = await CreateEnglishAndFrenchDocument();

        var saved = await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
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
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

        unpublished = ContentService.Unpublish(content, langUk.IsoCode); // last culture
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishLastCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
        Assert.AreEqual(
            PublishedState.Unpublished,
            content.PublishedState); // the last culture was unpublished so the document should also reflect this

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
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
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-fr", langFr.IsoCode);
        content.SetCultureName("content-en", langUk.IsoCode);

        var saved = await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        var unpublished = ContentService.Unpublish(content, langUk.IsoCode); // unpublish mandatory lang
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishMandatoryCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode)); // remains published
        Assert.AreEqual(PublishedState.Unpublished, content.PublishedState);
    }

    [Test]
    public async Task Unpublishing_Already_Unpublished_Culture()
    {
        var (content, langUk, langFr, contentType) = await CreateEnglishAndFrenchDocument();

        var saved = await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        var unpublished = ContentService.Unpublish(content, langUk.IsoCode);
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishCulture, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        // Change some data since Unpublish should always Save
        content.SetCultureName("content-en-updated", langUk.IsoCode);

        unpublished = ContentService.Unpublish(content, langUk.IsoCode); // unpublish again
        Assert.IsTrue(unpublished.Success);
        Assert.AreEqual(PublishResultType.SuccessUnpublishAlready, unpublished.Result);
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        // ensure that even though the culture was already unpublished that the data was still persisted
        Assert.AreEqual("content-en-updated", content.GetCultureName(langUk.IsoCode));
    }

    [Test]
    public async Task Publishing_No_Cultures_Still_Saves()
    {
        var (content, langUk, langFr, contentType) = await CreateEnglishAndFrenchDocument();

        var saved = await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        // Change some data since SaveAndPublish should always Save
        content.SetCultureName("content-en-updated", langUk.IsoCode);

        saved = await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        published = ContentService.Publish(content, new string[] { }); // publish without cultures
        Assert.IsTrue(saved.Success);
        Assert.AreEqual(PublishResultType.FailedPublishNothingToPublish, published.Result);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

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

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-en", langGb.IsoCode);
        content.SetCultureName("content-fr", langFr.IsoCode);

        Assert.IsTrue((await ContentService.SaveAsync(content, null, null, CancellationToken.None)).Success);
        Assert.IsTrue(ContentService.Publish(content, new[] { langGb.IsoCode, langFr.IsoCode }).Success);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
        Assert.IsTrue(content.IsCulturePublished(langGb.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCultureEdited(langGb.IsoCode));
        Assert.IsFalse(content.IsCultureEdited(langFr.IsoCode));

        // update the invariant property and save a pending version
        content.SetValue("metakeywords", "hello");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
        Assert.IsTrue(content.IsCulturePublished(langGb.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCultureEdited(langGb.IsoCode));
        Assert.IsFalse(content.IsCultureEdited(langFr.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Content_Variation_And_Detect_Changed_Cultures()
    {
        var (langUk, langFr, contentType) = await CreateEnglishAndFrenchDocumentType();

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-fr", langFr.IsoCode);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var published = ContentService.Publish(content, new[] { langFr.IsoCode });

        // audit log will only show that french was published
        var lastLog = (await AuditService.GetItemsByEntityAsync(content.Id, 0, 1)).Items.First();
        Assert.AreEqual("Published languages: fr-FR", lastLog.Comment);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        content.SetCultureName("content-en", langUk.IsoCode);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        published = ContentService.Publish(content, new[] { langUk.IsoCode });

        // audit log will only show that english was published
        lastLog = (await AuditService.GetItemsByEntityAsync(content.Id, 0, 1)).Items.First();
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
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-fr", langFr.IsoCode);
        content.SetCultureName("content-gb", langGb.IsoCode);
        var saved = await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var published = ContentService.Publish(content, new[] { langGb.IsoCode, langFr.IsoCode });
        Assert.IsTrue(saved.Success);
        Assert.IsTrue(published.Success);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        var unpublished = ContentService.Unpublish(content, langFr.IsoCode);

        // audit log will only show that french was unpublished
        var lastLog = (await AuditService.GetItemsByEntityAsync(content.Id, 0, 1)).Items.First();
        Assert.AreEqual("Unpublished languages: fr-FR", lastLog.Comment);

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        content.SetCultureName("content-en", langGb.IsoCode);
        unpublished = ContentService.Unpublish(content, langGb.IsoCode);

        // audit log will only show that english was published
        var logs = (await AuditService.GetItemsByEntityAsync(content.Id, 0, int.MaxValue, Direction.Ascending)).Items.ToList();
        Assert.AreEqual("Unpublished languages: en-GB", logs[^2].Comment);
        Assert.AreEqual("Unpublished (mandatory language unpublished)", logs[^1].Comment);
    }

    [Test]
    public async Task Can_Publish_Content_1()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsNotNull(content);

        // Act
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

        // Assert
        Assert.That(published.Success, Is.True);
        Assert.That(content.Published, Is.True);
    }

    [Test]
    public async Task Can_Publish_Content_2()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsNotNull(content);

        // Act
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: -1);

        // Assert
        Assert.That(published.Success, Is.True);
        Assert.That(content.Published, Is.True);
    }

    [Test]
    public async Task IsPublishable()
    {
        // Arrange
        var parent = await ContentService.CreateAsync("parent", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        await ContentService.SaveAsync(parent, null, null, CancellationToken.None);
        ContentService.Publish(parent, parent.AvailableCultures.ToArray());
        var content = await ContentService.CreateAsync("child", parent, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        Assert.IsTrue(await ContentService.IsPathPublishableAsync(content, CancellationToken.None));
        ContentService.Unpublish(parent);
        Assert.IsFalse(await ContentService.IsPathPublishableAsync(content, CancellationToken.None));
    }

    [Test]
    public async Task Can_Publish_Content_WithEvents()
    {
        var savingWasCalled = false;
        var publishingWasCalled = false;

        ContentNotificationHandler.SavingContent = notification =>
        {
            Assert.AreEqual(1, notification.SavedEntities.Count());
            var entity = notification.SavedEntities.First();
            Assert.AreEqual("foo", entity.Name);

            var e = ContentService.GetByIdAsync(entity.Key, CancellationToken.None).GetAwaiter().GetResult();
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
            var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
            Assert.AreEqual("Textpage", content.Name);

            content.Name = "foo";
            await ContentService.SaveAsync(content, null, null, CancellationToken.None);
            var published =
                ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

            Assert.That(published.Success, Is.True);
            Assert.That(content.Published, Is.True);

            var e = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
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
    public async Task Can_Not_Publish_Invalid_Cultures_For_Variant_Content()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("Name for en-US", "en-US");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        Assert.Throws<ArgumentNullException>(() => ContentService.Publish(content, null!));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new string[] { null }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new [] { string.Empty }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "*", null }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "en-US", "*" }));
    }

    [Test]
    public async Task Can_Not_Publish_Invalid_Cultures_For_Invariant_Content()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.Name = "Content name";
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        Assert.Throws<ArgumentNullException>(() => ContentService.Publish(content, null!));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new string[] { null! }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "*", null! }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "en-US" }));
        Assert.Throws<ArgumentException>(() => ContentService.Publish(content, new[] { "en-US", "*" }));
    }

    [Test]
    public async Task Can_Publish_Only_Valid_Content()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(
            "umbMandatory",
            "Mandatory Doc Type",
            mandatoryProperties: true,
            defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parentId = Textpage.Id;

        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        await ContentService.SaveAsync(parent, null, null, CancellationToken.None);
        var parentPublished = ContentService.Publish(parent, parent.AvailableCultures.ToArray());

        // parent can publish values
        // and therefore can be published
        Assert.IsTrue(parentPublished.Success);
        Assert.IsTrue(parent.Published);

        var content = ContentBuilder.CreateSimpleContent(contentType, "Invalid Content", parentId);
        content.SetValue("author", string.Empty);
        Assert.IsFalse(content.HasIdentity);

        // content cannot publish values because they are invalid
        var propertyValidationService = new PropertyValidationService(PropertyEditorCollection, DataTypeService, LocalizedTextService, ValueEditorCache, Mock.Of<ICultureDictionary>(), Mock.Of<ILanguageService>(), Mock.Of<IOptions<ContentSettings>>());
        var isValid = propertyValidationService.IsPropertyDataValid(
            content,
            out var invalidProperties,
            CultureImpact.Invariant);
        Assert.IsFalse(isValid);
        Assert.IsNotEmpty(invalidProperties);

        // and therefore cannot be published,
        // because it did not have a published version at all
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
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
        await ContentTypeService.CreateAsync(ct, Constants.Security.SuperUserKey);

        IContent content = ContentBuilder.CreateBasicContent(ct);
        content.SetCultureName("name-fr", langFr.IsoCode);
        content.SetCultureName("name-da", langDa.IsoCode);

        content.PublishCulture(CultureImpact.Explicit(langFr.IsoCode, langFr.IsDefault), DateTime.UtcNow, PropertyEditorCollection);
        var result = ContentService.CommitDocumentChanges(content);
        Assert.IsTrue(result.Success);
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langDa.IsoCode));

        content.UnpublishCulture(langFr.IsoCode);
        content.PublishCulture(CultureImpact.Explicit(langDa.IsoCode, langDa.IsDefault), DateTime.UtcNow, PropertyEditorCollection);

        result = ContentService.CommitDocumentChanges(content);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(PublishResultType.SuccessMixedCulture, result.Result);

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
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
    public async Task Can_Publish_Content_Children()
    {
        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        Console.WriteLine(" " + parent.Id);
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            PagedModel<IContent> descendantsPage = await ContentService.GetDescendantsAsync(parent.Key, page++ * pageSize, pageSize, ordering: null, CancellationToken.None);
            total = descendantsPage.Total;
            foreach (var x in descendantsPage.Items)
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

        // we only want the first so page size, etc.. is abitrary
        var children = (await ContentService.GetChildrenAsync(parent.Key, 0, 500, propertyAliases: null, ordering: null, CancellationToken.None)).Items;

        // children are published including ... that was released 5 mins ago
        Assert.IsTrue(children.First(x => x.Id == Subpage.Id).Published);
    }

    [Test]
    public async Task Cannot_Publish_Expired_Content()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None); // This Content expired 5min ago
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddMinutes(-5));
        await ContentService.SaveAsync(content, null, contentSchedule, CancellationToken.None);

        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsNotNull(parent);
        var parentPublished =
            ContentService.Publish(
                parent,
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
    public async Task Cannot_Publish_Expired_Culture()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("Hello", "en-US");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry("en-US", null, DateTime.UtcNow.AddMinutes(-5));
        await ContentService.SaveAsync(content, null, contentSchedule, CancellationToken.None);

        var published = ContentService.Publish(content, new[] { "en-US" });

        Assert.IsFalse(published.Success);
        Assert.AreEqual(PublishResultType.FailedPublishCultureHasExpired, published.Result);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public async Task Cannot_Publish_Content_Awaiting_Release()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddHours(2), null);
        await ContentService.SaveAsync(content, Constants.Security.SuperUserId, contentSchedule, CancellationToken.None);

        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsNotNull(parent);
        var parentPublished =
            ContentService.Publish(
                parent,
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
    public async Task Failed_Publish_Should_Not_Update_Edited_State_When_Edited_True()
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

        await contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Home")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("header", "Cool header")
            .Done()
            .Build();

        await contentService.SaveAsync(content, null, null, CancellationToken.None);
        contentService.Publish(content, Array.Empty<string>());

        content.Properties[0].SetValue("Foo", string.Empty);
        await contentService.SaveAsync(content, null, null, CancellationToken.None);
        contentService.PersistContentSchedule(
            content,
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
    public async Task Failed_Publish_Should_Not_Update_Edited_State_When_Edited_False()
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

        await contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Home")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("header", "Cool header")
            .Done()
            .Build();

        await contentService.SaveAsync(content, null, null, CancellationToken.None);
        contentService.Publish(content, Array.Empty<string>());

        contentService.PersistContentSchedule(
            content,
            ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddHours(2), null));
        await contentService.SaveAsync(content, null, null, CancellationToken.None);

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
    public async Task Cannot_Publish_Culture_Awaiting_Release()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("Hello", "en-US");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry("en-US", DateTime.UtcNow.AddHours(2), null);
        await ContentService.SaveAsync(content, null, contentSchedule, CancellationToken.None);

        var published = ContentService.Publish(content, new[] { "en-US" });

        Assert.IsFalse(published.Success);
        Assert.AreEqual(PublishResultType.FailedPublishCultureAwaitingRelease, published.Result);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public async Task Cannot_Publish_Content_Where_Parent_Is_Unpublished()
    {
        // Arrange
        var content = await ContentService.CreateAsync("Subpage with Unpublished Parent", Textpage.Key, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // Act
        var published = ContentService.PublishBranch(content, PublishBranchFilter.IncludeUnpublished, content.AvailableCultures.ToArray());

        // Assert
        Assert.That(published.All(x => x.Success), Is.False);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public async Task Cannot_Publish_Trashed_Content()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Trashed.Key, CancellationToken.None);
        Assert.IsNotNull(content);

        // Act
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);

        // Assert
        Assert.That(published.Success, Is.False);
        Assert.That(content.Published, Is.False);
        Assert.That(content.Trashed, Is.True);
    }

    [Test]
    public async Task Can_Save_And_Publish_Content()
    {
        // Arrange
        var content = await ContentService.CreateAsync("Home US", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        content.SetValue("author", "Barack Obama");

        // Act
        var saved = await ContentService.SaveAsync(content, Constants.Security.SuperUserId, null, CancellationToken.None);
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
    public async Task Can_Save_And_Publish_Content_And_Child_Without_Identity()
    {
        // Arrange
        var content = await ContentService.CreateAsync("Home US", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        content.SetValue("author", "Barack Obama");

        // Act
        var saved = await ContentService.SaveAsync(content, Constants.Security.SuperUserId, null, CancellationToken.None);
        var published = ContentService.Publish(content, content.AvailableCultures.ToArray(), userId: Constants.Security.SuperUserId);
        var childContent = await ContentService.CreateAsync("Child", content.Key, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        // Reset all identity properties
        childContent.Id = 0;
        childContent.Path = null;
        ((Content)childContent).ResetIdentity();
        var childSaved = await ContentService.SaveAsync(childContent, Constants.Security.SuperUserId, null, CancellationToken.None);
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
    public async Task Can_SaveAndPublish_Invariant_Content()
    {
        var content = await ContentService.CreateAsync("Home US", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        content.SetValue("author", "Barack Obama");

        var result = ContentService.SaveAndPublish(content, Array.Empty<string>());

        Assert.IsTrue(result.Success);
        Assert.That(content.HasIdentity, Is.True);
        Assert.That(content.Published, Is.True);
    }

    [Test]
    public async Task Can_SaveAndPublish_Invariant_Content_Without_Prior_Save()
    {
        var content = await ContentService.CreateAsync("Unsaved Content", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        content.SetValue("author", "Test Author");
        Assert.IsFalse(content.HasIdentity);

        var result = ContentService.SaveAndPublish(content, Array.Empty<string>());

        Assert.IsTrue(result.Success);
        Assert.That(content.HasIdentity, Is.True);
        Assert.That(content.Published, Is.True);

        // re-get to verify persistence
        var retrieved = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Test Author", retrieved.GetValue<string>("author"));
        Assert.IsTrue(retrieved.Published);
    }

    [TestCase(Constants.Security.SuperUserId)]
    [TestCase(-1)]
    public async Task Can_SaveAndPublish_With_Different_User_Ids(int userId)
    {
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsNotNull(content);

        var result = ContentService.SaveAndPublish(content, [], userId);

        Assert.IsTrue(result.Success);
        Assert.That(content.Published, Is.True);
    }

    [Test]
    public async Task Can_SaveAndPublish_Variant_Content_Multiple_Cultures()
    {
        var (content, langUk, langFr, _) = await CreateEnglishAndFrenchDocument();

        var result = ContentService.SaveAndPublish(content, [langFr.IsoCode, langUk.IsoCode]);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));

        // re-get and verify
        content = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsTrue(content.IsCulturePublished(langUk.IsoCode));
    }

    [Test]
    public async Task Can_SaveAndPublish_Variant_Content_Single_Culture()
    {
        var (content, langUk, langFr, _) = await CreateEnglishAndFrenchDocument();

        var result = ContentService.SaveAndPublish(content, [langFr.IsoCode]);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));

        // re-get and verify
        content = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;
        Assert.IsTrue(content.IsCulturePublished(langFr.IsoCode));
        Assert.IsFalse(content.IsCulturePublished(langUk.IsoCode));
    }

    [Test]
    public async Task Can_SaveAndPublish_And_Child_Without_Identity()
    {
        var content = await ContentService.CreateAsync("Home US", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        content.SetValue("author", "John Doe");

        var published = ContentService.SaveAndPublish(content, []);
        var childContent = await ContentService.CreateAsync("Child", content.Key, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        // Reset all identity properties
        childContent.Id = 0;
        childContent.Path = string.Empty;
        ((Content)childContent).ResetIdentity();
        var childPublished = ContentService.SaveAndPublish(childContent, []);

        Assert.That(content.HasIdentity, Is.True);
        Assert.That(content.Published, Is.True);
        Assert.That(childContent.HasIdentity, Is.True);
        Assert.That(childContent.Published, Is.True);
        Assert.That(published.Success, Is.True);
        Assert.That(childPublished.Success, Is.True);
    }

    [Test]
    public async Task SaveAndPublish_Fires_Notifications()
    {
        var savingWasCalled = false;
        var publishingWasCalled = false;
        var contentName = "contentName";

        ContentNotificationHandler.SavingContent = notification =>
        {
            savingWasCalled = true;
            Assert.AreEqual(1, notification.SavedEntities.Count());
            var entity = notification.SavedEntities.First();
            Assert.AreEqual(contentName, entity.Name);
        };

        ContentNotificationHandler.PublishingContent = notification =>
        {
            publishingWasCalled = true;
            Assert.AreEqual(1, notification.PublishedEntities.Count());
            var entity = notification.PublishedEntities.First();
            Assert.AreEqual(contentName, entity.Name);
        };

        try
        {
            var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
            Assert.IsNotNull(content);
            content.Name = contentName;

            var result = ContentService.SaveAndPublish(content, []);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(content.Published);
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
    public async Task SaveAndPublish_Can_Be_Cancelled_By_Saving_Notification()
    {
        ContentNotificationHandler.SavingContent = notification =>
        {
            notification.Cancel = true;
        };

        try
        {
            var content = await ContentService.CreateAsync("Cancel Me", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

            var result = ContentService.SaveAndPublish(content, Array.Empty<string>());

            Assert.IsFalse(result.Success);
            Assert.AreEqual(PublishResultType.FailedPublishCancelledByEvent, result.Result);
            Assert.IsFalse(content.Published);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
        }
    }

    [Test]
    public async Task SaveAndPublish_Rejects_Invalid_Cultures()
    {
        var (content, _, _, _) = await CreateEnglishAndFrenchDocument();

        Assert.Throws<InvalidOperationException>(() => ContentService.SaveAndPublish(content, ["*"]));
        Assert.Throws<InvalidOperationException>(() => ContentService.SaveAndPublish(content, [null!]));
        Assert.Throws<InvalidOperationException>(() => ContentService.SaveAndPublish(content, ["*", null!]));
        Assert.Throws<InvalidOperationException>(() => ContentService.SaveAndPublish(content, ["en-US", "*", "es-ES"]));
    }

    [Test]
    public async Task SaveAndPublish_Rejects_Whitespace_Cultures()
    {
        var (content, _, _, _) = await CreateEnglishAndFrenchDocument();

        Assert.Throws<ArgumentException>(() => ContentService.SaveAndPublish(content, [string.Empty]));
        Assert.Throws<ArgumentException>(() => ContentService.SaveAndPublish(content, ["   "]));
        Assert.Throws<ArgumentException>(() => ContentService.SaveAndPublish(content, ["en-US", "   "]));
    }

    [Test]
    public async Task SaveAndPublish_Rejects_Duplicate_Cultures()
    {
        var (content, langUk, _, _) = await CreateEnglishAndFrenchDocument();

        Assert.Throws<ArgumentException>(() => ContentService.SaveAndPublish(content, [langUk.IsoCode, langUk.IsoCode]));
    }

    [Test]
    public async Task SaveAndPublish_Rejects_Cultures_On_Invariant_Content()
    {
        var content = await ContentService.CreateAsync("Invariant", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.Throws<ArgumentException>(() => ContentService.SaveAndPublish(content, ["en-US"]));
    }

    [Test]
    public async Task SaveAndPublish_No_Cultures_On_Variant_Saves_But_Does_Not_Publish()
    {
        var (content, langUk, langFr, _) = await CreateEnglishAndFrenchDocument();

        // First publish both cultures
        var published = ContentService.SaveAndPublish(content, [langFr.IsoCode, langUk.IsoCode]);
        Assert.IsTrue(published.Success);

        // re-get
        content = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;

        // Change some data
        content.SetCultureName("content-en-updated", langUk.IsoCode);

        // SaveAndPublish with empty cultures - should save data but not publish
        var result = ContentService.SaveAndPublish(content, Array.Empty<string>());

        // re-get and verify data was saved even though nothing was published
        content = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;
        Assert.AreEqual("content-en-updated", content.GetCultureName(langUk.IsoCode));
    }

    [Test]
    public async Task Cannot_SaveAndPublish_Trashed_Content()
    {
        var content = await ContentService.GetByIdAsync(Trashed.Key, CancellationToken.None);
        Assert.IsNotNull(content);

        var result = ContentService.SaveAndPublish(content, Array.Empty<string>());

        Assert.IsFalse(result.Success);
        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Trashed);
    }

    [Test]
    public async Task Cannot_SaveAndPublish_Expired_Content()
    {
        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        Assert.IsNotNull(content);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddMinutes(-5));
        await ContentService.SaveAsync(content, null, contentSchedule, CancellationToken.None);

        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsNotNull(parent);
        ContentService.SaveAndPublish(parent, Array.Empty<string>());

        var result = ContentService.SaveAndPublish(content, Array.Empty<string>());

        Assert.IsFalse(result.Success);
        Assert.IsFalse(content.Published);
        Assert.AreEqual(PublishResultType.FailedPublishHasExpired, result.Result);
    }

    [Test]
    public async Task Cannot_SaveAndPublish_Expired_Culture()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("Hello", "en-US");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry("en-US", null, DateTime.UtcNow.AddMinutes(-5));
        await ContentService.SaveAsync(content, null, contentSchedule, CancellationToken.None);

        var result = ContentService.SaveAndPublish(content, ["en-US"]);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PublishResultType.FailedPublishCultureHasExpired, result.Result);
        Assert.IsFalse(content.Published);
    }

    [Test]
    public async Task Cannot_SaveAndPublish_Content_Awaiting_Release()
    {
        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        Assert.IsNotNull(content);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddHours(2), null);
        await ContentService.SaveAsync(content, Constants.Security.SuperUserId, contentSchedule, CancellationToken.None);

        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsNotNull(parent);
        ContentService.SaveAndPublish(parent, Array.Empty<string>());

        var result = ContentService.SaveAndPublish(content, Array.Empty<string>());

        Assert.IsFalse(result.Success);
        Assert.IsFalse(content.Published);
        Assert.AreEqual(PublishResultType.FailedPublishAwaitingRelease, result.Result);
    }

    [Test]
    public async Task Cannot_SaveAndPublish_Culture_Awaiting_Release()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("Hello", "en-US");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry("en-US", DateTime.UtcNow.AddHours(2), null);
        await ContentService.SaveAsync(content, null, contentSchedule, CancellationToken.None);

        var result = ContentService.SaveAndPublish(content, ["en-US"]);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PublishResultType.FailedPublishCultureAwaitingRelease, result.Result);
        Assert.IsFalse(content.Published);
    }

    [Test]
    public async Task SaveAndPublish_Invalid_Content_Still_Saves()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(
            "umbMandatory",
            "Mandatory Doc Type",
            mandatoryProperties: true,
            defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parentId = Textpage.Id;
        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsNotNull(parent);
        ContentService.SaveAndPublish(parent, Array.Empty<string>());

        var content = ContentBuilder.CreateSimpleContent(contentType, "Invalid Content", parentId);
        content.SetValue("author", string.Empty);
        Assert.IsFalse(content.HasIdentity);

        var result = ContentService.SaveAndPublish(content, Array.Empty<string>());

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PublishResultType.FailedPublishContentInvalid, result.Result);
        Assert.IsFalse(content.Published);

        // content IS saved even though publish failed
        Assert.Greater(content.Id, 0);
        Assert.IsTrue(content.HasIdentity);
    }

    [Test]
    [LongRunning]
    public async Task Failed_SaveAndPublish_Preserves_Edited_State()
    {
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

        await contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Home")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("header", "Cool header")
            .Done()
            .Build();

        contentService.SaveAndPublish(content, Array.Empty<string>());

        content.Properties[0]!.SetValue("forcedPropertyValue", string.Empty);
        await contentService.SaveAsync(content, null, null, CancellationToken.None);
        contentService.PersistContentSchedule(
            content,
            ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddHours(2), null));

        var result = contentService.SaveAndPublish(content, Array.Empty<string>());

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Content.Published);
            Assert.AreEqual(PublishResultType.FailedPublishAwaitingRelease, result.Result);
            Assert.IsTrue(result.Content.Edited, "result.Content.Edited");
        });
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_Published_Descendant_Versions()
    {
        // Arrange
        var root = (await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None))!;
        var rootPublished = ContentService.Publish(root, root.AvailableCultures.ToArray());

        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        content.Properties["title"].SetValue(content.Properties["title"].GetValue() + " Published");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var contentPublished = ContentService.Publish(content, content.AvailableCultures.ToArray());
        var publishedVersion = content.VersionId;

        content.Properties["title"].SetValue(content.Properties["title"].GetValue() + " Saved");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
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
        Assert.That(
            publishedContentVersion.Properties["title"].GetValue(published: true),
            Contains.Substring("Published"));

        // and has the correct draft properties
        Assert.That(publishedContentVersion.Properties["title"].GetValue(), Contains.Substring("Saved"));

        // Ensure that the latest version of the content is ok
        var currentContent = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        Assert.That(currentContent.Published, Is.True);
        Assert.That(currentContent.Properties["title"].GetValue(published: true), Contains.Substring("Published"));
        Assert.That(currentContent.Properties["title"].GetValue(), Contains.Substring("Saved"));
        Assert.That(currentContent.VersionId, Is.EqualTo(publishedContentVersion.VersionId));
    }

    [Test]
    public async Task Can_Save_Content()
    {
        // Arrange
        var content = await ContentService.CreateAsync("Home US", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        content.SetValue("author", "Barack Obama");

        // Act
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // Assert
        Assert.That(content.HasIdentity, Is.True);
    }

    [Test]
    public async Task SaveAsync_BasicContent_AssignsIdentity()
    {
        // Arrange
        var content = await ContentService.CreateAsync("Home US", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        content.SetValue("author", "Barack Obama");

        // Act
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // Assert
        Assert.That(content.HasIdentity, Is.True);
    }

    [Test]
    public async Task SaveAsync_SavingNotificationCancelled_ReturnsCancelAndDoesNotPersist()
    {
        ContentNotificationHandler.SavingContent = notification =>
        {
            notification.Cancel = true;
        };

        try
        {
            var content = await ContentService.CreateAsync("Cancel Me", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

            Attempt<ContentSaveOperationStatus> result = await ContentService.SaveAsync(content, null, null, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentSaveOperationStatus.CancelledByNotification, result.Result);
            Assert.IsFalse(content.HasIdentity);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
        }
    }

    [Test]
    public async Task SaveAsync_NameTooLong_ReturnsInvalidNameStatus()
    {
        var content = await ContentService.CreateAsync(new string('a', 256), (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

        Attempt<ContentSaveOperationStatus> result = await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentSaveOperationStatus.InvalidName, result.Result);
        Assert.IsFalse(content.HasIdentity);
    }

    [Test]
    public async Task SaveAsync_PublishedStateNotAllowed_ReturnsInvalidPublishedStateStatus()
    {
        var content = await ContentService.CreateAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        content.PublishedState = PublishedState.Publishing;

        Attempt<ContentSaveOperationStatus> result = await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentSaveOperationStatus.InvalidPublishedState, result.Result);
        Assert.IsFalse(content.HasIdentity);
    }

    [Test]
    public async Task Can_Update_Content_Property_Values()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        IContentType contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        IContent content = ContentBuilder.CreateSimpleContent(contentType, "hello");
        content.SetValue("title", "title of mine");
        content.SetValue("bodyText", "hello world");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        content.SetValue("title", "another title of mine"); // Change a value
        content.SetValue("bodyText", null); // Clear a value
        content.SetValue("author", "new author"); // Add a value
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.AreEqual("another title of mine", content.GetValue("title"));
        Assert.IsNull(content.GetValue("bodyText"));
        Assert.AreEqual("new author", content.GetValue("author"));

        content.SetValue("title", "new title");
        content.SetValue("bodyText", "new body text");
        content.SetValue("author", "new author text");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None); // new non-published version

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        content.SetValue("title", null); // Clear a value
        content.SetValue("bodyText", null); // Clear a value
        await ContentService.SaveAsync(content, null, null, CancellationToken.None); // saving non-published version

        // re-get
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.IsNull(content.GetValue("title")); // Test clearing the value worked with the non-published version
        Assert.IsNull(content.GetValue("bodyText"));
        Assert.AreEqual("new author text", content.GetValue("author"));

        // make sure that the published version remained the same
        var publishedContent = await ContentService.GetVersionAsync(content.PublishedVersionId, CancellationToken.None);
        Assert.AreEqual("another title of mine", publishedContent.GetValue("title"));
        Assert.IsNull(publishedContent.GetValue("bodyText"));
        Assert.AreEqual("new author", publishedContent.GetValue("author"));
    }

    [Test]
    public async Task Can_Bulk_Save_Content()
    {
        // Arrange
        var contentType = await ContentTypeService.GetAsync("umbTextpage");
        var subpage = ContentBuilder.CreateSimpleContent(contentType, "Text Subpage 1", Textpage.Id);
        var subpage2 = ContentBuilder.CreateSimpleContent(contentType, "Text Subpage 2", Textpage.Id);
        var list = new List<IContent> { subpage, subpage2 };

        // Act
        ContentService.Save(list);

        // Assert
        Assert.That(list.Any(x => !x.HasIdentity), Is.False);
    }

    [Test]
    public async Task Can_Bulk_Save_New_Hierarchy_Content()
    {
        // Arrange
        var hierarchy = (await CreateContentHierarchy()).ToList();

        // Act
        ContentService.Save(hierarchy);

        Assert.That(hierarchy.Any(), Is.True);
        Assert.That(hierarchy.Any(x => x.HasIdentity == false), Is.False);

        // all parent id's should be ok, they are lazy and if they equal zero an exception will be thrown
        Assert.DoesNotThrow(() => hierarchy.Any(x => x.ParentId != 0));
    }

    [Test]
    public async Task Can_Delete_Content_Of_Specific_ContentType()
    {
        // Arrange
        var contentType = await ContentTypeService.GetAsync("umbTextpage");

        // Act
        ContentService.DeleteOfType(contentType.Id);
        var rootContent = await ContentService.GetRootContentAsync(CancellationToken.None);
        PagedModel<IContent> contents = await ContentService.GetPagedOfTypeAsync(contentType.Key, 0, int.MaxValue, ordering: null, CancellationToken.None);

        // Assert
        Assert.That(rootContent.Any(), Is.False);
        Assert.That(contents.Items.Any(x => !x.Trashed), Is.False);
    }

    [Test]
    public async Task Can_Delete_Content()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        // Act
        await ContentService.DeleteAsync(content, null, CancellationToken.None);
        var deleted = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        // Assert
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task Can_Move_Content_To_RecycleBin()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        // Act
        ContentService.MoveToRecycleBin(content);

        // Assert
        Assert.That(content.ParentId, Is.EqualTo(-20));
        Assert.That(content.Trashed, Is.True);
    }

    [Test]
    [LongRunning]
    public async Task Can_Move_Content_Structure_To_RecycleBin_And_Empty_RecycleBin()
    {
        var contentType = await ContentTypeService.GetAsync("umbTextpage");

        var subsubpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 3", Subpage.Id);
        await ContentService.SaveAsync(subsubpage, null, null, CancellationToken.None);

        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        var descendants = new List<IContent>();
        while (page * pageSize < total)
        {
            PagedModel<IContent> descendantsPage = await ContentService.GetDescendantsAsync(content.Key, page++ * pageSize, pageSize, ordering: null, CancellationToken.None);
            total = descendantsPage.Total;
            descendants.AddRange(descendantsPage.Items);
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
            PagedModel<IContent> descendantsPage = await ContentService.GetDescendantsAsync(content.Key, page++ * pageSize, pageSize, ordering: null, CancellationToken.None);
            total = descendantsPage.Total;
            descendants.AddRange(descendantsPage.Items);
        }

        Assert.AreEqual(-20, content.ParentId);
        Assert.IsTrue(content.Trashed);
        Assert.AreEqual(4, descendants.Count);
        Assert.IsTrue(descendants.All(x => x.Path.StartsWith("-1,-20,")));
        Assert.True(descendants.All(x => x.Trashed));

        await ContentService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);
        var trashed = (await ContentService.GetPagedContentInRecycleBinAsync(0, int.MaxValue, ordering: null, CancellationToken.None)).Items.ToList();
        Assert.IsEmpty(trashed);
    }

    [Test]
    public async Task Can_Empty_RecycleBin()
    {
        // Arrange
        // Act
        await ContentService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);
        var contents = (await ContentService.GetPagedContentInRecycleBinAsync(0, int.MaxValue, ordering: null, CancellationToken.None)).Items.ToList();

        // Assert
        Assert.That(contents.Any(), Is.False);
    }

    [Test]
    public async Task RecycleBinSmellsAsync_ReflectsRecycleBinContents()
    {
        // Arrange - the Trashed fixture is already in the recycle bin.
        // Act, Assert
        Assert.That(await ContentService.RecycleBinSmellsAsync(CancellationToken.None), Is.True);

        await ContentService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);

        Assert.That(await ContentService.RecycleBinSmellsAsync(CancellationToken.None), Is.False);
    }

    [Test]
    [LongRunning]
    public async Task Ensures_Permissions_Are_Retained_For_Copied_Descendants_With_Explicit_Permissions()
    {
        // Arrange
        var userGroup = UserGroupBuilder.CreateUserGroup("1");
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);
        contentType.AllowedContentTypes = new List<ContentTypeSort>
        {
            new(contentType.Key, 0, contentType.Alias)
        };
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parentPage = ContentBuilder.CreateSimpleContent(contentType);
        await ContentService.SaveAsync(parentPage, null, null, CancellationToken.None);

        var childPage = ContentBuilder.CreateSimpleContent(contentType, "child", parentPage);
        await ContentService.SaveAsync(childPage, null, null, CancellationToken.None);

        // assign explicit permissions to the child
        await ContentService.SetPermissionAsync(childPage, "A", new[] { userGroup.Key }, CancellationToken.None);

        // Ok, now copy, what should happen is the childPage will retain it's own permissions
        var parentPage2 = ContentBuilder.CreateSimpleContent(contentType);
        await ContentService.SaveAsync(parentPage2, null, null, CancellationToken.None);

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
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);
        contentType.AllowedContentTypes = new List<ContentTypeSort>
        {
            new(contentType.Key, 0, contentType.Alias)
        };
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var parentPage = ContentBuilder.CreateSimpleContent(contentType);
        await ContentService.SaveAsync(parentPage, null, null, CancellationToken.None);
        await ContentService.SetPermissionAsync(parentPage, "A", new[] { userGroup.Key }, CancellationToken.None);

        var childPage1 = ContentBuilder.CreateSimpleContent(contentType, "child1", parentPage);
        await ContentService.SaveAsync(childPage1, null, null, CancellationToken.None);
        var childPage2 = ContentBuilder.CreateSimpleContent(contentType, "child2", childPage1);
        await ContentService.SaveAsync(childPage2, null, null, CancellationToken.None);
        var childPage3 = ContentBuilder.CreateSimpleContent(contentType, "child3", childPage2);
        await ContentService.SaveAsync(childPage3, null, null, CancellationToken.None);

        // Verify that the children have the inherited permissions
        var descendants = new List<IContent>();
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            PagedModel<IContent> descendantsPage = await ContentService.GetDescendantsAsync(parentPage.Key, page++ * pageSize, pageSize, ordering: null, CancellationToken.None);
            total = descendantsPage.Total;
            descendants.AddRange(descendantsPage.Items);
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
        await ContentService.SaveAsync(parentPage2, null, null, CancellationToken.None);
        await ContentService.SetPermissionAsync(parentPage2, "B", new[] { userGroup.Key }, CancellationToken.None);

        // Now copy, what should happen is the child pages will now have permissions inherited from the new parent
        var copy = ContentService.Copy(childPage1, parentPage2.Id, false, true);

        descendants.Clear();
        page = 0;
        while (page * pageSize < total)
        {
            PagedModel<IContent> descendantsPage = await ContentService.GetDescendantsAsync(parentPage2.Key, page++ * pageSize, pageSize, ordering: null, CancellationToken.None);
            total = descendantsPage.Total;
            descendants.AddRange(descendantsPage.Items);
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
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        object obj =
            new { tags = "[\"Hello\",\"World\"]" };
        var content1 = ContentBuilder.CreateBasicContent(contentType);
        content1.PropertyValues(obj);
        content1.ResetDirtyProperties(false);
        await ContentService.SaveAsync(content1, null, null, CancellationToken.None);
        Assert.IsTrue(ContentService.Publish(
            content1,
            content1.AvailableCultures.ToArray(),
            userId: -1).Success);
        var content2 = ContentBuilder.CreateBasicContent(contentType);
        content2.PropertyValues(obj);
        content2.ResetDirtyProperties(false);
        await ContentService.SaveAsync(content2, null, null, CancellationToken.None);
        Assert.IsTrue(ContentService.Publish(
            content2,
            content2.AvailableCultures.ToArray(),
            userId: -1).Success);

        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);
        editorGroup.StartContentId = content1.Id;
        await UserGroupService.UpdateAsync(editorGroup, Constants.Security.SuperUserKey);

        var admin = await UserService.GetAsync(Constants.Security.SuperUserKey);
        admin.StartContentIds = new[] { content1.Id };
        UserService.Save(admin);

        await RelationService.SaveAsync(new RelationType(
            "test",
            "test",
            false,
            Constants.ObjectTypes.Document,
            Constants.ObjectTypes.Document,
            false));
        Assert.IsNotNull(await RelationService.RelateAsync(content1.Id, content2.Id, "test"));

        await PublicAccessService.SaveAsync(new PublicAccessEntry(
            content1,
            content2,
            content2,
            new List<PublicAccessRule> { new() { RuleType = "test", RuleValue = "test" } }));
        Assert.IsTrue((await PublicAccessService.AddRuleAsync(content1, "test2", "test2")).Success);

        var user = await UserService.GetAsync(Constants.Security.SuperUserKey);
        var userGroup = await UserGroupService.GetAsync(user.Groups.First().Alias);
        NotificationService.TryCreateNotification(user, content1, "X", out Notification? notification);
        Assert.IsNotNull(notification);

        await ContentService.SetPermissionAsync(content1, "A", new[] { userGroup.Key }, CancellationToken.None);
        var updateDomainResult = await DomainService.UpdateDomainsAsync(
            content1.Key,
            new DomainsUpdateModel
            {
                Domains = new[] { new DomainModel { DomainName = "www.test.com", IsoCode = "en-US" } }
            });
        Assert.IsTrue(updateDomainResult.Success);

        // Act
        ContentService.MoveToRecycleBin(content1);
        await ContentService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);
        var contents = (await ContentService.GetPagedContentInRecycleBinAsync(0, int.MaxValue, ordering: null, CancellationToken.None)).Items.ToList();

        // Assert
        Assert.That(contents.Any(), Is.False);
    }

    [Test]
    public async Task Can_Move_Content()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Trashed.Key, CancellationToken.None);

        // Act - moving out of recycle bin
        ContentService.Move(content, Textpage.Id);

        // Assert
        Assert.That(content.ParentId, Is.EqualTo(Textpage.Id));
        Assert.That(content.ParentKey, Is.EqualTo(Textpage.Key));
        Assert.That(content.Trashed, Is.False);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public async Task MoveToRecycleBin_Sets_RecycleBin_Sentinel_ParentKey()
    {
        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);

        ContentService.MoveToRecycleBin(content);

        Assert.That(content!.ParentId, Is.EqualTo(Constants.System.RecycleBinContent));
        Assert.That(content.ParentKey, Is.EqualTo(Constants.System.RecycleBinContentKey));
    }

    [Test]
    public async Task Move_With_Descendants_Populates_ParentKey_Without_Redundant_IIdKeyMap_Calls()
    {
        var idKeyMapSpy = (SpyIdKeyMap)IdKeyMap;

        var destination = ContentBuilder.CreateSimpleContent(ContentType, "Move Destination");
        await ContentService.SaveAsync(destination, -1, null, CancellationToken.None);

        IContent? textpage = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        var callCountBeforeMove = idKeyMapSpy.GetKeyForIdAsyncCallCount;

        ContentService.Move(textpage!, destination.Id);

        // Resolving the new parent from its int id is the one unavoidable lookup (Move's public
        // signature only takes an int parentId) - moving Textpage's two children must not add any
        // further IIdKeyMap calls on top of that, regardless of how many descendants are moved.
        Assert.That(idKeyMapSpy.GetKeyForIdAsyncCallCount, Is.EqualTo(callCountBeforeMove + 1));

        Assert.That(textpage!.ParentKey, Is.EqualTo(destination.Key));

        IContent? subpage = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        IContent? subpage2 = await ContentService.GetByIdAsync(Subpage2.Key, CancellationToken.None);
        Assert.That(subpage!.ParentKey, Is.EqualTo(textpage.Key));
        Assert.That(subpage2!.ParentKey, Is.EqualTo(textpage.Key));
    }

    [Test]
    public async Task Can_Copy_Content()
    {
        // Arrange
        var temp = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);

        // Act
        var copy = ContentService.Copy(temp, temp.ParentId, false);
        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
        Assert.That(copy.ParentKey, Is.EqualTo(Textpage.Key));
        Assert.AreNotSame(content, copy);
        foreach (var property in copy.Properties)
        {
            Assert.AreEqual(property.GetValue(), content.Properties[property.Alias].GetValue());
        }

        // Assert.AreNotEqual(content.Name, copy.Name);
    }

    [Test]
    public async Task Copy_Of_Trashed_Content_Is_Not_Trashed()
    {
        // Arrange
        ContentService.MoveToRecycleBin(Subpage);
        Assert.That(Subpage.Trashed, Is.True);

        // Act
        var copy = ContentService.Copy(Subpage, Textpage.Id, false);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.Trashed, Is.False);

        PagedModel<IContent> recycleBinContents = await ContentService.GetPagedContentInRecycleBinAsync(0, int.MaxValue, ordering: null, CancellationToken.None);
        Assert.That(recycleBinContents.Items.Any(c => c.Key == copy.Key), Is.False);
    }

    [Test]
    public async Task Copy_To_Root_Populates_Null_ParentKey_Not_The_Root_Nodes_Own_Key()
    {
        IContent? subpage = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);

        // umbracoNode's own Root row (id -1) carries Constants.System.RootSystemKey, NOT the semantic
        // "no parent" value ParentKey contracts to - Copy must not let it leak through.
        IContent? copy = ContentService.Copy(subpage!, Constants.System.Root, false);

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy!.ParentKey, Is.Null);
    }

    [Test]
    public async Task Copy_Recursive_Populates_Descendant_ParentKey_Without_Redundant_IIdKeyMap_Calls()
    {
        var idKeyMapSpy = (SpyIdKeyMap)IdKeyMap;

        var destination = ContentBuilder.CreateSimpleContent(ContentType, "Copy Destination");
        await ContentService.SaveAsync(destination, -1, null, CancellationToken.None);

        IContent? textpage = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        var callCountBeforeCopy = idKeyMapSpy.GetKeyForIdAsyncCallCount;

        IContent? copy = ContentService.Copy(textpage!, destination.Id, false, true);

        // Resolving the new parent from its int id is the one unavoidable lookup (Copy's public
        // signature only takes an int parentId) - copying Textpage's two descendants must not add any
        // further IIdKeyMap calls on top of that, regardless of how many descendants are copied.
        Assert.That(idKeyMapSpy.GetKeyForIdAsyncCallCount, Is.EqualTo(callCountBeforeCopy + 1));

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy!.ParentKey, Is.EqualTo(destination.Key));

        PagedModel<IContent> copiedDescendants = await ContentService.GetChildrenAsync(copy.Key, 0, 10, null, null, CancellationToken.None);
        Assert.That(copiedDescendants.Items, Is.Not.Empty);
        foreach (IContent copiedDescendant in copiedDescendants.Items)
        {
            Assert.That(copiedDescendant.ParentKey, Is.EqualTo(copy.Key));
        }
    }

    [Test]
    public async Task Copy_Recursive_Notification_Reports_Each_Copys_Own_New_Parent_Not_The_Roots()
    {
        var destination = ContentBuilder.CreateSimpleContent(ContentType, "Copy Notification Destination");
        await ContentService.SaveAsync(destination, -1, null, CancellationToken.None);

        IContent? textpage = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        var reportedParentKeysByCopyKey = new Dictionary<Guid, Guid?>();
        ContentNotificationHandler.CopiedContent = notification => reportedParentKeysByCopyKey[notification.Copy.Key] = notification.ParentKey;

        IContent? copy;
        try
        {
            copy = ContentService.Copy(textpage!, destination.Id, false, true);
        }
        finally
        {
            ContentNotificationHandler.CopiedContent = null;
        }

        Assert.That(copy, Is.Not.Null);

        // The root copy's own new parent is the destination...
        Assert.That(reportedParentKeysByCopyKey[copy!.Key], Is.EqualTo(destination.Key));

        // ...but each descendant copy's own new parent is the root copy, not the destination the root
        // copy moved into - descendants must not report the root copy's parent key as their own.
        PagedModel<IContent> copiedDescendants = await ContentService.GetChildrenAsync(copy.Key, 0, 10, null, null, CancellationToken.None);
        Assert.That(copiedDescendants.Items, Is.Not.Empty);
        foreach (IContent copiedDescendant in copiedDescendants.Items)
        {
            Assert.That(reportedParentKeysByCopyKey[copiedDescendant.Key], Is.EqualTo(copy.Key));
        }
    }

    /// <summary>
    /// Provides a regression test for https://github.com/umbraco/Umbraco-CMS/issues/22540.
    /// </summary>
    [Test]
    public async Task Can_Copy_Culture_Variant_Document_Without_Per_Culture_Published_Flags()
    {
        // Arrange
        var (content, langUk, langFr, _) = await CreateEnglishAndFrenchDocument();

        Assert.IsTrue((await ContentService.SaveAsync(content, null, null, CancellationToken.None)).Success);
        Assert.IsTrue(ContentService.Publish(content, [langFr.IsoCode, langUk.IsoCode]).Success);

        // re-get to ensure we copy from the persisted state
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        // Act
        var copy = ContentService.Copy(content, content.ParentId, false);

        // Assert against umbracoDocumentCultureVariation directly - IContent's re-materialisation
        // after GetById hides the DB-level inconsistency that issue #22540 is actually about.
        AssertCultureVariationRowsForUnpublishedCopy(copy!.Id, langUk, langFr, "content-en", "content-fr");
    }

    /// <summary>
    /// Provides a regression test for https://github.com/umbraco/Umbraco-CMS/issues/22540 (covers the
    /// recursive descendant path).
    /// </summary>
    [Test]
    public async Task Can_Copy_Recursive_Culture_Variant_Document_Without_Per_Culture_Published_Flags_On_Descendants()
    {
        // Arrange
        var (langUk, langFr, contentType) = await CreateEnglishAndFrenchDocumentType();

        IContent parent = new Content("parent", Constants.System.Root, contentType);
        parent.SetCultureName("parent-fr", langFr.IsoCode);
        parent.SetCultureName("parent-en", langUk.IsoCode);
        Assert.IsTrue((await ContentService.SaveAsync(parent, null, null, CancellationToken.None)).Success);
        Assert.IsTrue(ContentService.Publish(parent, [langFr.IsoCode, langUk.IsoCode]).Success);

        IContent child = new Content("child", parent.Id, contentType);
        child.SetCultureName("child-fr", langFr.IsoCode);
        child.SetCultureName("child-en", langUk.IsoCode);
        Assert.IsTrue((await ContentService.SaveAsync(child, null, null, CancellationToken.None)).Success);
        Assert.IsTrue(ContentService.Publish(child, [langFr.IsoCode, langUk.IsoCode]).Success);

        // re-get to ensure we copy from the persisted state
        parent = await ContentService.GetByIdAsync(parent.Key, CancellationToken.None);

        // Act: copy the branch (recursive)
        var copy = ContentService.Copy(parent, parent.ParentId, false, recursive: true);

        var childCopy = (await ContentService.GetChildrenAsync(copy!.Key, 0, 500, propertyAliases: null, ordering: null, CancellationToken.None))
            .Items
            .First();

        // Assert against umbracoDocumentCultureVariation directly for both the root copy and the descendant copy.
        AssertCultureVariationRowsForUnpublishedCopy(copy.Id, langUk, langFr, "parent-en", "parent-fr");
        AssertCultureVariationRowsForUnpublishedCopy(childCopy.Id, langUk, langFr, "child-en", "child-fr");
    }

    private void AssertCultureVariationRowsForUnpublishedCopy(
        int nodeId,
        Language langUk,
        Language langFr,
        string expectedUkNamePrefix,
        string expectedFrNamePrefix)
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var rows = scope.Database.Fetch<DocumentCultureVariationDto>(
            "WHERE nodeId = @0",
            nodeId);

        Assert.That(
            rows,
            Has.Count.EqualTo(2),
            $"Expected one umbracoDocumentCultureVariation row per culture for node {nodeId}.");

        var ukRow = rows.Single(r => r.LanguageId == langUk.Id);
        var frRow = rows.Single(r => r.LanguageId == langFr.Id);

        Assert.Multiple(() =>
        {
            // the actual bug: umbracoDocumentCultureVariation.published must be 0 on an unpublished copy
            Assert.IsFalse(ukRow.Published, $"en-GB row on node {nodeId} should have published=0.");
            Assert.IsFalse(frRow.Published, $"fr-FR row on node {nodeId} should have published=0.");

            // sanity: edit-side culture names are still preserved (Copy appends " (n)" to avoid sibling collisions)
            Assert.That(ukRow.Name, Does.StartWith(expectedUkNamePrefix));
            Assert.That(frRow.Name, Does.StartWith(expectedFrNamePrefix));
        });
    }

    [Test]
    public async Task Can_Copy_And_Modify_Content_With_Events()
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
            await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

            var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
            var content = ContentBuilder.CreateSimpleContent(contentType);
            content.SetValue("title", "New Value");
            await ContentService.SaveAsync(content, null, null, CancellationToken.None);

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
    public async Task Can_Copy_Recursive()
    {
        // Arrange
        var temp = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.AreEqual("Textpage", temp.Name);
        Assert.AreEqual(3, await ContentService.CountChildrenAsync(temp.Key, null, CancellationToken.None));

        // Act
        var copy = ContentService.Copy(temp, temp.ParentId, false, true);
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
        Assert.AreNotSame(content, copy);
        Assert.AreEqual(3, await ContentService.CountChildrenAsync(copy.Key, null, CancellationToken.None));

        var child = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        var childCopy = (await ContentService.GetChildrenAsync(copy.Key, 0, 500, propertyAliases: null, ordering: null, CancellationToken.None)).Items.First();
        Assert.AreEqual(childCopy.Name, child.Name);
        Assert.AreNotEqual(childCopy.Id, child.Id);
        Assert.AreNotEqual(childCopy.Key, child.Key);
    }

    [Test]
    public async Task Can_Copy_NonRecursive()
    {
        // Arrange
        var temp = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.AreEqual("Textpage", temp.Name);
        Assert.AreEqual(3, await ContentService.CountChildrenAsync(temp.Key, null, CancellationToken.None));

        // Act
        var copy = ContentService.Copy(temp, temp.ParentId, false, false);
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.Id, Is.Not.EqualTo(content.Id));
        Assert.AreNotSame(content, copy);
        Assert.AreEqual(0, await ContentService.CountChildrenAsync(copy.Key, null, CancellationToken.None));
    }

    [Test]
    public async Task Can_Copy_Content_With_Tags()
    {
        const string propAlias = "tags";

        // create a content type that has a 'tags' property
        // the property needs to support tags, else nothing works of course!
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var contentType =
            ContentTypeBuilder.CreateSimpleTagsContentType(
                "umbTagsPage",
                "TagsPage",
                defaultTemplateId: template.Id);
        contentType.Key = new Guid("78D96D30-1354-4A1E-8450-377764200C58");
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType, "Simple Tags Page");
        content.AssignTags(
            PropertyEditorCollection,
            DataTypeService,
            IdKeyMap,
            Serializer,
            propAlias,
            new[] { "hello", "world" });
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // value has been set but no tags have been created (not published)
        Assert.AreEqual("[\"hello\",\"world\"]", content.GetValue(propAlias));
        var contentTags = TagService.GetTagsForEntity(content.Id).ToArray();
        Assert.AreEqual(0, contentTags.Length);

        // reloading the content yields the same result
        content = (Content)await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
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
    public async Task Can_Rollback_Version_On_Content()
    {
        // Arrange
        var parent = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.IsFalse(parent.Published);
        await ContentService.SaveAsync(parent, null, null, CancellationToken.None);
        ContentService.Publish(parent, parent.AvailableCultures.ToArray()); // publishing parent, so Text Page 2 can be updated.

        var content = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        Assert.IsFalse(content.Published);

        var versions = (await ContentService.GetVersionsAsync(Subpage.Key, CancellationToken.None)).ToList();
        Assert.AreEqual(1, versions.Count);

        var version1 = content.VersionId;

        content.Name = "Text Page 2 Updated";
        content.SetValue("author", "Francis Doe");

        // non published = edited
        Assert.IsTrue(content.Edited);

        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        ContentService.Publish(content, content.AvailableCultures.ToArray()); // new version
        var version2 = content.VersionId;
        Assert.AreNotEqual(version1, version2);

        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);
        Assert.AreEqual(
            "Francis Doe",
            (await ContentService.GetByIdAsync(content.Key, CancellationToken.None)).GetValue<string>("author")); // version2 author is Francis

        Assert.AreEqual("Text Page 2 Updated", content.Name);
        Assert.AreEqual("Text Page 2 Updated", content.PublishName);

        content.Name = "Text Page 2 ReUpdated";
        content.SetValue("author", "Jane Doe");

        // is not actually 'edited' until changes have been saved
        Assert.IsFalse(content.Edited);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None); // just save changes
        Assert.IsTrue(content.Edited);

        Assert.AreEqual("Text Page 2 ReUpdated", content.Name);
        Assert.AreEqual("Text Page 2 Updated", content.PublishName);

        content.Name = "Text Page 2 ReReUpdated";

        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        ContentService.Publish(content, content.AvailableCultures.ToArray()); // new version
        var version3 = content.VersionId;
        Assert.AreNotEqual(version2, version3);

        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);
        Assert.AreEqual(
            "Jane Doe",
            (await ContentService.GetByIdAsync(content.Key, CancellationToken.None)).GetValue<string>("author")); // version3 author is Jane

        Assert.AreEqual("Text Page 2 ReReUpdated", content.Name);
        Assert.AreEqual("Text Page 2 ReReUpdated", content.PublishName);

        // here we have
        // version1, first published version
        // version2, second published version
        // version3, third and current published version

        // rollback all values to version1
        var rollback = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        var rollto = await ContentService.GetVersionAsync(version1, CancellationToken.None);
        rollback.CopyFrom(rollto);
        rollback.Name = rollto.Name; // must do it explicitly
        await ContentService.SaveAsync(rollback, null, null, CancellationToken.None);

        Assert.IsNotNull(rollback);
        Assert.IsTrue(rollback.Published);
        Assert.IsTrue(rollback.Edited);
        Assert.AreEqual(
            "Francis Doe",
            (await ContentService.GetByIdAsync(content.Key, CancellationToken.None)).GetValue<string>("author")); // author is now Francis again
        Assert.AreEqual(version3, rollback.VersionId); // same version but with edits

        // props and name have rolled back
        Assert.AreEqual("Francis Doe", rollback.GetValue<string>("author"));
        Assert.AreEqual("Text Page 2 Updated", rollback.Name);

        // published props and name are still there
        Assert.AreEqual("Jane Doe", rollback.GetValue<string>("author", published: true));
        Assert.AreEqual("Text Page 2 ReReUpdated", rollback.PublishName);

        // rollback all values to current version
        // special because... current has edits... this really is equivalent to rolling back to version2
        var rollback2 = await ContentService.GetByIdAsync(Subpage.Key, CancellationToken.None);
        var rollto2 = await ContentService.GetVersionAsync(version3, CancellationToken.None);
        rollback2.CopyFrom(rollto2);
        rollback2.Name = rollto2.PublishName; // must do it explicitely AND must pick the publish one!
        await ContentService.SaveAsync(rollback2, null, null, CancellationToken.None);

        Assert.IsTrue(rollback2.Published);
        Assert.IsTrue(rollback2.Edited); // Still edited, change of behaviour

        Assert.AreEqual("Jane Doe", rollback2.GetValue<string>("author"));
        Assert.AreEqual("Text Page 2 ReReUpdated", rollback2.Name);

        // test rollback to self, again
        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.AreEqual("Text Page 2 ReReUpdated", content.Name);
        Assert.AreEqual("Jane Doe", content.GetValue<string>("author"));
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        ContentService.Publish(content, content.AvailableCultures.ToArray());
        Assert.IsFalse(content.Edited);
        content.Name = "Xxx";
        content.SetValue("author", "Bob Doe");
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        Assert.IsTrue(content.Edited);
        rollto = await ContentService.GetVersionAsync(content.VersionId, CancellationToken.None);
        content.CopyFrom(rollto);
        content.Name = rollto.PublishName; // must do it explicitely AND must pick the publish one!
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
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
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("multi", "Multi", defaultTemplateId: template.Id);
        contentType.Key = new Guid("45FF9A70-9C5F-448D-A476-DCD23566BBF8");
        contentType.Variations = ContentVariation.Culture;
        var p1 = contentType.PropertyTypes.First();
        p1.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

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
        await ContentService.SaveAsync(page, null, null, CancellationToken.None);
        var versionId0 = page.VersionId;

        page.SetValue(p1.Alias, "v1fr", langFr.IsoCode);
        page.SetValue(p1.Alias, "v1da", langDa.IsoCode);
        Thread.Sleep(1);
        await ContentService.SaveAsync(page, null, null, CancellationToken.None);
        ContentService.Publish(page, page.AvailableCultures.ToArray());
        var versionId1 = page.VersionId;

        Thread.Sleep(10);

        page.SetCultureName("fr2", langFr.IsoCode);
        page.SetValue(p1.Alias, "v2fr", langFr.IsoCode);
        Thread.Sleep(1);
        await ContentService.SaveAsync(page, null, null, CancellationToken.None);
        ContentService.Publish(page, new[] { langFr.IsoCode });
        var versionId2 = page.VersionId;

        Thread.Sleep(10);

        page.SetCultureName("da2", langDa.IsoCode);
        page.SetValue(p1.Alias, "v2da", langDa.IsoCode);
        Thread.Sleep(1);
        await ContentService.SaveAsync(page, null, null, CancellationToken.None);
        ContentService.Publish(page, new[] { langDa.IsoCode });
        var versionId3 = page.VersionId;

        Thread.Sleep(10);

        page.SetCultureName("fr3", langFr.IsoCode);
        page.SetCultureName("da3", langDa.IsoCode);
        page.SetValue(p1.Alias, "v3fr", langFr.IsoCode);
        page.SetValue(p1.Alias, "v3da", langDa.IsoCode);
        Thread.Sleep(1);
        await ContentService.SaveAsync(page, null, null, CancellationToken.None);
        ContentService.Publish(page, page.AvailableCultures.ToArray());
        var versionId4 = page.VersionId;

        // now get all versions
        var versions = (await ContentService.GetVersionsAsync(page.Key, CancellationToken.None)).ToArray();

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
        await ContentService.SaveAsync(page, null, null, CancellationToken.None);
        var versionId5 = page.VersionId;

        versions = (await ContentService.GetVersionsAsync(page.Key, CancellationToken.None)).ToArray();

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

        var versionsSlim = (await ContentService.GetVersionsSlimAsync(page.Key, 0, 50, CancellationToken.None)).ToArray();
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
        await ContentService.SaveAsync(page, null, null, CancellationToken.None);

        // and voila, rolled back!
        Assert.AreEqual(versions[4].GetPublishName(langFr.IsoCode), page.GetCultureName(langFr.IsoCode));
        Assert.AreEqual(versions[4].GetValue(p1.Alias, langFr.IsoCode), page.GetValue(p1.Alias, langFr.IsoCode));

        // note that rolling back invariant values means we also rolled back... DA... at least partially
        // bah?
    }

    [Test]
    public async Task Can_Save_Lazy_Content()
    {
        var contentType = await ContentTypeService.GetAsync("umbTextpage");
        var root = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

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
    public async Task Can_Verify_Property_Types_On_Content()
    {
        // Arrange
        var contentTypeService = ContentTypeService;
        var contentType = ContentTypeBuilder.CreateAllTypesContentType("allDataTypes", "All DataTypes");
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        var content =
            ContentBuilder.CreateAllTypesContent(contentType, "Random Content", Constants.System.Root);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
        var id = content.Id;

        // Act
        var sut = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

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
        Assert.That(
            sut.GetValue<DateTime>("dateTime").ToString("G"),
            Is.EqualTo(content.GetValue<DateTime>("dateTime").ToString("G")));
        Assert.That(sut.GetValue<string>("colorPicker"), Is.EqualTo("black"));
        Assert.That(sut.GetValue<string>("ddlMultiple"), Is.EqualTo("1234,1235"));
        Assert.That(sut.GetValue<string>("rbList"), Is.EqualTo("random"));
        Assert.That(
            sut.GetValue<DateTime>("date").ToString("G"),
            Is.EqualTo(content.GetValue<DateTime>("date").ToString("G")));
        Assert.That(sut.GetValue<string>("ddl"), Is.EqualTo("1234"));
        Assert.That(sut.GetValue<string>("chklist"), Is.EqualTo("randomc"));
        Assert.That(
            sut.GetValue<Udi>("contentPicker"),
            Is.EqualTo(Udi.Create(Constants.UdiEntityType.Document, new Guid("74ECA1D4-934E-436A-A7C7-36CC16D4095C"))));
        Assert.That(
            sut.GetValue<Udi>("memberPicker"),
            Is.EqualTo(Udi.Create(Constants.UdiEntityType.Member, new Guid("9A50A448-59C0-4D42-8F93-4F1D55B0F47D"))));
        Assert.That(
            sut.GetValue<string>("multiUrlPicker"),
            Is.EqualTo("[{\"name\":\"https://test.com\",\"url\":\"https://test.com\"}]"));
        Assert.That(sut.GetValue<string>("tags"), Is.EqualTo("this,is,tags"));
        Assert.That(
            sut.GetValue<string>("dateTimeWithTimeZone"),
            Is.EqualTo("{\"date\":\"2025-01-22T18:33:01.0000000+01:00\",\"timeZone\":\"Europe/Copenhagen\"}"));
    }

    [Test]
    [LongRunning]
    public async Task Can_Delete_Previous_Versions_Not_Latest()
    {
        // Arrange
        var content = await ContentService.GetByIdAsync(Trashed.Key, CancellationToken.None);
        var version = content.VersionId;

        // Act
        await ContentService.DeleteVersionAsync(Trashed.Key, version, true, Constants.Security.SuperUserKey, CancellationToken.None);
        var sut = await ContentService.GetByIdAsync(Trashed.Key, CancellationToken.None);

        // Assert
        Assert.That(sut.VersionId, Is.EqualTo(version));
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_Paged_Children()
    {
        // Start by cleaning the "db"
        var umbTextPage = await ContentService.GetByIdAsync(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"), CancellationToken.None);
        await ContentService.DeleteAsync(umbTextPage, null, CancellationToken.None);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        for (var i = 0; i < 10; i++)
        {
            var c1 = ContentBuilder.CreateSimpleContent(contentType);
            await ContentService.SaveAsync(c1, null, null, CancellationToken.None);
        }

        PagedModel<IContent> page = await ContentService.GetChildrenAsync(null, 0, 6, propertyAliases: null, ordering: null, CancellationToken.None);
        var entities = page.Items.ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(page.Total, Is.EqualTo(10));
        page = await ContentService.GetChildrenAsync(null, 6, 6, propertyAliases: null, ordering: null, CancellationToken.None);
        entities = page.Items.ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(page.Total, Is.EqualTo(10));
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_Paged_Children_Dont_Get_Descendants()
    {
        // Start by cleaning the "db"
        var umbTextPage = await ContentService.GetByIdAsync(new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"), CancellationToken.None);
        await ContentService.DeleteAsync(umbTextPage, null, CancellationToken.None);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Only add 9 as we also add a content with children
        for (var i = 0; i < 9; i++)
        {
            var c1 = ContentBuilder.CreateSimpleContent(contentType);
            await ContentService.SaveAsync(c1, null, null, CancellationToken.None);
        }

        var willHaveChildren = ContentBuilder.CreateSimpleContent(contentType);
        await ContentService.SaveAsync(willHaveChildren, null, null, CancellationToken.None);
        for (var i = 0; i < 10; i++)
        {
            var c1 = ContentBuilder.CreateSimpleContent(contentType, "Content" + i, willHaveChildren.Id);
            await ContentService.SaveAsync(c1, null, null, CancellationToken.None);
        }

        // children in root including the folder - not the descendants in the folder
        PagedModel<IContent> page = await ContentService.GetChildrenAsync(null, 0, 6, propertyAliases: null, ordering: null, CancellationToken.None);
        var entities = page.Items.ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(page.Total, Is.EqualTo(10));
        page = await ContentService.GetChildrenAsync(null, 6, 6, propertyAliases: null, ordering: null, CancellationToken.None);
        entities = page.Items.ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(page.Total, Is.EqualTo(10));

        // children in folder
        page = await ContentService.GetChildrenAsync(willHaveChildren.Key, 0, 6, propertyAliases: null, ordering: null, CancellationToken.None);
        entities = page.Items.ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(page.Total, Is.EqualTo(10));
        page = await ContentService.GetChildrenAsync(willHaveChildren.Key, 6, 6, propertyAliases: null, ordering: null, CancellationToken.None);
        entities = page.Items.ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(page.Total, Is.EqualTo(10));
    }

    [Test]
    public async Task GetPagedChildren_With_Null_PropertyAliases_Returns_All_Properties()
    {
        // Arrange
        var parentKey = await CreateContentWithChildForGetPagedChildrenParameterTests();

        // Act - null propertyAliases should load all properties
        var retrievedChild = await GetSingleChildWithPropertyAliases(parentKey, propertyAliases: null);

        // Assert - All properties should have their values loaded
        Assert.That(retrievedChild.Properties["title"]?.GetValue(), Is.Not.Null);
        Assert.That(retrievedChild.Properties["bodyText"]?.GetValue(), Is.Not.Null);
        Assert.That(retrievedChild.Properties["author"]?.GetValue(), Is.Not.Null);
    }

    [Test]
    public async Task GetPagedChildren_With_Empty_PropertyAliases_Returns_No_Property_Values()
    {
        // Arrange
        var parentKey = await CreateContentWithChildForGetPagedChildrenParameterTests();

        // Act - empty propertyAliases should load no custom properties
        var retrievedChild = await GetSingleChildWithPropertyAliases(parentKey, propertyAliases: []);

        // Assert - Properties should not be present when propertyAliases is empty
        Assert.That(retrievedChild.Properties.Contains("title"), Is.False, "title property should not be present");
        Assert.That(retrievedChild.Properties.Contains("bodyText"), Is.False, "bodyText property should not be present");
        Assert.That(retrievedChild.Properties.Contains("author"), Is.False, "author property should not be present");
    }

    [Test]
    public async Task GetPagedChildren_With_Single_PropertyAlias_Returns_Only_That_Property()
    {
        // Arrange
        var parentKey = await CreateContentWithChildForGetPagedChildrenParameterTests();

        // Act - only "title" should be loaded
        var retrievedChild = await GetSingleChildWithPropertyAliases(parentKey, propertyAliases: ["title"]);

        // Assert - Only "title" property should have its value loaded
        Assert.That(retrievedChild.Properties["title"]?.GetValue(), Is.Not.Null);
        Assert.That(retrievedChild.Properties["bodyText"]?.GetValue(), Is.Null);
        Assert.That(retrievedChild.Properties["author"]?.GetValue(), Is.Null);
    }

    [Test]
    public async Task GetPagedChildren_With_Multiple_PropertyAliases_Returns_Only_Those_Properties()
    {
        // Arrange
        var parentKey = await CreateContentWithChildForGetPagedChildrenParameterTests();

        // Act - "title" and "author" should be loaded, but not "bodyText"
        var retrievedChild = await GetSingleChildWithPropertyAliases(parentKey, propertyAliases: ["title", "author"]);

        // Assert - Only "title" and "author" properties should have values loaded
        Assert.That(retrievedChild.Properties["title"]?.GetValue(), Is.Not.Null);
        Assert.That(retrievedChild.Properties["author"]?.GetValue(), Is.Not.Null);
        Assert.That(retrievedChild.Properties["bodyText"]?.GetValue(), Is.Null);
    }

    [Test]
    public async Task GetPagedChildren_With_NonExistent_PropertyAlias_Returns_No_Properties()
    {
        // Arrange
        var parentKey = await CreateContentWithChildForGetPagedChildrenParameterTests();

        // Act - non-existent property alias should result in no property values
        var retrievedChild = await GetSingleChildWithPropertyAliases(parentKey, propertyAliases: ["nonExistentProperty"]);

        // Assert - No property values should be loaded since the alias doesn't exist
        Assert.That(retrievedChild.Properties["title"]?.GetValue(), Is.Null);
        Assert.That(retrievedChild.Properties["bodyText"]?.GetValue(), Is.Null);
        Assert.That(retrievedChild.Properties["author"]?.GetValue(), Is.Null);
        Assert.That(retrievedChild.Properties.Contains("nonExistentProperty"), Is.False);
    }

    [Test]
    public async Task GetPagedChildren_With_LoadTemplates_True_Loads_Template()
    {
        // Arrange
        var parentKey = await CreateContentWithChildForGetPagedChildrenParameterTests();

        // Act - loadTemplates: true (default) should load templates
        var retrievedChild = await GetSingleChildWithLoadTemplates(parentKey, loadTemplates: true);

        // Assert - Template should be loaded
        Assert.That(retrievedChild.TemplateId, Is.Not.Null);
    }

    [Test]
    public async Task GetPagedChildren_With_LoadTemplates_False_Does_Not_Load_Template()
    {
        // Arrange
        var parentKey = await CreateContentWithChildForGetPagedChildrenParameterTests();

        // Act - loadTemplates: false should not load templates
        var retrievedChild = await GetSingleChildWithLoadTemplates(parentKey, loadTemplates: false);

        // Assert - Template should not be loaded
        Assert.That(retrievedChild.TemplateId, Is.Null);
    }

    [Test]
    public async Task GetPagedChildren_Default_LoadTemplates_Loads_Template()
    {
        // Arrange
        var parentKey = await CreateContentWithChildForGetPagedChildrenParameterTests();

        // Act - default (no loadTemplates specified) should load templates (backwards compatible)
        var children = (await ContentService.GetChildrenAsync(parentKey, 0, 10, propertyAliases: null, ordering: null, CancellationToken.None)).Items.ToArray();

        Assert.That(children.Length, Is.EqualTo(1));

        // Assert - Template should be loaded by default
        Assert.That(children[0].TemplateId, Is.Not.Null);
    }

    /// <summary>
    /// Creates a content type with properties (title, bodyText, author) and a parent with one child.
    /// Returns the parent key for use in GetPagedChildren tests.
    /// </summary>
    private async Task<Guid> CreateContentWithChildForGetPagedChildrenParameterTests()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        await ContentService.SaveAsync(parent, null, null, CancellationToken.None);

        var child = ContentBuilder.CreateSimpleContent(contentType, "Child", parent.Id);
        await ContentService.SaveAsync(child, null, null, CancellationToken.None);

        return parent.Key;
    }

    /// <summary>
    /// Gets the single child of the parent using GetChildrenAsync with the specified propertyAliases.
    /// Asserts that exactly one child is returned.
    /// </summary>
    private async Task<IContent> GetSingleChildWithPropertyAliases(Guid parentKey, string[]? propertyAliases)
    {
        PagedModel<IContent> page = await ContentService.GetChildrenAsync(parentKey, 0, 10, propertyAliases, ordering: null, CancellationToken.None);
        var children = page.Items.ToArray();

        Assert.That(children.Length, Is.EqualTo(1));
        Assert.That(page.Total, Is.EqualTo(1));

        return children[0];
    }

    /// <summary>
    /// Gets the single child of the parent using GetChildrenAsync/GetChildrenWithoutTemplatesAsync with the specified loadTemplates parameter.
    /// Asserts that exactly one child is returned.
    /// </summary>
    private async Task<IContent> GetSingleChildWithLoadTemplates(Guid parentKey, bool loadTemplates)
    {
        PagedModel<IContent> page = loadTemplates
            ? await ContentService.GetChildrenAsync(parentKey, 0, 10, propertyAliases: null, ordering: null, CancellationToken.None)
            : await ContentService.GetChildrenWithoutTemplatesAsync(parentKey, 0, 10, propertyAliases: null, ordering: null, CancellationToken.None);
        var children = page.Items.ToArray();

        Assert.That(children.Length, Is.EqualTo(1));
        Assert.That(page.Total, Is.EqualTo(1));

        return children[0];
    }

    [Test]
    public async Task PublishingTest()
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
        await TemplateService.CreateAsync(contentType.DefaultTemplate, Constants.Security.SuperUserKey); // else, FK violation on contentType!
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = await ContentService.CreateAsync("foo", (Guid?)null, "foo", Constants.Security.SuperUserKey, CancellationToken.None);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        content.SetValue("title", "foo");
        Assert.IsTrue(content.Edited);

        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.IsFalse(content.Published);
        Assert.IsTrue(content.Edited);

        var versions = (await ContentService.GetVersionsAsync(content.Key, CancellationToken.None)).ToList();
        Assert.AreEqual(1, versions.Count());

        // publish content
        // becomes Published, !Edited
        // creates a new version
        // can get published property values
        ContentService.Publish(content, new []{ "*" });

        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.Edited);

        versions = (await ContentService.GetVersionsAsync(content.Key, CancellationToken.None)).ToList();
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

        content = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);
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

        versions = (await ContentService.GetVersionsAsync(content.Key, CancellationToken.None)).ToList();
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
        await ContentService.SaveAsync(content, null, null, CancellationToken.None); // this is effectively a rollback?
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

        var contentType = await ContentTypeService.GetAsync("umbTextpage");
        contentType.Variations = ContentVariation.Culture;
        contentType.AddPropertyType(new PropertyType(
            ShortStringHelper,
            Constants.PropertyEditors.Aliases.TextBox,
            ValueStorageType.Nvarchar,
            "prop")
        { Variations = ContentVariation.Culture });
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = new Content(string.Empty, Constants.System.Root, contentType);

        content.SetCultureName("name-us", langUk.IsoCode);
        content.SetCultureName("name-fr", langFr.IsoCode);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

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

        var contentType = await ContentTypeService.GetAsync("umbTextpage");
        contentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = new Content(string.Empty, Constants.System.Root, contentType);
        content.SetCultureName("root", langUk.IsoCode);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        for (var i = 0; i < 5; i++)
        {
            var child = new Content(string.Empty, content, contentType);
            child.SetCultureName("child", langUk.IsoCode);
            await ContentService.SaveAsync(child, null, null, CancellationToken.None);

            Assert.AreEqual(
                "child" + (i == 0 ? string.Empty : " (" + i + ")"),
                child.GetCultureName(langUk.IsoCode));

            // Save it again to ensure that the unique check is not performed again against it's own name
            await ContentService.SaveAsync(child, null, null, CancellationToken.None);
            Assert.AreEqual(
                "child" + (i == 0 ? string.Empty : " (" + i + ")"),
                child.GetCultureName(langUk.IsoCode));
        }
    }

    [Test]
    public async Task Ensure_Invariant_Unique_Name_When_Url_Segments_Collide()
    {
        // Siblings whose names differ only in punctuation produce the same URL segment
        // (e.g. "Title" and "Title." both produce "title"), so the second should get a suffix.
        var contentType = (await ContentTypeService.GetAsync("umbTextpage"))!;

        var parent = new Content("root", Constants.System.Root, contentType);
        await ContentService.SaveAsync(parent, null, null, CancellationToken.None);

        var child1 = new Content("Title", parent, contentType);
        await ContentService.SaveAsync(child1, null, null, CancellationToken.None);
        Assert.AreEqual("Title", child1.Name);

        var child2 = new Content("Title.", parent, contentType);
        await ContentService.SaveAsync(child2, null, null, CancellationToken.None);
        Assert.AreEqual("Title. (1)", child2.Name);

        // Save again to verify the name is stable (idempotent).
        await ContentService.SaveAsync(child2, null, null, CancellationToken.None);
        Assert.AreEqual("Title. (1)", child2.Name);
    }

    [Test]
    public async Task Ensure_Unique_Culture_Names_When_Url_Segments_Collide()
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

        var contentType = (await ContentTypeService.GetAsync("umbTextpage"))!;
        contentType.Variations = ContentVariation.Culture;
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = new Content(string.Empty, Constants.System.Root, contentType);
        parent.SetCultureName("root", langUk.IsoCode);
        await ContentService.SaveAsync(parent, null, null, CancellationToken.None);

        var child1 = new Content(string.Empty, parent, contentType);
        child1.SetCultureName("Title", langUk.IsoCode);
        await ContentService.SaveAsync(child1, null, null, CancellationToken.None);
        Assert.AreEqual("Title", child1.GetCultureName(langUk.IsoCode));

        var child2 = new Content(string.Empty, parent, contentType);
        child2.SetCultureName("Title.", langUk.IsoCode);
        await ContentService.SaveAsync(child2, null, null, CancellationToken.None);
        Assert.AreEqual("Title. (1)", child2.GetCultureName(langUk.IsoCode));

        // Save again to verify the name is stable (idempotent).
        await ContentService.SaveAsync(child2, null, null, CancellationToken.None);
        Assert.AreEqual("Title. (1)", child2.GetCultureName(langUk.IsoCode));
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

        var contentType = await ContentTypeService.GetAsync("umbTextpage");
        contentType.Variations = ContentVariation.Culture;
        contentType.AddPropertyType(new PropertyType(
            ShortStringHelper,
            Constants.PropertyEditors.Aliases.TextBox,
            ValueStorageType.Nvarchar,
            "prop")
        { Variations = ContentVariation.Culture });

        // TODO: add test w/ an invariant prop
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = await ContentService.CreateAsync("Home US", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);

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
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // content has been saved,
        // it has names, but no publishNames, and no published cultures
        var content2 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

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
        AssertPerCulture(
            content,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));

        // nothing has been published yet
        AssertPerCulture(
            content,
            (x, c) => x.IsCulturePublished(c),
            (langFr, false),
            (langUk, false),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCulturePublished(c),
            (langFr, false),
            (langUk, false),
            (langDe, false));

        // not published => must be edited, if available
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

        // Act
        ContentService.Publish(content, new[] { langFr.IsoCode, langUk.IsoCode });

        // both FR and UK have been published,
        // and content has been published,
        // it has names, publishNames, and published cultures
        content2 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

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
        AssertPerCulture(
            content,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));

        // fr and uk have been published now
        AssertPerCulture(
            content,
            (x, c) => x.IsCulturePublished(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCulturePublished(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));

        // fr and uk, published without changes, not edited
        AssertPerCulture(
            content,
            (x, c) => x.IsCultureEdited(c),
            (langFr, false),
            (langUk, false),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCultureEdited(c),
            (langFr, false),
            (langUk, false),
            (langDe, false));

        AssertPerCulture(
            content,
            (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langFr, false),
            (langUk, false)); // DE would throw
        AssertPerCulture(
            content2,
            (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langFr, false),
            (langUk, false)); // DE would throw

        // note that content and content2 culture published dates might be slightly different due to roundtrip to database

        // Act
        ContentService.Publish(content, new []{ "*" });

        // now it has publish name for invariant neutral
        content2 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        Assert.AreEqual("name-fr", content2.PublishName);

        content.SetCultureName("Home US2", null);
        content.SetCultureName("name-fr2", langFr.IsoCode);
        content.SetCultureName("name-uk2", langUk.IsoCode);
        content.SetValue("author", "Barack Obama2");
        content.SetValue("prop", "value-fr2", langFr.IsoCode);
        content.SetValue("prop", "value-uk2", langUk.IsoCode);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // content has been saved,
        // it has updated names, unchanged publishNames, and published cultures
        content2 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

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
        AssertPerCulture(
            content,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));

        // no change
        AssertPerCulture(
            content,
            (x, c) => x.IsCulturePublished(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCulturePublished(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));

        // we have changed values so now fr and uk are edited
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

        AssertPerCulture(
            content,
            (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langFr, false),
            (langUk, false)); // DE would throw
        AssertPerCulture(
            content2,
            (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langFr, false),
            (langUk, false)); // DE would throw

        // Act
        // cannot just 'save' since we are changing what's published!
        ContentService.Unpublish(content, langFr.IsoCode);

        // content has been published,
        // the french culture is gone
        // (only if french is not mandatory, else everything would be gone!)
        content2 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

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
        AssertPerCulture(
            content,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));

        // fr is not published anymore
        AssertPerCulture(
            content,
            (x, c) => x.IsCulturePublished(c),
            (langFr, false),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCulturePublished(c),
            (langFr, false),
            (langUk, true),
            (langDe, false));

        // and so, fr has to be edited
        AssertPerCulture(content, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));
        AssertPerCulture(content2, (x, c) => x.IsCultureEdited(c), (langFr, true), (langUk, true), (langDe, false));

        AssertPerCulture(
            content,
            (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langUk, false)); // FR, DE would throw
        AssertPerCulture(
            content2,
            (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
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
        content2 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

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
        Assert.AreEqual(
            "value-uk1",
            content2.GetValue("prop", langUk.IsoCode, published: true)); // has value, see note above

        // no change
        AssertPerCulture(
            content,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
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

        content2 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        // no change
        AssertPerCulture(
            content,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCultureAvailable(c),
            (langFr, true),
            (langUk, true),
            (langDe, false));

        // no change
        AssertPerCulture(
            content,
            (x, c) => x.IsCulturePublished(c),
            (langFr, false),
            (langUk, true),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCulturePublished(c),
            (langFr, false),
            (langUk, true),
            (langDe, false));

        // now, uk is no more edited
        AssertPerCulture(
            content,
            (x, c) => x.IsCultureEdited(c),
            (langFr, true),
            (langUk, false),
            (langDe, false));
        AssertPerCulture(
            content2,
            (x, c) => x.IsCultureEdited(c),
            (langFr, true),
            (langUk, false),
            (langDe, false));

        AssertPerCulture(
            content,
            (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langUk, false)); // FR, DE would throw
        AssertPerCulture(
            content2,
            (x, c) => x.GetPublishDate(c) == DateTime.MinValue,
            (langUk, false)); // FR, DE would throw

        // Act
        content.SetCultureName("name-uk3", langUk.IsoCode);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        content2 = await ContentService.GetByIdAsync(content.Key, CancellationToken.None);

        // note that the 'edited' flags only change once saved - not immediately
        // but they change, on what's being saved, and when getting it back

        // changing the name = edited!
        Assert.IsTrue(content.IsCultureEdited(langUk.IsoCode));
        Assert.IsTrue(content2.IsCultureEdited(langUk.IsoCode));
    }

    [Test]
    public async Task Cannot_Publish_Newly_Created_Unsaved_Content()
    {
        var content = await ContentService.CreateAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        var publishResult = ContentService.Publish(content, new[] { "*" });
        Assert.AreEqual(PublishResultType.FailedPublishUnsavedChanges, publishResult.Result);
    }

    [Test]
    public async Task Cannot_Publish_Unsaved_Content()
    {
        var content = await ContentService.CreateAsync("Test", (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None);
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);
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
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // reset any state and attempt a publish
        content = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;
        var result = ContentService.Publish(content, new[] { "*" });

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PublishResultType.FailedPublishContentInvalid, result.Result);

        // verify saved state
        content = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;
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
        await ContentService.SaveAsync(content, null, null, CancellationToken.None);

        // reset any state and attempt a publish
        content = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;
        var result = ContentService.Publish(content, new[] { langEn.IsoCode });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(PublishResultType.SuccessPublishCulture, result.Result);

        // verify saved state
        content = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
        Assert.AreEqual(langEn.IsoCode, content.PublishedCultures.First());
    }

    private void AssertPerCulture<T>(
        IContent item,
        Func<IContent, string, T> getter,
        params (ILanguage Language, bool Result)[] testCases)
    {
        foreach (var testCase in testCases)
        {
            var value = getter(item, testCase.Language.IsoCode);
            Assert.AreEqual(
                testCase.Result,
                value,
                $"Expected {testCase.Result} and got {value} for culture {testCase.Language.IsoCode}.");
        }
    }

    private async Task<IEnumerable<IContent>> CreateContentHierarchy()
    {
        var contentType = await ContentTypeService.GetAsync("umbTextpage");
        var root = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);

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
            var c = ContentBuilder.CreateSimpleContent(
                contentType,
                "Hierarchy Simple Text Subpage " + i,
                content);
            list.Add(c);

            Debug.Print("Created: 'Hierarchy Simple Text Subpage {0}' - Depth: {1}", i, depth);
        }

        return list;
    }

    private async Task<(Language LangUk, Language LangFr, ContentType ContentType)> CreateEnglishAndFrenchDocumentType()
    {
        var langUk = (Language)new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .WithIsDefault(true)
            .Build();
        var langFr = (Language)new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        await LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langUk, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return (langUk, langFr, contentType);
    }

    private async Task<(IContent Content, Language LangUk, Language LangFr, ContentType ContentType)> CreateEnglishAndFrenchDocument()
    {
        var (langUk, langFr, contentType) = await CreateEnglishAndFrenchDocumentType();

        IContent content = new Content("content", Constants.System.Root, contentType);
        content.SetCultureName("content-fr", langFr.IsoCode);
        content.SetCultureName("content-en", langUk.IsoCode);

        return (content, langUk, langFr, contentType);
    }

    public class ContentNotificationHandler :
        INotificationHandler<ContentCopyingNotification>,
        INotificationHandler<ContentCopiedNotification>,
        INotificationHandler<ContentPublishingNotification>,
        INotificationHandler<ContentSavingNotification>
    {
        public static Action<ContentPublishingNotification>? PublishingContent { get; set; }

        public static Action<ContentCopyingNotification>? CopyingContent { get; set; }

        public static Action<ContentCopiedNotification>? CopiedContent { get; set; }

        public static Action<ContentSavingNotification>? SavingContent { get; set; }

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
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

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
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return (langEn, langDa, contentType);
    }

    [Test]
    public async Task SortChildren_Persists_The_Supplied_Order()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType("sortChildrenPage", "Sort Children Page");
        contentType.AllowedAsRoot = true;
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var root = new Content("Root", Constants.System.Root, contentType);
        await ContentService.SaveAsync(root, null, null, CancellationToken.None);

        var childIds = new List<int>();
        var childKeys = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            var child = new Content($"Child {i}", root.Id, contentType);
            await ContentService.SaveAsync(child, null, null, CancellationToken.None);
            childIds.Add(child.Id);
            childKeys.Add(child.Key);
        }

        async Task<int[]> ChildIdsInSortOrder() => (await ContentService.GetChildrenAsync(root.Key, 0, 100, propertyAliases: null, ordering: null, CancellationToken.None))
            .Items
            .OrderBy(child => child.SortOrder)
            .Select(child => child.Id)
            .ToArray();

        // Children were created in ascending sort order.
        Assert.AreEqual(childIds.ToArray(), await ChildIdsInSortOrder());

        var reversedKeys = Enumerable.Reverse(childKeys).ToArray();
        var result = await ContentService.SortChildrenAsync(root.Key, reversedKeys, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(Enumerable.Reverse(childIds).ToArray(), await ChildIdsInSortOrder());
    }

    [Test]
    public async Task Cannot_Change_Key_Of_Persisted_Content()
    {
        // Arrange - get a persisted content item
        var content = await ContentService.GetByIdAsync(Textpage.Key, CancellationToken.None);
        Assert.That(content, Is.Not.Null);

        var originalKey = content!.Key;
        var newKey = Guid.NewGuid();

        // Act & Assert - attempting to change the Key should throw
        var exception = Assert.Throws<InvalidOperationException>(() => content.Key = newKey);
        Assert.That(exception!.Message, Does.Contain("Cannot change the Key"));
        Assert.That(exception.Message, Does.Contain("Content"));

        // Verify the Key was not changed
        Assert.That(content.Key, Is.EqualTo(originalKey));
    }
}
