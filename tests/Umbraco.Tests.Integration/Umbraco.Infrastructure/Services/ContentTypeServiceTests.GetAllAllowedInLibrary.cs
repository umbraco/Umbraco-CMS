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
    public async Task GetAllAllowedInLibrary_Returns_Element_Types_With_AllowedInLibrary_True()
    {
        var contentType = ContentTypeBuilder.CreateBasicElementType("allowedElement", "Allowed Element");
        contentType.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().Alias, Is.EqualTo("allowedElement"));
    }

    [Test]
    public async Task GetAllAllowedInLibrary_Excludes_Non_Element_Types_Even_If_AllowedInLibrary_True()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType("nonElement", "Non Element");
        contentType.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.That(result.Total, Is.EqualTo(0));
        Assert.That(result.Items.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetAllAllowedInLibrary_Excludes_Element_Types_With_AllowedInLibrary_False()
    {
        var contentType = ContentTypeBuilder.CreateBasicElementType("notAllowed", "Not Allowed Element");
        contentType.AllowedInLibrary = false;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.That(result.Total, Is.EqualTo(0));
        Assert.That(result.Items.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetAllAllowedInLibrary_Returns_Empty_When_No_Matching_Types_Exist()
    {
        // Create non-matching types
        var nonElement = ContentTypeBuilder.CreateBasicContentType("nonElement", "Non Element");
        nonElement.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(nonElement, Constants.Security.SuperUserKey);

        var elementNotAllowed = ContentTypeBuilder.CreateBasicElementType("notAllowed", "Not Allowed");
        elementNotAllowed.AllowedInLibrary = false;
        await ContentTypeService.CreateAsync(elementNotAllowed, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.That(result.Total, Is.EqualTo(0));
        Assert.That(result.Items.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetAllAllowedInLibrary_Pagination_Works_Correctly()
    {
        for (var i = 0; i < 3; i++)
        {
            var contentType = ContentTypeBuilder.CreateBasicElementType($"allowed{i}", $"Allowed Element {i}");
            contentType.AllowedInLibrary = true;
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        }

        var firstPage = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 2);
        Assert.That(firstPage.Total, Is.EqualTo(3));
        Assert.That(firstPage.Items.Count(), Is.EqualTo(2));

        var secondPage = await ContentTypeService.GetAllAllowedInLibraryAsync(2, 2);
        Assert.That(secondPage.Total, Is.EqualTo(3));
        Assert.That(secondPage.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToAllowInLibrary))]
    public async Task GetAllAllowedInLibrary_With_Filter_Allows_Matching_Types()
    {
        var allowed = ContentTypeBuilder.CreateBasicElementType("allowedElement", "Allowed Element");
        allowed.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(allowed, Constants.Security.SuperUserKey);

        var other = ContentTypeBuilder.CreateBasicElementType("otherElement", "Other Element");
        other.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(other, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.First().Alias, Is.EqualTo("allowedElement"));
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowInLibrary))]
    public async Task GetAllAllowedInLibrary_With_Filter_Excludes_Matching_Types()
    {
        var allowed = ContentTypeBuilder.CreateBasicElementType("allowedElement", "Allowed Element");
        allowed.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(allowed, Constants.Security.SuperUserKey);

        var other = ContentTypeBuilder.CreateBasicElementType("otherElement", "Other Element");
        other.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(other, Constants.Security.SuperUserKey);

        var result = await ContentTypeService.GetAllAllowedInLibraryAsync(0, 100);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.First().Alias, Is.EqualTo("otherElement"));
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
