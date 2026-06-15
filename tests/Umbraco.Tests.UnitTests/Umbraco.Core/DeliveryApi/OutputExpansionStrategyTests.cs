using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Rendering;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

/// <summary>
/// Any tests contained within this class specifically test property expansion V1 and not V2. If the aim is to test both
/// versions, please put the tests in the base class.
/// </summary>
[TestFixture]
public class OutputExpansionStrategyTests : OutputExpansionStrategyTestBase
{
    [Test]
    public void OutputExpansionStrategy_CanExpandSpecifiedElement()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "element" });
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor, CreateVariationContextAccessor());
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var contentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPicker2Value = CreateSimplePickedContent(666, 777);

        var content = CreatePublishedContentMock();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, 444, "number"),
            CreateElementProperty(content.Object, "element", 333, contentPickerValue.Key, "contentPicker", apiContentBuilder, apiElementBuilder),
            CreateElementProperty(content.Object, "element2", 555, contentPicker2Value.Key, "contentPicker", apiContentBuilder, apiElementBuilder));

        var result = apiContentBuilder.Build(content.Object);

        Assert.That(result.Properties, Has.Count.EqualTo(3));
        Assert.That(result.Properties["number"], Is.EqualTo(444));

        var elementOutput = result.Properties["element"] as IApiElement;
        Assert.That(elementOutput, Is.Not.Null);
        Assert.That(elementOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(elementOutput.Properties["number"], Is.EqualTo(333));
        var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.That(contentPickerOutput, Is.Not.Null);
        Assert.That(contentPickerOutput.Id, Is.EqualTo(contentPickerValue.Key));
        Assert.That(contentPickerOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(contentPickerOutput.Properties["numberOne"], Is.EqualTo(111));
        Assert.That(contentPickerOutput.Properties["numberTwo"], Is.EqualTo(222));

        elementOutput = result.Properties["element2"] as IApiElement;
        Assert.That(elementOutput, Is.Not.Null);
        Assert.That(elementOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(elementOutput.Properties["number"], Is.EqualTo(555));
        contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.That(contentPickerOutput, Is.Not.Null);
        Assert.That(contentPickerOutput.Id, Is.EqualTo(contentPicker2Value.Key));
        Assert.That(contentPickerOutput.Properties.Count, Is.EqualTo(0));
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandAllElements()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(true );
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor, CreateVariationContextAccessor());
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var contentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPicker2Value = CreateSimplePickedContent(666, 777);

        var content = CreatePublishedContentMock();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, 444, "number"),
            CreateElementProperty(content.Object, "element", 333, contentPickerValue.Key, "contentPicker", apiContentBuilder, apiElementBuilder),
            CreateElementProperty(content.Object, "element2", 555, contentPicker2Value.Key, "contentPicker", apiContentBuilder, apiElementBuilder));

        var result = apiContentBuilder.Build(content.Object);

        Assert.That(result.Properties, Has.Count.EqualTo(3));
        Assert.That(result.Properties["number"], Is.EqualTo(444));

        var expectedElementOutputs = new[]
        {
            new
            {
                PropertyAlias = "element",
                ElementNumber = 333,
                ElementContentPicker = contentPickerValue.Key,
                ContentNumberOne = 111,
                ContentNumberTwo = 222
            },
            new
            {
                PropertyAlias = "element2",
                ElementNumber = 555,
                ElementContentPicker = contentPicker2Value.Key,
                ContentNumberOne = 666,
                ContentNumberTwo = 777
            }
        };

        foreach (var expectedElementOutput in expectedElementOutputs)
        {
            var elementOutput = result.Properties[expectedElementOutput.PropertyAlias] as IApiElement;
            Assert.That(elementOutput, Is.Not.Null);
            Assert.That(elementOutput.Properties, Has.Count.EqualTo(2));
            Assert.That(elementOutput.Properties["number"], Is.EqualTo(expectedElementOutput.ElementNumber));
            var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
            Assert.That(contentPickerOutput, Is.Not.Null);
            Assert.That(contentPickerOutput.Id, Is.EqualTo(expectedElementOutput.ElementContentPicker));
            Assert.That(contentPickerOutput.Properties, Has.Count.EqualTo(2));
            Assert.That(contentPickerOutput.Properties["numberOne"], Is.EqualTo(expectedElementOutput.ContentNumberOne));
            Assert.That(contentPickerOutput.Properties["numberTwo"], Is.EqualTo(expectedElementOutput.ContentNumberTwo));
        }
    }

    [Test]
    public void OutputExpansionStrategy_DoesNotExpandElementNestedContentPicker()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "element" });
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor, CreateVariationContextAccessor());
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var nestedContentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPickerValue = CreateMultiLevelPickedContent(987, nestedContentPickerValue, "contentPicker", apiContentBuilder);

        var content = CreatePublishedContentMock();
        SetupContentMock(content, CreateElementProperty(content.Object, "element", 333, contentPickerValue.Key, "contentPicker", apiContentBuilder, apiElementBuilder));

        var result = apiContentBuilder.Build(content.Object);

        Assert.That(result.Properties, Has.Count.EqualTo(1));

        var elementOutput = result.Properties["element"] as IApiElement;
        Assert.That(elementOutput, Is.Not.Null);
        Assert.That(elementOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(elementOutput.Properties["number"], Is.EqualTo(333));
        var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.That(contentPickerOutput, Is.Not.Null);
        Assert.That(contentPickerOutput.Id, Is.EqualTo(contentPickerValue.Key));
        Assert.That(contentPickerOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(contentPickerOutput.Properties["number"], Is.EqualTo(987));
        var nestedContentPickerOutput = contentPickerOutput.Properties["contentPicker"] as IApiContent;
        Assert.That(nestedContentPickerOutput, Is.Not.Null);
        Assert.That(nestedContentPickerOutput.Id, Is.EqualTo(nestedContentPickerValue.Key));
        Assert.That(nestedContentPickerOutput.Properties.Count, Is.EqualTo(0));
    }

    protected override IOutputExpansionStrategyAccessor CreateOutputExpansionStrategyAccessor(string? expand = null, string? fields = null)
    {
        var httpContextMock = new Mock<HttpContext>();
        var httpRequestMock = new Mock<HttpRequest>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        httpRequestMock
            .SetupGet(r => r.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues> { { "expand", expand } }));

        httpContextMock.SetupGet(c => c.Request).Returns(httpRequestMock.Object);
        httpContextAccessorMock.SetupGet(a => a.HttpContext).Returns(httpContextMock.Object);

        IOutputExpansionStrategy outputExpansionStrategy = new RequestContextOutputExpansionStrategy(httpContextAccessorMock.Object, new ApiPropertyRenderer(new NoopPublishedValueFallback()));
        var outputExpansionStrategyAccessorMock = new Mock<IOutputExpansionStrategyAccessor>();
        outputExpansionStrategyAccessorMock.Setup(s => s.TryGetValue(out outputExpansionStrategy)).Returns(true);

        return outputExpansionStrategyAccessorMock.Object;
    }

    protected override string? FormatExpandSyntax(bool expandAll = false, string[]? expandPropertyAliases = null)
        => expandAll ? "all" : expandPropertyAliases?.Any() is true ? $"property:{string.Join(",", expandPropertyAliases)}" : null;
}
