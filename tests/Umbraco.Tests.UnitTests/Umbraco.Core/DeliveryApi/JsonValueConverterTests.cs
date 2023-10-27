using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class JsonValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void JsonValueConverterTests_ConvertsCustomPropertyWithValueTypeJson()
    {
        var valueEditor = Mock.Of<IDataValueEditor>(x => x.ValueType == ValueTypes.Json);
        var dataEditor = Mock.Of<IDataEditor>(x => x.GetValueEditor() == valueEditor);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => new[] { dataEditor }));
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == "My.Custom.Json");

        var valueConverter = new JsonValueConverter(propertyEditors, Mock.Of<ILogger<JsonValueConverter>>());
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), propertyType, "{\"message\": \"Hello, JSON\"}", false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), propertyType, PropertyCacheLevel.Element, inter, false, false);
        Assert.IsTrue(result is JsonNode);
        JsonNode jsonNode = (JsonNode)result;
        Assert.AreEqual("Hello, JSON", jsonNode["message"]!.GetValue<string>());
    }
}
