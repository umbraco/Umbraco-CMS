using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.PropertyEditors;

[TestFixture]
[TestOf(typeof(DateTime2PropertyIndexValueFactory))]
public class DateTime2PropertyIndexValueFactoryTest
{
    private static readonly IJsonSerializer _jsonSerializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
    private static readonly Mock<DateTime2ValueConverterBase> _valueConverter = new(MockBehavior.Strict, _jsonSerializer);

    [Test]
    public void GetIndexValues_ReturnsEmptyValues_ForNullPropertyValue()
    {
        var propertyMock = new Mock<IProperty>(MockBehavior.Strict);
        propertyMock.SetupGet(x => x.Alias)
            .Returns("testAlias");
        propertyMock.Setup(x => x.GetValue("en-US", null, true))
            .Returns(null);
        var factory = new DateTime2PropertyIndexValueFactory(_valueConverter.Object);

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
    public void GetIndexValues_ReturnsFormattedUtcDateTime()
    {
        var dataTypeKey = Guid.NewGuid();
        var propertyMock = new Mock<IProperty>(MockBehavior.Strict);
        propertyMock.SetupGet(x => x.Alias)
            .Returns("testAlias");
        propertyMock.SetupGet(x => x.PropertyType)
            .Returns(Mock.Of<IPropertyType>(x => x.DataTypeKey == dataTypeKey));
        propertyMock.Setup(x => x.GetValue("en-US", null, true))
            .Returns("{\"date\":\"2023-01-18T12:00:00+01:00\",\"timeZone\":\"Europe/Copenhagen\"}");

        var dto = new DateTime2ValueConverterBase.DateTime2Dto
        {
            Date = DateTimeOffset.Parse("2023-01-18T12:00:00+01:00"), TimeZone = "Europe/Copenhagen"
        };

        _valueConverter.Setup(x => x.ConvertToObject(It.IsAny<DateTime2ValueConverterBase.DateTime2Dto>()))
            .Returns(dto.Date)
            .Verifiable(Times.Once);

        var factory = new DateTime2PropertyIndexValueFactory(_valueConverter.Object);

        var result = factory.GetIndexValues(
            propertyMock.Object,
            "en-US",
            null,
            true,
            [],
            new Dictionary<Guid, IContentType>())
            .ToList();

        _valueConverter.VerifyAll();

        Assert.AreEqual(1, result.Count);
        var indexValue = result.First();
        Assert.AreEqual(indexValue.FieldName, "testAlias");
        Assert.AreEqual(1, indexValue.Values.Count());
        var value = indexValue.Values.First();
        Assert.AreEqual("2023-01-18T11:00:00.0000000", value);
    }
}
