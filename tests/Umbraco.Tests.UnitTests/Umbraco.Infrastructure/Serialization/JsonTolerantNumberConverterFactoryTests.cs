using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Serialization;

[TestFixture]
public class JsonTolerantNumberConverterFactoryTests
{
    private readonly SystemTextConfigurationEditorJsonSerializer _serializer =
        new(new DefaultJsonSerializerEncoderFactory());

    [TestCase("\"\"")]
    [TestCase("\" \"")]
    [TestCase("null")]
    public void Can_Deserialize_Empty_Int_Configuration_As_Zero(string minMaxValue)
    {
        // Legacy (pre-v14) databases stored the picker min/max as empty strings; deserialization must not throw.
        var json = $"{{\"ignoreUserStartNodes\":false,\"minNumber\":{minMaxValue},\"maxNumber\":{minMaxValue}}}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.That(configuration, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(configuration.MinNumber, Is.EqualTo(0));
            Assert.That(configuration.MaxNumber, Is.EqualTo(0));
        });
    }

    [Test]
    public void Can_Deserialize_Empty_Int_Configuration_On_MultiNodePicker_As_Zero()
    {
        var json = "{\"minNumber\":\"\",\"maxNumber\":\"\",\"filter\":\"\"}";

        MultiNodePickerConfiguration? configuration =
            _serializer.Deserialize<MultiNodePickerConfiguration>(json);

        Assert.That(configuration, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(configuration.MinNumber, Is.EqualTo(0));
            Assert.That(configuration.MaxNumber, Is.EqualTo(0));
        });
    }

    [Test]
    public void Can_Deserialize_Empty_Decimal_Configuration_As_Zero()
    {
        var json = "{\"minVal\":\"\",\"maxVal\":\"\",\"step\":\"\",\"minimumRange\":\"\"}";

        SliderConfiguration? configuration = _serializer.Deserialize<SliderConfiguration>(json);

        Assert.That(configuration, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(configuration.MinimumValue, Is.EqualTo(0m));
            Assert.That(configuration.MaximumValue, Is.EqualTo(0m));
            Assert.That(configuration.Step, Is.EqualTo(0m));
        });
    }

    [Test]
    public void Can_Deserialize_Empty_Nullable_Int_Configuration_As_Zero()
    {
        var json = "{\"maxChars\":\"\"}";

        TextAreaConfiguration? configuration = _serializer.Deserialize<TextAreaConfiguration>(json);

        Assert.That(configuration, Is.Not.Null);
        Assert.That(configuration.MaxChars, Is.EqualTo(0));
    }

    [TestCase("\"5\"", 5)]
    [TestCase("5", 5)]
    public void Can_Deserialize_Valid_Number(string minNumberValue, int expected)
    {
        var json = $"{{\"minNumber\":{minNumberValue},\"maxNumber\":10}}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.That(configuration, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(configuration.MinNumber, Is.EqualTo(expected));
            Assert.That(configuration.MaxNumber, Is.EqualTo(10));
        });
    }

    [Test]
    public void Can_Deserialize_Fractional_Number_For_Integer_Field_As_Zero()
    {
        // A fractional JSON number cannot represent an int, so it must resolve to the default rather than
        // being silently truncated (e.g. 1.5 -> 1).
        var json = "{\"minNumber\":1.5,\"maxNumber\":10}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.That(configuration, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(configuration.MinNumber, Is.EqualTo(0));
            Assert.That(configuration.MaxNumber, Is.EqualTo(10));
        });
    }

    [Test]
    public void Can_Deserialize_Fractional_Number_For_Decimal_Field()
    {
        // Floating-point config must retain its fractional value.
        var json = "{\"step\":0.5,\"minVal\":1,\"maxVal\":10}";

        SliderConfiguration? configuration = _serializer.Deserialize<SliderConfiguration>(json);

        Assert.That(configuration, Is.Not.Null);
        Assert.That(configuration.Step, Is.EqualTo(0.5m));
    }

    [Test]
    public void Can_Serialize_Number_As_Numeric_Value()
    {
        var configuration = new MultiUrlPickerConfiguration { MinNumber = 2, MaxNumber = 5 };

        var serialized = _serializer.Serialize(configuration);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain("\"minNumber\":2"), serialized);
            Assert.That(serialized, Does.Contain("\"maxNumber\":5"), serialized);
        });
    }
}
