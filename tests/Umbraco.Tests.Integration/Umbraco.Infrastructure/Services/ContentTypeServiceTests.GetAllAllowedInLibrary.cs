// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class ContentTypeServiceTests
{
    [Test]
    public async Task GetAllAllowedInLibrary_Returns_Element_Types_With_AllowedInLibrary_True()
    {
        var contentType = ContentTypeBuilder.CreateBasicElementType("allowedElement", "Allowed Element");
        contentType.AllowedInLibrary = true;
        ContentTypeService.Save(contentType);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.AreEqual(1, result.Total);
        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual("allowedElement", result.Items.First().Alias);
    }

    [Test]
    public async Task GetAllAllowedInLibrary_Excludes_Non_Element_Types_Even_If_AllowedInLibrary_True()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType("nonElement", "Non Element");
        contentType.AllowedInLibrary = true;
        ContentTypeService.Save(contentType);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.AreEqual(0, result.Total);
        Assert.AreEqual(0, result.Items.Count());
    }

    [Test]
    public async Task GetAllAllowedInLibrary_Excludes_Element_Types_With_AllowedInLibrary_False()
    {
        var contentType = ContentTypeBuilder.CreateBasicElementType("notAllowed", "Not Allowed Element");
        contentType.AllowedInLibrary = false;
        ContentTypeService.Save(contentType);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.AreEqual(0, result.Total);
        Assert.AreEqual(0, result.Items.Count());
    }

    [Test]
    public async Task GetAllAllowedInLibrary_Returns_Empty_When_No_Matching_Types_Exist()
    {
        // Create non-matching types
        var nonElement = ContentTypeBuilder.CreateBasicContentType("nonElement", "Non Element");
        nonElement.AllowedInLibrary = true;
        ContentTypeService.Save(nonElement);

        var elementNotAllowed = ContentTypeBuilder.CreateBasicElementType("notAllowed", "Not Allowed");
        elementNotAllowed.AllowedInLibrary = false;
        ContentTypeService.Save(elementNotAllowed);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.AreEqual(0, result.Total);
        Assert.AreEqual(0, result.Items.Count());
    }

    [Test]
    public async Task GetAllAllowedInLibrary_Pagination_Works_Correctly()
    {
        for (var i = 0; i < 3; i++)
        {
            var contentType = ContentTypeBuilder.CreateBasicElementType($"allowed{i}", $"Allowed Element {i}");
            contentType.AllowedInLibrary = true;
            ContentTypeService.Save(contentType);
        }

        var firstPage = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 2);
        Assert.AreEqual(3, firstPage.Total);
        Assert.AreEqual(2, firstPage.Items.Count());

        var secondPage = await ContentTypeService.GetAllAllowedInLibraryAsync(2, 2);
        Assert.AreEqual(3, secondPage.Total);
        Assert.AreEqual(1, secondPage.Items.Count());
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToAllowInLibrary))]
    public async Task GetAllAllowedInLibrary_With_Filter_Allows_Matching_Types()
    {
        var allowed = ContentTypeBuilder.CreateBasicElementType("allowedElement", "Allowed Element");
        allowed.AllowedInLibrary = true;
        ContentTypeService.Save(allowed);

        var other = ContentTypeBuilder.CreateBasicElementType("otherElement", "Other Element");
        other.AllowedInLibrary = true;
        ContentTypeService.Save(other);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.AreEqual(1, result.Total);
        Assert.AreEqual("allowedElement", result.Items.First().Alias);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowInLibrary))]
    public async Task GetAllAllowedInLibrary_With_Filter_Excludes_Matching_Types()
    {
        var allowed = ContentTypeBuilder.CreateBasicElementType("allowedElement", "Allowed Element");
        allowed.AllowedInLibrary = true;
        ContentTypeService.Save(allowed);

        var other = ContentTypeBuilder.CreateBasicElementType("otherElement", "Other Element");
        other.AllowedInLibrary = true;
        ContentTypeService.Save(other);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.AreEqual(1, result.Total);
        Assert.AreEqual("otherElement", result.Items.First().Alias);
    }

    public static void ConfigureContentTypeFilterToAllowInLibrary(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForAllowedInLibrary>();

    public static void ConfigureContentTypeFilterToDisallowInLibrary(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForDisallowedInLibrary>();

    private class ContentTypeFilterForAllowedInLibrary() : ContentTypeFilterForInLibrary(true);

    private class ContentTypeFilterForDisallowedInLibrary() : ContentTypeFilterForInLibrary(false);

    private abstract class ContentTypeFilterForInLibrary(bool allowed) : IContentTypeFilter
    {
        public Task<IEnumerable<TItem>> FilterAllowedInLibraryAsync<TItem>(IEnumerable<TItem> contentTypes)
            where TItem : IContentTypeComposition
            => Task.FromResult(contentTypes.Where(x =>
                (allowed && x.Alias == "allowedElement") || (!allowed && x.Alias != "allowedElement")));
    }
}
