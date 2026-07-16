using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Serialization;

[TestFixture]
public class JsonTolerantNumberConverterFactoryTests
{
    private readonly IConfigurationEditorJsonSerializer _serializer =
        new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    [TestCase("\"\"")]
    [TestCase("\" \"")]
    [TestCase("null")]
    public void Can_Deserialize_Empty_Int_Configuration_As_Zero(string minMaxValue)
    {
        // Legacy (pre-v14) databases stored the picker min/max as empty strings; deserialization must not throw.
        var json = $"{{\"ignoreUserStartNodes\":false,\"minNumber\":{minMaxValue},\"maxNumber\":{minMaxValue}}}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, configuration.MinNumber);
            Assert.AreEqual(0, configuration.MaxNumber);
        });
    }

    [Test]
    public void Can_Deserialize_Empty_Int_Configuration_On_MultiNodePicker_As_Zero()
    {
        var json = "{\"minNumber\":\"\",\"maxNumber\":\"\",\"filter\":\"\"}";

        MultiNodePickerConfiguration? configuration =
            _serializer.Deserialize<MultiNodePickerConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, configuration.MinNumber);
            Assert.AreEqual(0, configuration.MaxNumber);
        });
    }

    [Test]
    public void Can_Deserialize_Empty_Decimal_Configuration_As_Zero()
    {
        var json = "{\"minVal\":\"\",\"maxVal\":\"\",\"step\":\"\",\"minimumRange\":\"\"}";

        SliderConfiguration? configuration = _serializer.Deserialize<SliderConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0m, configuration.MinimumValue);
            Assert.AreEqual(0m, configuration.MaximumValue);
            Assert.AreEqual(0m, configuration.Step);
        });
    }

    [Test]
    public void Can_Deserialize_Empty_Nullable_Int_Configuration_As_Zero()
    {
        var json = "{\"maxChars\":\"\"}";

        TextAreaConfiguration? configuration = _serializer.Deserialize<TextAreaConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.AreEqual(0, configuration.MaxChars);
    }

    [TestCase("\"5\"", 5)]
    [TestCase("5", 5)]
    public void Can_Deserialize_Valid_Number(string minNumberValue, int expected)
    {
        var json = $"{{\"minNumber\":{minNumberValue},\"maxNumber\":10}}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expected, configuration.MinNumber);
            Assert.AreEqual(10, configuration.MaxNumber);
        });
    }

    [Test]
    public void Can_Serialize_Number_As_Numeric_Value()
    {
        var configuration = new MultiUrlPickerConfiguration { MinNumber = 2, MaxNumber = 5 };

        var serialized = _serializer.Serialize(configuration);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(serialized.Contains("\"minNumber\":2"), serialized);
            Assert.IsTrue(serialized.Contains("\"maxNumber\":5"), serialized);
        });
    }
}
