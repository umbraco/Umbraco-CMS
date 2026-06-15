using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Rendering;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

/// <summary>
/// Any tests contained within this class specifically test property expansion V2 (and field limiting) - not V1. If the
/// aim is to test expansion for both versions, please put the tests in the base class.
/// </summary>
[TestFixture]
public class OutputExpansionStrategyV2Tests : OutputExpansionStrategyTestBase
{
    [TestCase("contentPicker", "contentPicker")]
    [TestCase("rootPicker", "nestedPicker")]
    public void OutputExpansionStrategy_CanExpandNestedContentPicker(string rootPropertyTypeAlias, string nestedPropertyTypeAlias)
    {
        var accessor = CreateOutputExpansionStrategyAccessor($"properties[{rootPropertyTypeAlias}[properties[{nestedPropertyTypeAlias}]]]");
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor, CreateVariationContextAccessor());

        var content = CreatePublishedContentMock();

        var nestedContentPickerContent = CreateSimplePickedContent(987, 654);
        var contentPickerContent = CreateMultiLevelPickedContent(123, nestedContentPickerContent, nestedPropertyTypeAlias, apiContentBuilder);
        var contentPickerContentProperty = CreateContentPickerProperty(content.Object, contentPickerContent.Key, rootPropertyTypeAlias, apiContentBuilder);

        SetupContentMock(content, contentPickerContentProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.That(result.Properties, Has.Count.EqualTo(1));

        var contentPickerOneOutput = result.Properties[rootPropertyTypeAlias] as ApiContent;
        Assert.That(contentPickerOneOutput, Is.Not.Null);
        Assert.That(contentPickerOneOutput.Id, Is.EqualTo(contentPickerContent.Key));
        Assert.That(contentPickerOneOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(contentPickerOneOutput.Properties["number"], Is.EqualTo(123));

        var nestedContentPickerOutput = contentPickerOneOutput.Properties[nestedPropertyTypeAlias] as ApiContent;
        Assert.That(nestedContentPickerOutput, Is.Not.Null);
        Assert.That(nestedContentPickerOutput.Id, Is.EqualTo(nestedContentPickerContent.Key));
        Assert.That(nestedContentPickerOutput.Properties, Is.Not.Empty);
        Assert.That(nestedContentPickerOutput.Properties["numberOne"], Is.EqualTo(987));
        Assert.That(nestedContentPickerOutput.Properties["numberTwo"], Is.EqualTo(654));
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandSpecifiedElement()
    {
        // var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "element" });
        var accessor = CreateOutputExpansionStrategyAccessor("properties[element[properties[$all]]]");
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
        var accessor = CreateOutputExpansionStrategyAccessor("properties[element[properties[$all]],element2[properties[$all]]]" );
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
        var accessor = CreateOutputExpansionStrategyAccessor("properties[element[properties[contentPicker]]]" );
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

    [Test]
    public void OutputExpansionStrategy_CanExpandElementNestedContentPicker()
    {
        var accessor = CreateOutputExpansionStrategyAccessor("properties[element[properties[contentPicker[properties[nestedContentPicker]]]]]");
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor, CreateVariationContextAccessor());
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var nestedContentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPickerValue = CreateMultiLevelPickedContent(987, nestedContentPickerValue, "nestedContentPicker", apiContentBuilder);

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
        var nestedContentPickerOutput = contentPickerOutput.Properties["nestedContentPicker"] as IApiContent;
        Assert.That(nestedContentPickerOutput, Is.Not.Null);
        Assert.That(nestedContentPickerOutput.Id, Is.EqualTo(nestedContentPickerValue.Key));
        Assert.That(nestedContentPickerOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(nestedContentPickerOutput.Properties["numberOne"], Is.EqualTo(111));
        Assert.That(nestedContentPickerOutput.Properties["numberTwo"], Is.EqualTo(222));
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandContentPickerBeyondTwoLevels()
    {
        var accessor = CreateOutputExpansionStrategyAccessor($"properties[level1Picker[properties[level2Picker[properties[level3Picker[properties[level4Picker]]]]]]]");
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor, CreateVariationContextAccessor());

        var content = CreatePublishedContentMock();

        var level5PickedContent = CreateSimplePickedContent(1234, 5678);
        var level4PickedContent = CreateMultiLevelPickedContent(444, level5PickedContent, "level4Picker", apiContentBuilder);
        var level3PickedContent = CreateMultiLevelPickedContent(333, level4PickedContent, "level3Picker", apiContentBuilder);
        var level2PickedContent = CreateMultiLevelPickedContent(222, level3PickedContent, "level2Picker", apiContentBuilder);
        var contentPickerContentProperty = CreateContentPickerProperty(content.Object, level2PickedContent.Key, "level1Picker", apiContentBuilder);

        SetupContentMock(content, contentPickerContentProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.That(result.Properties, Has.Count.EqualTo(1));

        var level1PickerOutput = result.Properties["level1Picker"] as ApiContent;
        Assert.That(level1PickerOutput, Is.Not.Null);
        Assert.That(level1PickerOutput.Id, Is.EqualTo(level2PickedContent.Key));
        Assert.That(level1PickerOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(level1PickerOutput.Properties["number"], Is.EqualTo(222));

        var level2PickerOutput = level1PickerOutput.Properties["level2Picker"] as ApiContent;
        Assert.That(level2PickerOutput, Is.Not.Null);
        Assert.That(level2PickerOutput.Id, Is.EqualTo(level3PickedContent.Key));
        Assert.That(level2PickerOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(level2PickerOutput.Properties["number"], Is.EqualTo(333));

        var level3PickerOutput = level2PickerOutput.Properties["level3Picker"] as ApiContent;
        Assert.That(level3PickerOutput, Is.Not.Null);
        Assert.That(level3PickerOutput.Id, Is.EqualTo(level4PickedContent.Key));
        Assert.That(level3PickerOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(level3PickerOutput.Properties["number"], Is.EqualTo(444));

        var level4PickerOutput = level3PickerOutput.Properties["level4Picker"] as ApiContent;
        Assert.That(level4PickerOutput, Is.Not.Null);
        Assert.That(level4PickerOutput.Id, Is.EqualTo(level5PickedContent.Key));
        Assert.That(level4PickerOutput.Properties, Has.Count.EqualTo(2));
        Assert.That(level4PickerOutput.Properties["numberOne"], Is.EqualTo(1234));
        Assert.That(level4PickerOutput.Properties["numberTwo"], Is.EqualTo(5678));
    }

    [TestCase("numberOne")]
    [TestCase("numberTwo")]
    public void OutputExpansionStrategy_CanLimitDirectFields(string includedField)
    {
        var accessor = CreateOutputExpansionStrategyAccessor(fields: $"properties[{includedField}]");
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor, CreateVariationContextAccessor());

        var content = CreateSimplePickedContent(123, 456);

        var result = apiContentBuilder.Build(content);

        Assert.That(result.Properties, Has.Count.EqualTo(1));
        Assert.That(result.Properties.ContainsKey(includedField), Is.True);
        Assert.That(result.Properties[includedField], Is.EqualTo(includedField is "numberOne" ? 123 : 456));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OutputExpansionStrategy_CanLimitFieldsOfExpandedContent(bool expand)
    {
        var accessor = CreateOutputExpansionStrategyAccessor(expand ? "properties[$all]" : null, "properties[contentPickerOne[properties[numberOne]],contentPickerTwo[properties[numberTwo]]]");
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor, CreateVariationContextAccessor());

        var content = CreatePublishedContentMock();

        var contentPickerOneContent = CreateSimplePickedContent(12, 34);
        var contentPickerOneProperty = CreateContentPickerProperty(content.Object, contentPickerOneContent.Key, "contentPickerOne", apiContentBuilder);
        var contentPickerTwoContent = CreateSimplePickedContent(56, 78);
        var contentPickerTwoProperty = CreateContentPickerProperty(content.Object, contentPickerTwoContent.Key, "contentPickerTwo", apiContentBuilder);

        SetupContentMock(content, contentPickerOneProperty, contentPickerTwoProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.That(result.Properties, Has.Count.EqualTo(2));

        var contentPickerOneOutput = result.Properties["contentPickerOne"] as ApiContent;
        Assert.That(contentPickerOneOutput, Is.Not.Null);
        Assert.That(contentPickerOneOutput.Id, Is.EqualTo(contentPickerOneContent.Key));
        // yeah we shouldn't test two things in one unit test, but given the risk of false positives when testing
        // conditional field limiting, this is preferable.
        if (expand)
        {
            Assert.That(contentPickerOneOutput.Properties, Has.Count.EqualTo(1));
            Assert.That(contentPickerOneOutput.Properties["numberOne"], Is.EqualTo(12));
        }
        else
        {
            Assert.That(contentPickerOneOutput.Properties, Is.Empty);
        }

        var contentPickerTwoOutput = result.Properties["contentPickerTwo"] as ApiContent;
        Assert.That(contentPickerTwoOutput, Is.Not.Null);
        Assert.That(contentPickerTwoOutput.Id, Is.EqualTo(contentPickerTwoContent.Key));
        if (expand)
        {
            Assert.That(contentPickerTwoOutput.Properties, Has.Count.EqualTo(1));
            Assert.That(contentPickerTwoOutput.Properties["numberTwo"], Is.EqualTo(78));
        }
        else
        {
            Assert.That(contentPickerTwoOutput.Properties, Is.Empty);
        }
    }

    protected override IOutputExpansionStrategyAccessor CreateOutputExpansionStrategyAccessor(string? expand = null, string? fields = null)
    {
        var httpContextMock = new Mock<HttpContext>();
        var httpRequestMock = new Mock<HttpRequest>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        httpRequestMock
            .SetupGet(r => r.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues> { { "expand", expand }, { "fields", fields } }));

        httpContextMock.SetupGet(c => c.Request).Returns(httpRequestMock.Object);
        httpContextAccessorMock.SetupGet(a => a.HttpContext).Returns(httpContextMock.Object);
        IOutputExpansionStrategy outputExpansionStrategy = new RequestContextOutputExpansionStrategyV2(
            httpContextAccessorMock.Object,
            new ApiPropertyRenderer(new NoopPublishedValueFallback()),
            Mock.Of<ILogger<RequestContextOutputExpansionStrategyV2>>());
        var outputExpansionStrategyAccessorMock = new Mock<IOutputExpansionStrategyAccessor>();
        outputExpansionStrategyAccessorMock.Setup(s => s.TryGetValue(out outputExpansionStrategy)).Returns(true);

        return outputExpansionStrategyAccessorMock.Object;
    }

    protected override string? FormatExpandSyntax(bool expandAll = false, string[]? expandPropertyAliases = null)
        => expandAll ? "$all" : expandPropertyAliases?.Any() is true ? $"properties[{string.Join(",", expandPropertyAliases)}]" : null;
}
