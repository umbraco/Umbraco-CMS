using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[TestOf(typeof(TimeOnlyPropertyIndexValueFactory))]
public class TimeOnlyPropertyIndexValueFactoryTests
{
    private static readonly IJsonSerializer _jsonSerializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    [Test]
    public void GetIndexValues_ReturnsEmptyValues_ForNullPropertyValue()
    {
        var propertyMock = new Mock<IProperty>(MockBehavior.Strict);
        propertyMock.SetupGet(x => x.Alias)
            .Returns("testAlias");
        propertyMock.Setup(x => x.GetValue("en-US", null, true))
            .Returns(null);
        var factory = new TimeOnlyPropertyIndexValueFactory(_jsonSerializer, Mock.Of<ILogger<TimeOnlyPropertyIndexValueFactory>>());

        var result = factory.GetIndexValues(
            propertyMock.Object,
            "en-US",
            null,
            true,
            [],
            new Dictionary<Guid, IContentType>())
            .ToList();

        Assert.AreEqual(1, result.Count);
        var indexValue = result.First();
        Assert.AreEqual(indexValue.FieldName, "testAlias");
        Assert.IsEmpty(indexValue.Values);
    }

    [Test]
    public void GetIndexValues_ReturnsFormattedDateTime()
    {
        var propertyMock = new Mock<IProperty>(MockBehavior.Strict);
        propertyMock.SetupGet(x => x.Alias)
            .Returns("testAlias");
        propertyMock.Setup(x => x.GetValue("en-US", null, true))
            .Returns("{\"date\":\"2023-01-18T12:00:00+01:00\",\"timeZone\":\"Europe/Copenhagen\"}");

        var factory = new TimeOnlyPropertyIndexValueFactory(_jsonSerializer, Mock.Of<ILogger<TimeOnlyPropertyIndexValueFactory>>());

        var result = factory.GetIndexValues(
            propertyMock.Object,
            "en-US",
            null,
            true,
            [],
            new Dictionary<Guid, IContentType>())
            .ToList();

        Assert.AreEqual(1, result.Count);
        var indexValue = result.First();
        Assert.AreEqual(indexValue.FieldName, "testAlias");
        Assert.AreEqual(1, indexValue.Values.Count());
        var value = indexValue.Values.First();
        Assert.AreEqual("11:00:00", value);
    }
}
