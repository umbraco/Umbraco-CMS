using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheDocumentTypeTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangeCapture>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    /// <summary>
    ///     Proves that removing a property does not rebuild the stored database cache, yet the removed property
    ///     is no longer exposed by any published read path (template <c>Value</c>/<c>GetProperty</c>/<c>Properties</c>,
    ///     which is also the source the Delivery API maps from). Correctness comes from resolving against the
    ///     current content type, not from regenerating the stored blob.
    /// </summary>
    [Test]
    public async Task Removing_Property_Hides_Value_Without_Rebuilding_Stored_Cache()
    {
        // Arrange - ensure the draft blob is populated, then snapshot the stored cmsContentNu row.
        var before = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.That(before!.Value("title"), Is.Not.Null);
        var blobBefore = ReadDraftBlobSignature(TextpageId);

        // Act - a pure property removal (a RawDataUnaffected structural change).
        ContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert - the property is gone from every published read path.
        var after = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.Multiple(() =>
        {
            Assert.That(after!.Value("title"), Is.Null, "Templates must not render the removed property's old value.");
            Assert.That(after!.GetProperty("title"), Is.Null);
            Assert.That(after!.Properties.Any(p => p.Alias == "title"), Is.False, "Delivery API maps from Properties, so it must not expose the removed property.");
        });

        // Assert - the stored cmsContentNu blob was NOT rebuilt. A rebuild would have regenerated it from the
        // now-deleted property data (dropping the orphaned value); an unchanged blob proves the rebuild was skipped.
        var blobAfter = ReadDraftBlobSignature(TextpageId);
        Assert.That(blobAfter, Is.EqualTo(blobBefore), "The stored blob should be untouched — the rebuild must be skipped for a property removal.");
    }

    /// <summary>
    ///     Guards against over-broadening the skip: a property alias change is structural but NOT
    ///     RawDataUnaffected, so it must still rebuild the stored cache (the blob is re-keyed to the new alias).
    /// </summary>
    [Test]
    public async Task Renaming_Property_Alias_Still_Rebuilds_Stored_Cache()
    {
        // Arrange
        var before = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.That(before!.Value("title"), Is.Not.Null);
        var blobBefore = ReadDraftBlobSignature(TextpageId);

        // Act - rename an existing property's alias (a structural change that DOES affect the stored data).
        var titleProperty = ContentType.PropertyTypes.First(x => x.Alias == "title");
        titleProperty.Alias = "newTitle";
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert - the stored blob was rebuilt (re-keyed under the new alias), proving the rebuild was not skipped.
        var blobAfter = ReadDraftBlobSignature(TextpageId);
        Assert.That(blobAfter, Is.Not.EqualTo(blobBefore), "An alias change must still rebuild the stored blob.");
    }

    /// <summary>
    ///     A composition removal is structural but is deliberately NOT treated as RawDataUnaffected (kept as a
    ///     full rebuild), so the change must not carry the flag.
    /// </summary>
    [Test]
    public async Task Removing_A_Composition_Still_Requires_A_Rebuild()
    {
        var composition = await CreateContentType("compositionType", "compProp");
        var composing = await CreateContentType("composingType", "ownProp", composition);

        ContentTypeChangeCapture.Changes.Clear();

        // Act
        composing.RemoveContentType("compositionType");
        await ContentTypeService.UpdateAsync(composing, Constants.Security.SuperUserKey);

        // Assert
        var changes = ChangeTypesFor(composing.Id);
        Assert.Multiple(() =>
        {
            Assert.That(changes.Any(c => c.RequiresRawDataRebuild()), Is.True, "Composition removal must still require a rebuild.");
            Assert.That(changes.Any(c => c.HasType(ContentTypeChangeTypes.RawDataUnaffected)), Is.False);
        });
    }

    /// <summary>
    ///     Exercises the multi-type guard: in a batch save, one type only has a property removed (a
    ///     RawDataUnaffected candidate) while a type it composes has a property alias change (rebuild-required,
    ///     which propagates to the composing type). The composing type must NOT end up flagged RawDataUnaffected,
    ///     because its inherited renamed property means the stored blob is genuinely stale.
    /// </summary>
    [Test]
    public async Task Batch_Save_Does_Not_Flag_A_Type_That_Independently_Requires_A_Rebuild()
    {
        var composition = await CreateContentType("compositionType", "compProp");
        var composing = await CreateContentType("composingType", "ownProp", composition);

        ContentTypeChangeCapture.Changes.Clear();

        // Act - composing removes its own property (candidate); composition renames its property alias
        // (rebuild-required, propagates to composing). Saved together as a single batch so both changes are
        // classified in one ComposeContentTypeChanges call — which is what exercises the multi-type guard.
        composing.RemovePropertyType("ownProp");
        composition.PropertyTypes.First(p => p.Alias == "compProp").Alias = "compPropRenamed";
        ContentTypeService.Save(new[] { composing, composition });

        // Assert - the composing type keeps a full rebuild; the guard prevents the removal-only flag.
        // (It can surface as more than one change entry — the guard must leave none of them flagged.)
        var composingChanges = ChangeTypesFor(composing.Id);
        Assert.Multiple(() =>
        {
            Assert.That(composingChanges.Any(c => c.RequiresRawDataRebuild()), Is.True, "A type that inherits a renamed property must still be rebuilt.");
            Assert.That(composingChanges.Any(c => c.HasType(ContentTypeChangeTypes.RawDataUnaffected)), Is.False);
        });
    }

    private static IReadOnlyList<ContentTypeChangeTypes> ChangeTypesFor(int contentTypeId) =>
        ContentTypeChangeCapture.Changes.Where(c => c.Item.Id == contentTypeId).Select(c => c.ChangeTypes).ToList();

    private async Task<IContentType> CreateContentType(string alias, string propertyAlias, IContentType? composition = null)
    {
        ContentType contentType = ContentTypeBuilder.CreateBasicContentType(alias, alias);
        contentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, propertyAlias)
            {
                Name = propertyAlias,
                DataTypeId = Constants.DataTypes.Textbox,
            },
            "content",
            "Content");

        if (composition is not null)
        {
            contentType.AddContentType(composition);
        }

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    private string ReadDraftBlobSignature(int nodeId)
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var sql = SqlContext.Sql()
            .Select<ContentNuDto>()
            .From<ContentNuDto>()
            .Where<ContentNuDto>(x => x.NodeId == nodeId);
        ContentNuDto draft = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(sql).Single(x => !x.Published);
        return (draft.Data ?? string.Empty) + "|" + Convert.ToBase64String(draft.RawData ?? Array.Empty<byte>());
    }

    private sealed class ContentTypeChangeCapture : INotificationHandler<ContentTypeChangedNotification>
    {
        public static readonly List<ContentTypeChange<IContentType>> Changes = new();

        public void Handle(ContentTypeChangedNotification notification) => Changes.AddRange(notification.Changes);
    }

    [Test]
    public async Task Structural_Update_Removes_Property_From_Draft_Content_By_Id()
    {
        // Act
        var oldTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNotNull(oldTextPage.Value("title"));

        ContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNull(newTextPage.Value("title"));
    }

    [Test]
    public async Task Structural_Update_Removes_Property_From_Draft_Content_By_Key()
    {
        // Act
        await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        ContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);
        Assert.IsNull(newTextPage.Value("title"));
    }

    [Test]
    public async Task Non_Structural_Update_Preserves_Property_Values_In_Draft_Content()
    {
        // Arrange - load content into cache and verify the title property has a value.
        var oldTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNotNull(oldTextPage);
        var originalTitle = oldTextPage.Value("title");
        Assert.IsNotNull(originalTitle);

        // Act - perform a non-structural change (rename the content type).
        ContentType.Name = "Renamed Textpage";
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert - property values should still be available from cache.
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNotNull(newTextPage);
        Assert.AreEqual(originalTitle, newTextPage.Value("title"));
    }

    [Test]
    public async Task Non_Structural_Update_Preserves_Property_Values_When_Fetched_By_Key()
    {
        // Arrange - load content into cache by key and verify the title property has a value.
        var oldTextPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);
        Assert.IsNotNull(oldTextPage);
        var originalTitle = oldTextPage.Value("title");
        Assert.IsNotNull(originalTitle);

        // Act - perform a non-structural change (update the description).
        ContentType.Description = "Updated description";
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert - property values should still be available from cache.
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);
        Assert.IsNotNull(newTextPage);
        Assert.AreEqual(originalTitle, newTextPage.Value("title"));
    }

    [Test]
    public async Task Content_Gets_Removed_When_DocumentType_Is_Deleted()
    {
        // Load into cache
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, preview: true);
        Assert.IsNotNull(textPage);

        await ContentTypeService.DeleteAsync(textPage.ContentType.Key, Constants.Security.SuperUserKey);

        var textPageAgain = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, preview: true);
        Assert.IsNull(textPageAgain);
    }
}
