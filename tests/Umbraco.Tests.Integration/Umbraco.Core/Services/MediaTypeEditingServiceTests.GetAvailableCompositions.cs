using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     These tests are enough to test that we are not mixing content and media types when looking for compositions,
///     even though those share a base implementation.
///     For a more detailed test suite on compositions, check <see cref="ContentTypeEditingServiceTests" />.
/// </summary>
public partial class MediaTypeEditingServiceTests
{
    [Test]
    public async Task Can_Get_Available_Compositions_Only_From_Media_Type_Key()
    {
        var container1 = MediaTypePropertyContainerModel();
        var propertyType1 = MediaTypePropertyTypeModel("Property 1", "property1", containerKey: container1.Key);
        var mediaType1 = MediaTypeCreateModel(
            "Media type 1",
            "mediaType1",
            propertyTypes: new[] { propertyType1 },
            containers: new[] { container1 });

        var result1 = await MediaTypeEditingService.CreateAsync(mediaType1, Constants.Security.SuperUserKey);

        var container2 = MediaTypePropertyContainerModel();
        var propertyType2 = MediaTypePropertyTypeModel("Property 2", "property2", containerKey: container2.Key);
        var mediaType2 = MediaTypeCreateModel(
            "Media type 2",
            "mediaType2",
            propertyTypes: new[] { propertyType2 },
            containers: new[] { container2 });

        var result2 = await MediaTypeEditingService.CreateAsync(mediaType2, Constants.Security.SuperUserKey);

        var container3 = MediaTypePropertyContainerModel();
        var propertyType3 = MediaTypePropertyTypeModel("Property 3", "property3", containerKey: container3.Key);
        var mediaType3 = MediaTypeCreateModel(
            "Media type 3",
            "mediaType3",
            propertyTypes: new[] { propertyType3 },
            containers: new[] { container3 });

        var result3 = await MediaTypeEditingService.CreateAsync(mediaType3, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await MediaTypeEditingService.GetAvailableCompositionsAsync(
                result1.Result!.Key,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>());

        // Verify that mediaType2 and mediaType3 are present in available compositions
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result2.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result3.Result!.Key && compositionsResult.Allowed));
    }

    [Test]
    public async Task Available_Compositions_For_Media_Type_Are_Not_Content_Types()
    {
        var container1 = MediaTypePropertyContainerModel();
        var propertyType1 = MediaTypePropertyTypeModel("Property 1", "property1", containerKey: container1.Key);
        var mediaType = MediaTypeCreateModel(
            "Media type",
            "mediaType",
            propertyTypes: new[] { propertyType1 },
            containers: new[] { container1 });

        var result1 = await MediaTypeEditingService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var container2 = ContentTypePropertyContainerModel();
        var propertyType2 = ContentTypePropertyTypeModel("Property 2", "property2", containerKey: container2.Key);
        var contentType = ContentTypeCreateModel(
            "Content type",
            "contentType",
            propertyTypes: new[] { propertyType2 },
            containers: new[] { container2 });

        var result2 = await ContentTypeEditingService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await MediaTypeEditingService.GetAvailableCompositionsAsync(
                result1.Result!.Key,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>());

        Assert.IsFalse(availableCompositions.Any(compositionsResult => compositionsResult.Composition.Key == result2.Result!.Key));
    }
}
