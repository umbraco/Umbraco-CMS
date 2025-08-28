using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.PropertyEditors;

[TestFixture]
[TestOf(typeof(DateTime2PropertyIndexValueFactory))]
public class DateTime2PropertyIndexValueFactoryTest
{
    private readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private readonly Mock<IDataTypeConfigurationCache> _dataTypeConfigurationCache = new(MockBehavior.Strict);

    [Test]
    public void GetIndexValues_ReturnsEmptyValues_ForNullPropertyValue()
    {
        var propertyMock = new Mock<IProperty>(MockBehavior.Strict);
        propertyMock.SetupGet(x => x.Alias)
            .Returns("testAlias");
        propertyMock.Setup(x => x.GetValue("en-US", null, true))
            .Returns(null);
        var factory = new DateTime2PropertyIndexValueFactory(_dataTypeConfigurationCache.Object, _jsonSerializer);

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

    [TestCase(DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.All, "{\"date\":\"2023-01-18T12:00:00+01:00\",\"timeZone\":\"Europe/Copenhagen\"}", "2023-01-18T11:00:00.0000000Z")]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.None, "{\"date\":\"2023-01-18T12:00:00Z\",\"timeZone\":\"Europe/Copenhagen\"}", "2023-01-18T12:00:00.0000000")]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.None, "{\"date\":\"2023-01-18T00:00:00Z\",\"timeZone\":null}", "2023-01-18")]
    [TestCase(DateTime2Configuration.DateTimeFormat.TimeOnly, DateTime2Configuration.TimeZoneMode.None, "{\"date\":\"0001-01-01T12:00:00Z\",\"timeZone\":null}", "12:00:00.0000000")]
    public void GetIndexValues_ReturnsFormattedUtcDateTime(DateTime2Configuration.DateTimeFormat format, DateTime2Configuration.TimeZoneMode mode, string propertyValue, string expectedIndexValue)
    {
        var dataTypeKey = Guid.NewGuid();
        var propertyMock = new Mock<IProperty>(MockBehavior.Strict);
        propertyMock.SetupGet(x => x.Alias)
            .Returns("testAlias");
        propertyMock.SetupGet(x => x.PropertyType)
            .Returns(Mock.Of<IPropertyType>(x => x.DataTypeKey == dataTypeKey));
        propertyMock.Setup(x => x.GetValue("en-US", null, true))
            .Returns(propertyValue);

        var configuration = new DateTime2Configuration
        {
            Format = format,
            TimeZones = new DateTime2Configuration.TimeZonesConfiguration { Mode = mode },
        };
        _dataTypeConfigurationCache.Setup(x => x.GetConfigurationAs<DateTime2Configuration>(dataTypeKey))
            .Returns(configuration);

        var factory = new DateTime2PropertyIndexValueFactory(_dataTypeConfigurationCache.Object, _jsonSerializer);

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
        Assert.AreEqual(expectedIndexValue, value);
    }
}
