using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class ContentTypeEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Available_Compositions_Only_From_Content_Type_Key(bool isElement)
    {
        var contentType1 = CreateBasicContentTypeModelWithSingleProperty("Content type 1", "Property 1", isElement);
        var result1 = await ContentTypeEditingService.CreateAsync(contentType1, Constants.Security.SuperUserKey);

        var contentType2 = CreateBasicContentTypeModelWithSingleProperty("Content type 2", "Property 2", isElement);
        var result2 = await ContentTypeEditingService.CreateAsync(contentType2, Constants.Security.SuperUserKey);

        var contentType3 = CreateBasicContentTypeModelWithSingleProperty("Content type 3", "Property 3", isElement);
        var result3 = await ContentTypeEditingService.CreateAsync(contentType3, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                result1.Result!.Key,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>(),
                isElement);

        Assert.AreEqual(2, availableCompositions.Count());

        // Verify that contentType2 and contentType3 are present in available compositions
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result2.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result3.Result!.Key && compositionsResult.Allowed));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Available_Compositions_For_Content_Type_Are_Not_Media_Types(bool isElement)
    {
        var contentType = CreateBasicContentTypeModelWithSingleProperty("Content type", "Property 1", isElement);
        var result1 = await ContentTypeEditingService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var container2 = MediaTypePropertyContainerModel();
        var propertyType2 = MediaTypePropertyTypeModel("Property 2", containerKey: container2.Key);
        var mediaType = MediaTypeCreateModel(
            "Media type",
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

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Available_Compositions_For_New_Item(bool isElement)
    {
        var contentType1 = CreateBasicContentTypeModelWithSingleProperty("Content type 1", "Property 1", isElement);
        var result1 = await ContentTypeEditingService.CreateAsync(contentType1, Constants.Security.SuperUserKey);

        var contentType2 = CreateBasicContentTypeModelWithSingleProperty("Content type 2", "Property 2", isElement);
        var result2 = await ContentTypeEditingService.CreateAsync(contentType2, Constants.Security.SuperUserKey);

        var contentType3 = CreateBasicContentTypeModelWithSingleProperty("Content type 3", "Property 3", isElement);
        var result3 = await ContentTypeEditingService.CreateAsync(contentType3, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                null,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>(),
                isElement);

        Assert.AreEqual(3, availableCompositions.Count());

        // Verify that all content types are present in available compositions
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result1.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result2.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result3.Result!.Key && compositionsResult.Allowed));
    }

    [Test]
    public async Task Available_Compositions_For_Element_Types_Are_Only_Element_Types()
    {
        var elementType1 = CreateBasicContentTypeModelWithSingleProperty("Element type 1", "Property 1", true);
        var result1 = await ContentTypeEditingService.CreateAsync(elementType1, Constants.Security.SuperUserKey);

        var elementType2 = CreateBasicContentTypeModelWithSingleProperty("Element type 2", "Property 2", true);
        var result2 = await ContentTypeEditingService.CreateAsync(elementType2, Constants.Security.SuperUserKey);

        var elementType3 = CreateBasicContentTypeModelWithSingleProperty("Element type 3", "Property 3", true);
        var result3 = await ContentTypeEditingService.CreateAsync(elementType3, Constants.Security.SuperUserKey);

        var contentType = CreateBasicContentTypeModelWithSingleProperty("Content type", "Property 4", false);
        await ContentTypeEditingService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                result1.Result!.Key,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>(),
                true);

        Assert.AreEqual(2, availableCompositions.Count());

        // Verify that only element types are present in available compositions
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result2.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result3.Result!.Key && compositionsResult.Allowed));
    }

    [Test]
    public async Task Available_Compositions_For_Content_Types_Can_Be_Element_Types()
    {
        var elementType1 = CreateBasicContentTypeModelWithSingleProperty("Element type 1", "Property 1", true);
        var result1 = await ContentTypeEditingService.CreateAsync(elementType1, Constants.Security.SuperUserKey);

        var elementType2 = CreateBasicContentTypeModelWithSingleProperty("Element type 2", "Property 2", true);
        var result2 = await ContentTypeEditingService.CreateAsync(elementType2, Constants.Security.SuperUserKey);

        var elementType3 = CreateBasicContentTypeModelWithSingleProperty("Element type 3", "Property 3", true);
        var result3 = await ContentTypeEditingService.CreateAsync(elementType3, Constants.Security.SuperUserKey);

        var contentType1 = CreateBasicContentTypeModelWithSingleProperty("Content type 1", "Property 4", false);
        var result4 = await ContentTypeEditingService.CreateAsync(contentType1, Constants.Security.SuperUserKey);

        var contentType2 = CreateBasicContentTypeModelWithSingleProperty("Content type 2", "Property 5", false);
        var result5 = await ContentTypeEditingService.CreateAsync(contentType2, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                result4.Result!.Key,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>(),
                false);

        Assert.AreEqual(4, availableCompositions.Count());

        // Verify that the rest of content types (element types included) are present in available compositions
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result1.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result2.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result3.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result5.Result!.Key && compositionsResult.Allowed));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Content_Type_Picked_As_Composition_Is_Available_But_Not_Allowed(bool isElement)
    {
        var contentType1 = CreateBasicContentTypeModelWithSingleProperty("Content type 1", "Property 1", isElement);
        var result1 = await ContentTypeEditingService.CreateAsync(contentType1, Constants.Security.SuperUserKey);

        var contentType2 = CreateBasicContentTypeModelWithSingleProperty("Content type 2", "Property 2", isElement);
        var result2 = await ContentTypeEditingService.CreateAsync(contentType2, Constants.Security.SuperUserKey);

        var contentType3 = CreateBasicContentTypeModelWithSingleProperty("Content type 3", "Property 3", isElement);
        var result3 = await ContentTypeEditingService.CreateAsync(contentType3, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                null,
                new[] { result2.Result!.Key },
                Enumerable.Empty<string>(),
                isElement);

        // Verify that all content types are present in available compositions
        Assert.AreEqual(3, availableCompositions.Count());

        // Verify that contentType2 is not allowed because it is selected as composition already, so cannot be picked again
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result1.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result2.Result!.Key && compositionsResult.Allowed == false));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result3.Result!.Key && compositionsResult.Allowed));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Content_Type_With_Matching_Composition_Property_Is_Available_But_Not_Allowed(bool isElement)
    {
        var contentType1 = CreateBasicContentTypeModelWithSingleProperty("Content type 1", "Property 1", isElement);
        var result1 = await ContentTypeEditingService.CreateAsync(contentType1, Constants.Security.SuperUserKey);

        var contentType2 = CreateBasicContentTypeModelWithSingleProperty("Content type 2", "Property 2", isElement);
        var result2 = await ContentTypeEditingService.CreateAsync(contentType2, Constants.Security.SuperUserKey);

        var contentType3 = CreateBasicContentTypeModelWithSingleProperty("Content type 3", "Property 3", isElement);
        var result3 = await ContentTypeEditingService.CreateAsync(contentType3, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                null,
                Enumerable.Empty<Guid>(),
                new[] { "property3" },
                isElement);

        // Verify that all content types are present in available compositions
        Assert.AreEqual(3, availableCompositions.Count());

        // Verify that contentType3 is not allowed because it has a matching property with the current state of the item we are using to look
        // for available compositions
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result1.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result2.Result!.Key && compositionsResult.Allowed));
        Assert.IsTrue(availableCompositions.Any(compositionsResult =>
            compositionsResult.Composition.Key == result3.Result!.Key && compositionsResult.Allowed == false));
    }

    [Test]
    public async Task Parent_With_Inheriting_Child_Has_Available_Compositions()
    {
        var parent = CreateBasicContentTypeModelWithSingleProperty("Parent", "parentProperty", false);
        var parentResult = await ContentTypeEditingService.CreateAsync(parent, Constants.Security.SuperUserKey);

        var child = ContentTypeCreateModel(
            "Child",
            compositions: [new Composition { CompositionType = CompositionType.Inheritance, Key = parentResult.Result!.Key }]);
        var childResult = await ContentTypeEditingService.CreateAsync(child, Constants.Security.SuperUserKey);

        var other = CreateBasicContentTypeModelWithSingleProperty("Other", "otherProperty", false);
        var otherResult = await ContentTypeEditingService.CreateAsync(other, Constants.Security.SuperUserKey);

        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions =
            await ContentTypeEditingService.GetAvailableCompositionsAsync(
                parentResult.Result!.Key,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<string>(),
                false);

        // Being inherited from by "Child" must not lock "Parent" out of having its own compositions
        Assert.IsTrue(availableCompositions.Any(x => x.Composition.Key == otherResult.Result!.Key && x.Allowed));

        // The inheriting child is never offered as a composition of its own parent
        Assert.IsFalse(availableCompositions.Any(x => x.Composition.Key == childResult.Result!.Key));
    }

    private ContentTypeCreateModel CreateBasicContentTypeModelWithSingleProperty(string contentTypeName, string propertyName, bool isElement)
    {
        var container = ContentTypePropertyContainerModel();
        var propertyType = ContentTypePropertyTypeModel(propertyName, containerKey: container.Key);
        var contentType = ContentTypeCreateModel(
            contentTypeName,
            isElement: isElement,
            propertyTypes: new[] { propertyType },
            containers: new[] { container });

        return contentType;
    }
}
