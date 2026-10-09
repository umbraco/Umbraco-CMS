// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class ContentTypeServiceTests
{
    [Test]
    public async Task GetAllAllowedForBlueprints_Returns_Document_Types()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType("documentType", "Document Type");
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedForBlueprintsAsync(null, 0, 100);

        Assert.AreEqual(1, result.Total);
        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual("documentType", result.Items.First().Alias);
    }

    [Test]
    public async Task GetAllAllowedForBlueprints_Excludes_Element_Types()
    {
        var documentType = ContentTypeBuilder.CreateBasicContentType("documentType", "Document Type");
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);

        var elementType = ContentTypeBuilder.CreateBasicElementType("elementType", "Element Type");
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedForBlueprintsAsync(null, 0, 100);

        Assert.AreEqual(1, result.Total);
        Assert.AreEqual("documentType", result.Items.First().Alias);
    }

    [Test]
    public async Task GetAllAllowedForBlueprints_Pagination_Works_Correctly()
    {
        for (var i = 0; i < 3; i++)
        {
            var contentType = ContentTypeBuilder.CreateBasicContentType($"documentType{i}", $"Document Type {i}");
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        }

        var firstPage = await ContentTypeService.GetAllAllowedForBlueprintsAsync(null, 0, 2);
        Assert.AreEqual(3, firstPage.Total);
        Assert.AreEqual(2, firstPage.Items.Count());

        var secondPage = await ContentTypeService.GetAllAllowedForBlueprintsAsync(null, 2, 2);
        Assert.AreEqual(3, secondPage.Total);
        Assert.AreEqual(1, secondPage.Items.Count());
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterExcludingFromBlueprints))]
    public async Task GetAllAllowedForBlueprints_With_Filter_Excludes_Matching_Types()
    {
        var excluded = ContentTypeBuilder.CreateBasicContentType(ContentTypeFilterExcludingFromBlueprints.ExcludedAlias, "Excluded");
        await ContentTypeService.CreateAsync(excluded, Constants.Security.SuperUserKey);

        var other = ContentTypeBuilder.CreateBasicContentType("otherDocumentType", "Other Document Type");
        await ContentTypeService.CreateAsync(other, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedForBlueprintsAsync(null, 0, 100);

        Assert.AreEqual(1, result.Total);
        Assert.AreEqual("otherDocumentType", result.Items.First().Alias);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterCapturingBlueprintParentKey))]
    public async Task GetAllAllowedForBlueprints_Threads_ParentKey_To_Filter()
    {
        ContentTypeFilterCapturingBlueprintParentKey.Reset();

        var contentType = ContentTypeBuilder.CreateBasicContentType("documentType", "Document Type");
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parentKey = Guid.NewGuid();
        await ContentTypeService.GetAllAllowedForBlueprintsAsync(parentKey, 0, 100);

        Assert.IsTrue(ContentTypeFilterCapturingBlueprintParentKey.WasCalled);
        Assert.AreEqual(parentKey, ContentTypeFilterCapturingBlueprintParentKey.LastParentKey);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterCapturingBlueprintParentKey))]
    public async Task GetAllAllowedForBlueprints_Threads_Null_ParentKey_At_Root_To_Filter()
    {
        ContentTypeFilterCapturingBlueprintParentKey.Reset();

        var contentType = ContentTypeBuilder.CreateBasicContentType("documentType", "Document Type");
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        await ContentTypeService.GetAllAllowedForBlueprintsAsync(null, 0, 100);

        Assert.IsTrue(ContentTypeFilterCapturingBlueprintParentKey.WasCalled);
        Assert.IsNull(ContentTypeFilterCapturingBlueprintParentKey.LastParentKey);
    }

    public static void ConfigureContentTypeFilterExcludingFromBlueprints(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterExcludingFromBlueprints>();

    public static void ConfigureContentTypeFilterCapturingBlueprintParentKey(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterCapturingBlueprintParentKey>();

    private class ContentTypeFilterExcludingFromBlueprints : IContentTypeFilter
    {
        public const string ExcludedAlias = "excludedDocumentType";

        public Task<IEnumerable<TItem>> FilterAllowedForBlueprintsAsync<TItem>(IEnumerable<TItem> contentTypes, Guid? parentKey)
            where TItem : IContentTypeComposition
            => Task.FromResult(contentTypes.Where(x => x.Alias != ExcludedAlias));
    }

    private class ContentTypeFilterCapturingBlueprintParentKey : IContentTypeFilter
    {
        public static bool WasCalled { get; private set; }

        public static Guid? LastParentKey { get; private set; }

        public static void Reset()
        {
            WasCalled = false;
            LastParentKey = null;
        }

        public Task<IEnumerable<TItem>> FilterAllowedForBlueprintsAsync<TItem>(IEnumerable<TItem> contentTypes, Guid? parentKey)
            where TItem : IContentTypeComposition
        {
            WasCalled = true;
            LastParentKey = parentKey;
            return Task.FromResult(contentTypes);
        }
    }
}
