using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[TestOf(typeof(DateTimeUnspecifiedPropertyIndexValueFactory))]
public class DateTimeUnspecifiedPropertyIndexValueFactoryTests
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
        var factory = new DateTimeUnspecifiedPropertyIndexValueFactory(_jsonSerializer, Mock.Of<ILogger<DateTimeUnspecifiedPropertyIndexValueFactory>>());

        var result = factory.GetIndexValues(
            propertyMock.Object,
            "en-US",
            null,
            true,
            [],
            new Dictionary<Guid, IContentType>())
            .ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        var indexValue = result.First();
        Assert.That(indexValue.FieldName, Is.EqualTo("testAlias"));
        Assert.That(indexValue.Values, Is.Empty);
    }

    [Test]
    public void GetIndexValues_ReturnsFormattedDateTime()
    {
        var propertyMock = new Mock<IProperty>(MockBehavior.Strict);
        propertyMock.SetupGet(x => x.Alias)
            .Returns("testAlias");
        propertyMock.Setup(x => x.GetValue("en-US", null, true))
            .Returns("{\"date\":\"2023-01-18T12:00:00+01:00\",\"timeZone\":\"Europe/Copenhagen\"}");

        var factory = new DateTimeUnspecifiedPropertyIndexValueFactory(_jsonSerializer, Mock.Of<ILogger<DateTimeUnspecifiedPropertyIndexValueFactory>>());

        var result = factory.GetIndexValues(
            propertyMock.Object,
            "en-US",
            null,
            true,
            [],
            new Dictionary<Guid, IContentType>())
            .ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        var indexValue = result.First();
        Assert.That(indexValue.FieldName, Is.EqualTo("testAlias"));
        Assert.That(indexValue.Values.Count(), Is.EqualTo(1));
        var value = indexValue.Values.First();
        Assert.That(value, Is.EqualTo("2023-01-18T11:00:00"));
    }
}
