using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Search.Core.Cache.ContentType;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.NotificationHandlers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentTypeTests : SearcherTestBase
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentType _parentContentType = null!;
    private IContentType _childContentType = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, RebuildIndexesNotificationHandler>();
    }

    [Test]
    public async Task CannotSearchForRemovedPropertyType()
    {
        await CreateDocumentsAndWaitForIndexing();

        SearchResult results = await Searcher.SearchAsync(
            Cms.Core.Constants.IndexAliases.DraftContent,
            query: "Home Page");

        Assert.That(results.Total, Is.EqualTo(2));

        _childContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(_childContentType, Constants.Security.SuperUserKey);

        await WaitForIndexesToRebuild();

        results = await Searcher.SearchAsync(
            Cms.Core.Constants.IndexAliases.DraftContent,
            query: "Home Page");

        Assert.That(results.Total, Is.EqualTo(1));
    }

    [Test]
    public async Task CannotSearchForRemovedDocumentType()
    {
        await CreateDocumentsAndWaitForIndexing();

        SearchResult results = await Searcher.SearchAsync(
            Cms.Core.Constants.IndexAliases.DraftContent,
            query: "Home Page");

        Assert.That(results.Total, Is.EqualTo(2));

        await ContentTypeService.DeleteAsync(_childContentType.Key, Constants.Security.SuperUserKey);

        await WaitForIndexesToRebuild();

        results = await Searcher.SearchAsync(
            Cms.Core.Constants.IndexAliases.DraftContent,
            query: "Home Page");

        Assert.That(results.Total, Is.EqualTo(1));

        IContentType? contentType = await ContentTypeService.GetAsync(_childContentType.Key);
        Assert.That(contentType, Is.Null);
    }

    private async Task CreateDocumentsAndWaitForIndexing()
        => await WaitForIndexing(
            Cms.Core.Constants.IndexAliases.DraftContent,
            async () => await CreateDocuments());

    private async Task CreateDocuments()
    {
        ContentTypeCreateModel parentContentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType(
            "parentType",
            "Parent Type");
        Attempt<IContentType?, ContentTypeOperationStatus> parentContentTypeAttempt = await ContentTypeEditingService.CreateAsync(
            parentContentTypeCreateModel,
            Constants.Security.SuperUserKey);
        Assert.That(parentContentTypeAttempt.Success, Is.True);
        _parentContentType = parentContentTypeAttempt.Result!;

        // Create Child ContentType
        ContentTypeCreateModel childContentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType(
            "childType",
            "Child Type");
        Attempt<IContentType?, ContentTypeOperationStatus> childContentTypeAttempt = await ContentTypeEditingService.CreateAsync(
            childContentTypeCreateModel,
            Constants.Security.SuperUserKey);
        Assert.That(childContentTypeAttempt.Success, Is.True);
        _childContentType = childContentTypeAttempt.Result!;

        // Update Parent ContentType to allow Child ContentType
        ContentTypeUpdateModel parentContentTypeUpdateModel = ContentTypeUpdateHelper.CreateContentTypeUpdateModel(_parentContentType);
        parentContentTypeUpdateModel.AllowedContentTypes =
        [
            new ContentTypeSort(_childContentType.Key, 0, childContentTypeCreateModel.Alias)
        ];
        Attempt<IContentType?, ContentTypeOperationStatus> updatedParentResult = await ContentTypeEditingService.UpdateAsync(
            _parentContentType,
            parentContentTypeUpdateModel,
            Constants.Security.SuperUserKey);
        Assert.That(updatedParentResult.Success, Is.True);

        // Create Root Document (Parent)
        ContentCreateModel rootCreateModel = ContentEditingBuilder.CreateSimpleContent(_parentContentType.Key, "Root Document");
        Attempt<ContentCreateResult, ContentEditingOperationStatus> createRootResult = await ContentEditingService.CreateAsync(rootCreateModel, Constants.Security.SuperUserKey);
        Assert.That(createRootResult.Success, Is.True);
        IContent? rootDocument = createRootResult.Result.Content;

        // Create Child Document under Root
        ContentCreateModel childCreateModel = ContentEditingBuilder.CreateSimpleContent(
            _childContentType.Key,
            "Child Document",
            rootDocument!.Key);
        Attempt<ContentCreateResult, ContentEditingOperationStatus> createChildResult = await ContentEditingService.CreateAsync(childCreateModel, Constants.Security.SuperUserKey);
        Assert.That(createChildResult.Success, Is.True);
    }

    private async Task WaitForIndexesToRebuild()
        => await Task.Delay(3000);
}
