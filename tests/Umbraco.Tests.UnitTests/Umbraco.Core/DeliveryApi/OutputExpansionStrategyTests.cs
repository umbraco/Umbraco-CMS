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
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var contentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPicker2Value = CreateSimplePickedContent(666, 777);

        var content = new Mock<IPublishedContent>();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, 444, "number"),
            CreateElementProperty(content.Object, "element", 333, contentPickerValue.Key, "contentPicker", apiContentBuilder, apiElementBuilder),
            CreateElementProperty(content.Object, "element2", 555, contentPicker2Value.Key, "contentPicker", apiContentBuilder, apiElementBuilder));

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(3, result.Properties.Count);
        Assert.AreEqual(444, result.Properties["number"]);

        var elementOutput = result.Properties["element"] as IApiElement;
        Assert.IsNotNull(elementOutput);
        Assert.AreEqual(2, elementOutput.Properties.Count);
        Assert.AreEqual(333, elementOutput.Properties["number"]);
        var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(contentPickerOutput);
        Assert.AreEqual(contentPickerValue.Key, contentPickerOutput.Id);
        Assert.AreEqual(2, contentPickerOutput.Properties.Count);
        Assert.AreEqual(111, contentPickerOutput.Properties["numberOne"]);
        Assert.AreEqual(222, contentPickerOutput.Properties["numberTwo"]);

        elementOutput = result.Properties["element2"] as IApiElement;
        Assert.IsNotNull(elementOutput);
        Assert.AreEqual(2, elementOutput.Properties.Count);
        Assert.AreEqual(555, elementOutput.Properties["number"]);
        contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(contentPickerOutput);
        Assert.AreEqual(contentPicker2Value.Key, contentPickerOutput.Id);
        Assert.AreEqual(0, contentPickerOutput.Properties.Count);
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandAllElements()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(true );
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var contentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPicker2Value = CreateSimplePickedContent(666, 777);

        var content = new Mock<IPublishedContent>();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, 444, "number"),
            CreateElementProperty(content.Object, "element", 333, contentPickerValue.Key, "contentPicker", apiContentBuilder, apiElementBuilder),
            CreateElementProperty(content.Object, "element2", 555, contentPicker2Value.Key, "contentPicker", apiContentBuilder, apiElementBuilder));

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(3, result.Properties.Count);
        Assert.AreEqual(444, result.Properties["number"]);

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
            Assert.IsNotNull(elementOutput);
            Assert.AreEqual(2, elementOutput.Properties.Count);
            Assert.AreEqual(expectedElementOutput.ElementNumber, elementOutput.Properties["number"]);
            var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
            Assert.IsNotNull(contentPickerOutput);
            Assert.AreEqual(expectedElementOutput.ElementContentPicker, contentPickerOutput.Id);
            Assert.AreEqual(2, contentPickerOutput.Properties.Count);
            Assert.AreEqual(expectedElementOutput.ContentNumberOne, contentPickerOutput.Properties["numberOne"]);
            Assert.AreEqual(expectedElementOutput.ContentNumberTwo, contentPickerOutput.Properties["numberTwo"]);
        }
    }

    [Test]
    public void OutputExpansionStrategy_DoesNotExpandElementNestedContentPicker()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "element" });
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var nestedContentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPickerValue = CreateMultiLevelPickedContent(987, nestedContentPickerValue, "contentPicker", apiContentBuilder);

        var content = new Mock<IPublishedContent>();
        SetupContentMock(content, CreateElementProperty(content.Object, "element", 333, contentPickerValue.Key, "contentPicker", apiContentBuilder, apiElementBuilder));

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(1, result.Properties.Count);

        var elementOutput = result.Properties["element"] as IApiElement;
        Assert.IsNotNull(elementOutput);
        Assert.AreEqual(2, elementOutput.Properties.Count);
        Assert.AreEqual(333, elementOutput.Properties["number"]);
        var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(contentPickerOutput);
        Assert.AreEqual(contentPickerValue.Key, contentPickerOutput.Id);
        Assert.AreEqual(2, contentPickerOutput.Properties.Count);
        Assert.AreEqual(987, contentPickerOutput.Properties["number"]);
        var nestedContentPickerOutput = contentPickerOutput.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(nestedContentPickerOutput);
        Assert.AreEqual(nestedContentPickerValue.Key, nestedContentPickerOutput.Id);
        Assert.AreEqual(0, nestedContentPickerOutput.Properties.Count);
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
