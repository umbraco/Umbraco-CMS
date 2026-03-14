using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="TimeOnlyPropertyIndexValueFactory"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
[TestOf(typeof(TimeOnlyPropertyIndexValueFactory))]
public class TimeOnlyPropertyIndexValueFactoryTests
{
    private static readonly IJsonSerializer _jsonSerializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    /// <summary>
    /// Tests that GetIndexValues returns empty values when the property value is null.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>GetIndexValues</c> returns the expected formatted time string ("HH:mm:ss")
    /// after converting the provided date and time zone information to UTC, ensuring correct time zone handling.
    /// </summary>
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
