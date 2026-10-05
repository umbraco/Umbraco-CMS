// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// URL alias rows are maintained inside the transaction that persists the document. These tests drive every
/// content operation through <see cref="IContentService"/> and assert one invariant after each: the alias table
/// equals what <see cref="IDocumentUrlAliasService.RebuildAllAliasesAsync"/> would produce from the content.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
internal sealed class DocumentUrlAliasPersistenceTests : UmbracoIntegrationTest
{
    private const string EnglishIsoCode = "en-US";
    private const string FrenchIsoCode = "fr-FR";

    private IDocumentUrlAliasService AliasService => GetRequiredService<IDocumentUrlAliasService>();

    private IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    private IDocumentUrlAliasRepository AliasRepository => GetRequiredService<IDocumentUrlAliasRepository>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private LockRecorder Locks => GetRequiredService<LockRecorder>();

    private FailableAliasRepository FailableRepository => GetRequiredService<FailableAliasRepository>();

    private ContentType ContentType { get; set; } = null!;

    private Content RootPage { get; set; } = null!;

    private int TemplateId { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlServiceInitializerNotificationHandler>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlAliasServiceInitializerNotificationHandler>();

        builder.Services.AddSingleton<LockRecorder>();
        builder.Services.AddUnique<IDistributedLockingMechanismFactory>(sp => new RecordingLockingMechanismFactory(
            ActivatorUtilities.CreateInstance<DefaultDistributedLockingMechanismFactory>(sp),
            sp.GetRequiredService<LockRecorder>()));

        builder.Services.AddSingleton(sp => new FailableAliasRepository(ActivatorUtilities.CreateInstance<DocumentUrlAliasRepository>(sp)));
        builder.Services.AddUnique<IDocumentUrlAliasRepository>(sp => sp.GetRequiredService<FailableAliasRepository>());
    }

    [SetUp]
    public async Task SetUpTestData()
    {
        await DocumentUrlService.InitAsync(false, CancellationToken.None);
        await AliasService.InitAsync(false, CancellationToken.None);

        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        TemplateId = template.Id;

        ContentType = CreateContentTypeWithUrlAlias("pageWithAlias", ContentVariation.Nothing);
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);

        RootPage = ContentBuilder.CreateSimpleContent(ContentType, "Root");
        ContentService.Save(RootPage, -1);
        Assert.That(ContentService.Publish(RootPage, ["*"]).Success, Is.True);

        Locks.Clear();
        FailableRepository.FailWrites = false;
    }

    [Test]
    public void Publish_WritesTheAliasRows_WithoutTakingTheDocumentUrlAliasesLock()
    {
        Content page = CreatePage("Page", "my-alias");
        Locks.Clear();

        PublishResult result = ContentService.Publish(page, ["*"]);

        Assert.That(result.Success, Is.True, result.Result.ToString());
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "my-alias" }));
        Assert.That(
            Locks.Obtained.Any(x => x.LockId == Constants.Locks.ContentTree && x.Type == DistributedLockType.WriteLock),
            Is.True,
            "The publish must have taken the content tree write lock, otherwise the recorder saw nothing and this test proves nothing.");
        Assert.That(
            Locks.Obtained.Where(x => x.LockId == Constants.Locks.DocumentUrlAliases),
            Is.Empty,
            "A publish must write its alias rows under the content tree lock it already holds, not under the global DocumentUrlAliases lock.");
        AssertAliasTableMatchesRebuildQuery("after publish");
    }

    [Test]
    public void DraftSave_TakesNoAliasLock_AndLeavesTheRowsAlone()
    {
        Content page = CreatePage("Page", "my-alias");
        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);

        IContent draft = ContentService.GetById(page.Id)!;
        draft.SetValue(Constants.Conventions.Content.UrlAlias, "draft-alias");
        Locks.Clear();
        ContentService.Save(draft, -1);

        Assert.That(Locks.Obtained.Where(x => x.LockId == Constants.Locks.DocumentUrlAliases), Is.Empty);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "my-alias" }), "A draft edit must not change the published alias rows.");
        AssertAliasTableMatchesRebuildQuery("after draft save");
    }

    [Test]
    public void RepublishWithChangedAlias_ReplacesTheRows_AndClearingItRemovesThem()
    {
        Content page = CreatePage("Page", "first, /Second/, first");
        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "first", "second" }));
        AssertAliasTableMatchesRebuildQuery("after first publish");

        IContent edit = ContentService.GetById(page.Id)!;
        edit.SetValue(Constants.Conventions.Content.UrlAlias, "second, third");
        ContentService.Save(edit, -1);
        Assert.That(ContentService.Publish(edit, ["*"]).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "second", "third" }));
        AssertAliasTableMatchesRebuildQuery("after republish with a changed alias");

        edit = ContentService.GetById(page.Id)!;
        edit.SetValue(Constants.Conventions.Content.UrlAlias, string.Empty);
        ContentService.Save(edit, -1);
        Assert.That(ContentService.Publish(edit, ["*"]).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.Empty);
        AssertAliasTableMatchesRebuildQuery("after republish with the alias cleared");
    }

    [Test]
    public void Unpublish_RemovesTheRows_AndPublishingAgainRestoresThem()
    {
        Content page = CreatePage("Page", "my-alias");
        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);

        Assert.That(ContentService.Unpublish(page).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.Empty);
        AssertAliasTableMatchesRebuildQuery("after unpublish");

        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "my-alias" }));
        AssertAliasTableMatchesRebuildQuery("after publishing again");
    }

    [Test]
    public void MoveToRecycleBin_RemovesTheRows_AndARestoredDocumentGetsThemBackWhenPublishedAgain()
    {
        Content page = CreatePage("Page", "my-alias");
        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);

        Assert.That(ContentService.MoveToRecycleBin(page).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.Empty, "A trashed document is not routable, so its alias rows must go.");
        AssertAliasTableMatchesRebuildQuery("after trash");

        // Restoring a published document from the bin unpublishes it (see ContentService.Move), so it has no
        // aliases until it is published again.
        Assert.That(ContentService.Move(page, RootPage.Id).Success, Is.True);
        Assert.That(ContentService.GetById(page.Id)!.Published, Is.False);
        Assert.That(AliasesFor(page.Key), Is.Empty);
        AssertAliasTableMatchesRebuildQuery("after restore");

        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "my-alias" }));
        AssertAliasTableMatchesRebuildQuery("after publishing the restored document");
    }

    [Test]
    public async Task RebuildAllAliasesAsync_DoesNotResurrectTheAliasesOfAnUnpublishedDocument()
    {
        Content page = CreatePage("Page", "my-alias");
        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);
        Assert.That(ContentService.Unpublish(page).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.Empty);

        // Unpublishing clears the document's published flag but leaves its last published version flagged, so the
        // rebuild has to look at the document, not only at the version.
        await AliasService.RebuildAllAliasesAsync();

        Assert.That(AliasesFor(page.Key), Is.Empty);
        Assert.That(await AliasService.GetDocumentKeysByAliasAsync("my-alias", null), Is.Empty);
    }

    [Test]
    public void Move_Sort_Rollback_AndCopy_DoNotChangeTheRows()
    {
        Content otherParent = CreatePage("Other parent", null);
        Assert.That(ContentService.Publish(otherParent, ["*"]).Success, Is.True);
        Content page = CreatePage("Page", "my-alias");
        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);

        Assert.That(ContentService.Move(page, otherParent.Id).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "my-alias" }));
        AssertAliasTableMatchesRebuildQuery("after move");

        Assert.That(ContentService.Sort(new[] { page }).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "my-alias" }));
        AssertAliasTableMatchesRebuildQuery("after sort");

        IContent draft = ContentService.GetById(page.Id)!;
        draft.SetValue(Constants.Conventions.Content.UrlAlias, "draft-alias");
        ContentService.Save(draft, -1);
        var oldestVersionId = ContentService.GetVersionIds(page.Id, int.MaxValue).Min();
        Assert.That(ContentService.Rollback(page.Id, oldestVersionId).Success, Is.True);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "my-alias" }));
        AssertAliasTableMatchesRebuildQuery("after rollback");

        IContent copy = ContentService.Copy(page, RootPage.Id, relateToOriginal: false)!;
        Assert.That(AliasesFor(copy.Key), Is.Empty, "An unpublished copy has no published alias.");
        AssertAliasTableMatchesRebuildQuery("after copy");

        Assert.That(ContentService.Publish(copy, ["*"]).Success, Is.True);
        Assert.That(AliasesFor(copy.Key), Is.EqualTo(new[] { "my-alias" }));
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "my-alias" }));
        AssertAliasTableMatchesRebuildQuery("after publishing the copy");
    }

    [Test]
    public async Task VariantContent_UnpublishingOneCulture_RemovesOnlyThatCulturesRows()
    {
        (ILanguage english, ILanguage french, Content page) = await CreatePublishedVariantPageAsync();
        Assert.That(
            RowsFor(page.Key).Select(x => (x.NullableLanguageId, x.Alias)),
            Is.EquivalentTo(new[] { ((int?)english.Id, "english-alias"), ((int?)french.Id, "french-alias") }));
        AssertAliasTableMatchesRebuildQuery("after publishing both cultures");

        Assert.That(ContentService.Unpublish(page, FrenchIsoCode).Success, Is.True);
        Assert.That(
            RowsFor(page.Key).Select(x => (x.NullableLanguageId, x.Alias)),
            Is.EquivalentTo(new[] { ((int?)english.Id, "english-alias") }));
        AssertAliasTableMatchesRebuildQuery("after unpublishing one culture");

        Assert.That(ContentService.Unpublish(page, EnglishIsoCode).Success, Is.True);
        Assert.That(RowsFor(page.Key), Is.Empty, "Unpublishing the last culture unpublishes the document.");
        AssertAliasTableMatchesRebuildQuery("after unpublishing the last culture");
    }

    [Test]
    public async Task VariantContent_UnpublishingTheMandatoryCulture_RemovesAllRows()
    {
        (ILanguage english, _, Content page) = await CreatePublishedVariantPageAsync();
        english.IsMandatory = true;
        Assert.That((await LanguageService.UpdateAsync(english, Constants.Security.SuperUserKey)).Success, Is.True);

        PublishResult result = ContentService.Unpublish(page, EnglishIsoCode);

        Assert.That(result.Result, Is.EqualTo(PublishResultType.SuccessUnpublishMandatoryCulture));
        Assert.That(RowsFor(page.Key), Is.Empty);
        AssertAliasTableMatchesRebuildQuery("after unpublishing the mandatory culture");
    }

    [Test]
    public void Publish_WithScopedNotificationsSuppressed_StillWritesTheRows()
    {
        Content page = CreatePage("Page", "my-alias");

        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope())
        {
            using (scope.Notifications.Suppress())
            {
                Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);
            }

            scope.Complete();
        }

        Assert.That(
            AliasesFor(page.Key),
            Is.EqualTo(new[] { "my-alias" }),
            "The alias rows are part of the content transaction, so suppressing the scoped notifications must not lose them.");
        AssertAliasTableMatchesRebuildQuery("after a publish with suppressed notifications");
    }

    [Test]
    public void AliasWriteFailure_RollsBackThePublish()
    {
        Content page = CreatePage("Page", "my-alias");
        FailableRepository.FailWrites = true;

        try
        {
            Assert.That(() => ContentService.Publish(page, ["*"]), Throws.Exception);
        }
        finally
        {
            FailableRepository.FailWrites = false;
        }

        IContent reloaded = ContentService.GetById(page.Id)!;
        Assert.That(reloaded.Published, Is.False, "A publish whose alias rows cannot be written must roll back, not commit without them.");
        Assert.That(RowsFor(page.Key), Is.Empty);
        AssertAliasTableMatchesRebuildQuery("after a rolled-back publish");
    }

    [Test]
    public void SaveBlueprint_DoesNotWriteAliasRows()
    {
        Content blueprint = ContentBuilder.CreateSimpleContent(ContentType, "Blueprint");
        blueprint.SetValue(Constants.Conventions.Content.UrlAlias, "blueprint-alias");

        ContentService.SaveBlueprint(blueprint, -1);

        Assert.That(RowsFor(blueprint.Key), Is.Empty);
        AssertAliasTableMatchesRebuildQuery("after saving a blueprint");
    }

    [Test]
    public async Task Publish_UpdatesTheInMemoryCache_AndTrashRemovesItAgain()
    {
        Content page = CreatePage("Page", "my-alias");
        Assert.That(ContentService.Publish(page, ["*"]).Success, Is.True);

        Assert.That(await AliasService.GetDocumentKeysByAliasAsync("my-alias", null), Is.EqualTo(new[] { page.Key }));

        Assert.That(ContentService.MoveToRecycleBin(page).Success, Is.True);

        Assert.That(await AliasService.GetDocumentKeysByAliasAsync("my-alias", null), Is.Empty);
    }

    [Test]
    public void ScheduledPublishAndExpiry_MaintainTheRows()
    {
        Content page = ContentBuilder.CreateSimpleContent(ContentType, "Scheduled", RootPage.Id);
        page.SetValue(Constants.Conventions.Content.UrlAlias, "scheduled-alias");
        DateTime now = DateTime.Now;
        ContentService.Save(page, -1, ContentScheduleCollection.CreateWithEntry(now.AddMinutes(-1), null));
        Assert.That(RowsFor(page.Key), Is.Empty, "Nothing is published yet.");

        List<PublishResult> released = ContentService.PerformScheduledPublish(now).ToList();

        Assert.That(released.Any(x => x.Entity.Id == page.Id && x.Success), Is.True);
        Assert.That(AliasesFor(page.Key), Is.EqualTo(new[] { "scheduled-alias" }));
        AssertAliasTableMatchesRebuildQuery("after a scheduled publish");

        ContentService.PersistContentSchedule(page, ContentScheduleCollection.CreateWithEntry(null, now.AddMinutes(-1)));
        List<PublishResult> expired = ContentService.PerformScheduledPublish(now).ToList();

        Assert.That(expired.Any(x => x.Entity.Id == page.Id && x.Success), Is.True);
        Assert.That(RowsFor(page.Key), Is.Empty);
        AssertAliasTableMatchesRebuildQuery("after a scheduled expiry");
    }

    [Test]
    public void PublishBranch_WritesRowsForEveryDescendantWithAnAlias()
    {
        Content parent = CreatePage("Parent", "parent-alias");
        var children = new List<Content>();
        for (var i = 1; i <= 6; i++)
        {
            children.Add(CreatePage($"Child {i}", i % 2 == 0 ? $"child-{i}" : null, parent.Id));
        }

        ContentService.PublishBranch(parent, PublishBranchFilter.IncludeUnpublished, ["*"]);

        Assert.That(AliasesFor(parent.Key), Is.EqualTo(new[] { "parent-alias" }));
        Assert.That(children.Count(c => AliasesFor(c.Key).Count > 0), Is.EqualTo(3));
        AssertAliasTableMatchesRebuildQuery("after publishing a branch");
    }

    [Test]
    public async Task RebuildAllAliasesAsync_AfterTheWrites_ChangesNothing()
    {
        Content kept = CreatePage("Kept", "kept-alias");
        Assert.That(ContentService.Publish(kept, ["*"]).Success, Is.True);
        Content unpublished = CreatePage("Unpublished", "gone-alias");
        Assert.That(ContentService.Publish(unpublished, ["*"]).Success, Is.True);
        Assert.That(ContentService.Unpublish(unpublished).Success, Is.True);
        Content trashed = CreatePage("Trashed", "trashed-alias");
        Assert.That(ContentService.Publish(trashed, ["*"]).Success, Is.True);
        Assert.That(ContentService.MoveToRecycleBin(trashed).Success, Is.True);
        List<(Guid, int?, string)> before = AllRows();

        await AliasService.RebuildAllAliasesAsync();

        Assert.That(AllRows(), Is.EquivalentTo(before));
        Assert.That(before.Select(x => x.Item3), Is.EquivalentTo(new[] { "kept-alias" }));
    }

    private Content CreatePage(string name, string? alias, int? parentId = null)
    {
        Content page = ContentBuilder.CreateSimpleContent(ContentType, name, parentId ?? RootPage.Id);
        if (alias is not null)
        {
            page.SetValue(Constants.Conventions.Content.UrlAlias, alias);
        }

        ContentService.Save(page, -1);
        return page;
    }

    private async Task<(ILanguage English, ILanguage French, Content Page)> CreatePublishedVariantPageAsync()
    {
        ILanguage french = new LanguageBuilder().WithCultureInfo(FrenchIsoCode).Build();
        Assert.That((await LanguageService.CreateAsync(french, Constants.Security.SuperUserKey)).Success, Is.True);
        ILanguage english = (await LanguageService.GetAsync(EnglishIsoCode))!;
        french = (await LanguageService.GetAsync(FrenchIsoCode))!;

        ContentType variantType = CreateContentTypeWithUrlAlias("pageWithAliasVariant", ContentVariation.Culture);
        Assert.That((await ContentTypeService.CreateAsync(variantType, Constants.Security.SuperUserKey)).Success, Is.True);

        var page = new Content("Variant", RootPage.Id, variantType, Constants.Security.SuperUserId, EnglishIsoCode);
        page.SetCultureName("Variante", FrenchIsoCode);
        page.SetValue(Constants.Conventions.Content.UrlAlias, "english-alias", EnglishIsoCode);
        page.SetValue(Constants.Conventions.Content.UrlAlias, "french-alias", FrenchIsoCode);
        ContentService.Save(page, -1);
        PublishResult result = ContentService.Publish(page, [EnglishIsoCode, FrenchIsoCode]);
        Assert.That(result.Success, Is.True, result.Result.ToString());

        return (english, french, page);
    }

    private List<PublishedDocumentUrlAlias> RowsFor(Guid documentKey)
    {
        using (CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return AliasRepository.GetAll().Where(x => x.DocumentKey == documentKey).ToList();
        }
    }

    private List<string> AliasesFor(Guid documentKey)
        => RowsFor(documentKey).Select(x => x.Alias).OrderBy(x => x, StringComparer.Ordinal).ToList();

    private List<(Guid, int?, string)> AllRows()
    {
        using (CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return AliasRepository.GetAll().Select(x => (x.DocumentKey, x.NullableLanguageId, x.Alias)).ToList();
        }
    }

    /// <summary>
    /// The rebuild's source query encodes the contract (published version only, not trashed, not a blueprint), so
    /// per-document writes are correct exactly when the table equals its normalised output.
    /// </summary>
    private void AssertAliasTableMatchesRebuildQuery(string step)
    {
        List<(Guid, int?, string)> rows;
        List<(Guid, int?, string)> expected;
        using (CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            rows = AliasRepository.GetAll().Select(x => (x.DocumentKey, x.NullableLanguageId, x.Alias)).ToList();
            expected = AliasRepository.GetAllDocumentUrlAliases()
                .SelectMany(raw => Normalize(raw.AliasValue).Select(alias => (raw.DocumentKey, raw.LanguageId, alias)))
                .Distinct()
                .ToList();
        }

        Assert.That(rows, Is.EquivalentTo(expected), $"The alias table must equal what a rebuild would produce {step}.");
    }

    private static IEnumerable<string> Normalize(string rawValue)
        => rawValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().Trim('/').ToLowerInvariant())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private ContentType CreateContentTypeWithUrlAlias(string alias, ContentVariation variation)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithContentVariation(variation)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSortOrder(1)
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithAlias("title")
                    .WithName("Title")
                    .WithSortOrder(1)
                    .Done()
                .AddPropertyType()
                    .WithAlias("bodyText")
                    .WithName("Body text")
                    .WithSortOrder(2)
                    .Done()
                .AddPropertyType()
                    .WithAlias("author")
                    .WithName("Author")
                    .WithSortOrder(3)
                    .Done()
                .AddPropertyType()
                    .WithAlias(Constants.Conventions.Content.UrlAlias)
                    .WithName("URL Alias")
                    .WithSortOrder(4)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithVariations(variation)
                    .Done()
                .Done()
            .AddAllowedTemplate()
                .WithId(TemplateId)
                .WithAlias("defaultTemplate")
                .WithName("Default Template")
                .Done()
            .WithDefaultTemplateId(TemplateId)
            .Build();

        return (ContentType)contentType;
    }

    internal sealed class LockRecorder
    {
        private readonly ConcurrentQueue<(int LockId, DistributedLockType Type)> _obtained = new();

        public IReadOnlyList<(int LockId, DistributedLockType Type)> Obtained => _obtained.ToList();

        public void Record(int lockId, DistributedLockType type) => _obtained.Enqueue((lockId, type));

        public void Clear() => _obtained.Clear();
    }

    private sealed class RecordingLockingMechanismFactory(IDistributedLockingMechanismFactory inner, LockRecorder recorder)
        : IDistributedLockingMechanismFactory
    {
        private RecordingLockingMechanism? _mechanism;

        public IDistributedLockingMechanism DistributedLockingMechanism
            => _mechanism ??= new RecordingLockingMechanism(inner.DistributedLockingMechanism, recorder);
    }

    private sealed class RecordingLockingMechanism(IDistributedLockingMechanism inner, LockRecorder recorder) : IDistributedLockingMechanism
    {
        public bool Enabled => inner.Enabled;

        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null)
        {
            IDistributedLock obtained = inner.ReadLock(lockId, obtainLockTimeout);
            recorder.Record(lockId, DistributedLockType.ReadLock);
            return obtained;
        }

        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null)
        {
            IDistributedLock obtained = inner.WriteLock(lockId, obtainLockTimeout);
            recorder.Record(lockId, DistributedLockType.WriteLock);
            return obtained;
        }
    }

    /// <summary>Delegates to the real repository, and fails every write while <see cref="FailWrites"/> is set.</summary>
    internal sealed class FailableAliasRepository(IDocumentUrlAliasRepository inner) : IDocumentUrlAliasRepository
    {
        public bool FailWrites { get; set; }

        public void Save(IEnumerable<PublishedDocumentUrlAlias> aliases)
        {
            ThrowIfFailing();
            inner.Save(aliases);
        }

        public IEnumerable<PublishedDocumentUrlAlias> GetAll() => inner.GetAll();

        public void DeleteByDocumentKey(IEnumerable<Guid> documentKeys)
        {
            ThrowIfFailing();
            inner.DeleteByDocumentKey(documentKeys);
        }

        public void DeleteAll()
        {
            ThrowIfFailing();
            inner.DeleteAll();
        }

        public IEnumerable<DocumentUrlAliasRaw> GetAllDocumentUrlAliases() => inner.GetAllDocumentUrlAliases();

        private void ThrowIfFailing()
        {
            if (FailWrites)
            {
                throw new InvalidOperationException("Simulated alias write failure.");
            }
        }
    }
}
