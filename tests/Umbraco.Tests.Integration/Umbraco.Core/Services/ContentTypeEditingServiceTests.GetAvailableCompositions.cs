using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ContentTypeEditingServiceTests
{
    [Test]
    public async Task Can_Get_Available_Compositions_Only_From_Content_Type_Key()
    {
        var container1 = ContentTypePropertyContainerModel();
        var propertyType1 = ContentTypePropertyTypeModel("Property 1", "property1", containerKey: container1.Key);
        var contentType1 = ContentTypeCreateModel(
            "Content type 1",
            "contentType1",
            propertyTypes: new[] { propertyType1 },
            containers: new[] { container1 });

        var result1 = await ContentTypeEditingService.CreateAsync(contentType1, Constants.Security.SuperUserKey);

        var container2 = ContentTypePropertyContainerModel();
        var propertyType2 = ContentTypePropertyTypeModel("Property 2", "property2", containerKey: container2.Key);
        var contentType2 = ContentTypeCreateModel(
            "Content type 2",
            "contentType2",
            propertyTypes: new[] { propertyType2 },
            containers: new[] { container2 });

        var result2 = await ContentTypeEditingService.CreateAsync(contentType2, Constants.Security.SuperUserKey);

        var container3 = ContentTypePropertyContainerModel();
        var propertyType3 = ContentTypePropertyTypeModel("Property 3", "property3", containerKey: container3.Key);
        var contentType3 = ContentTypeCreateModel(
            "Content type 3",
            "contentType3",
            propertyTypes: new[] { propertyType3 },
            containers: new[] { container3 });

        var result3 = await ContentTypeEditingService.CreateAsync(contentType3, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                result1.Result!.Key,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>(),
                false);

        Assert.AreEqual(2, availableCompositions.Count());

        // Verify that contentType2 and contentType3 are present in available compositions
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result2.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result3.Result!.Key && compositionsResult.Allowed));
    }

    [Test]
    public async Task Available_Compositions_For_Content_Type_Are_Not_Media_Types()
    {
        var container1 = ContentTypePropertyContainerModel();
        var propertyType1 = ContentTypePropertyTypeModel("Property 1", "property1", containerKey: container1.Key);
        var contentType = ContentTypeCreateModel(
            "Content type",
            "contentType",
            propertyTypes: new[] { propertyType1 },
            containers: new[] { container1 });

        var result1 = await ContentTypeEditingService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var container2 = MediaTypePropertyContainerModel();
        var propertyType2 = MediaTypePropertyTypeModel("Property 2", "property2", containerKey: container2.Key);
        var mediaType = MediaTypeCreateModel(
            "Media type",
            "mediaType",
            propertyTypes: new[] { propertyType2 },
            containers: new[] { container2 });

        await MediaTypeEditingService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                result1.Result!.Key,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>(),
                false);

        Assert.AreEqual(0, availableCompositions.Count());
    }
}
