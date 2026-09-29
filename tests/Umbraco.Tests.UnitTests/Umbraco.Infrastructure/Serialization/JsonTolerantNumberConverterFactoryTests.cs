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
    public void Can_Deserialize_Empty_Int_Range_As_Zero(string minMaxValue)
    {
        // Legacy databases stored the picker min/max as empty strings; deserialization must not throw and
        // an empty string bound resolves to the default (zero).
        var json = $"{{\"ignoreUserStartNodes\":false,\"validationLimit\":{{\"min\":{minMaxValue},\"max\":{minMaxValue}}}}}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.That(configuration!.ValidationLimit.Min, Is.EqualTo(0));
            Assert.That(configuration.ValidationLimit.Max, Is.EqualTo(0));
        });
    }

    [Test]
    public void Can_Deserialize_Null_Int_Range_As_Unbounded()
    {
        // An explicit JSON null resolves to a null (unbounded) nullable bound.
        var json = "{\"validationLimit\":{\"min\":null,\"max\":null}}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.That(configuration!.ValidationLimit.Min, Is.Null);
            Assert.That(configuration.ValidationLimit.Max, Is.Null);
        });
    }

    [Test]
    public void Can_Deserialize_Empty_Int_Range_On_MultiNodePicker_As_Zero()
    {
        var json = "{\"validationLimit\":{\"min\":\"\",\"max\":\"\"},\"filter\":\"\"}";

        MultiNodePickerConfiguration? configuration =
            _serializer.Deserialize<MultiNodePickerConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.That(configuration!.ValidationLimit.Min, Is.EqualTo(0));
            Assert.That(configuration.ValidationLimit.Max, Is.EqualTo(0));
        });
    }

    [Test]
    public void Can_Deserialize_Empty_Decimal_Configuration_As_Zero()
    {
        var json = "{\"validationRange\":{\"min\":\"\",\"max\":\"\"},\"step\":\"\",\"minimumRange\":\"\"}";

        SliderConfiguration? configuration = _serializer.Deserialize<SliderConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.That(configuration!.ValidationRange.Min, Is.EqualTo(0m));
            Assert.That(configuration.ValidationRange.Max, Is.EqualTo(0m));
            Assert.That(configuration.Step, Is.EqualTo(0m));
            Assert.That(configuration.MinimumRange, Is.EqualTo(0m));
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
    public void Can_Deserialize_Valid_Number(string minValue, int expected)
    {
        var json = $"{{\"validationLimit\":{{\"min\":{minValue},\"max\":10}}}}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.That(configuration!.ValidationLimit.Min, Is.EqualTo(expected));
            Assert.That(configuration.ValidationLimit.Max, Is.EqualTo(10));
        });
    }

    [Test]
    public void Can_Deserialize_Fractional_Number_For_Integer_Field_As_Zero()
    {
        // A fractional JSON number cannot represent an int, so it must resolve to the default rather than
        // being silently truncated (e.g. 1.5 -> 1).
        var json = "{\"validationLimit\":{\"min\":1.5,\"max\":10}}";

        MultiUrlPickerConfiguration? configuration =
            _serializer.Deserialize<MultiUrlPickerConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.That(configuration!.ValidationLimit.Min, Is.EqualTo(0));
            Assert.That(configuration.ValidationLimit.Max, Is.EqualTo(10));
        });
    }

    [Test]
    public void Can_Deserialize_Fractional_Number_For_Decimal_Field()
    {
        // Floating-point config must retain its fractional value.
        var json = "{\"step\":0.5,\"validationRange\":{\"min\":1,\"max\":10}}";

        SliderConfiguration? configuration = _serializer.Deserialize<SliderConfiguration>(json);

        Assert.IsNotNull(configuration);
        Assert.Multiple(() =>
        {
            Assert.That(configuration!.Step, Is.EqualTo(0.5m));
            Assert.That(configuration.ValidationRange.Min, Is.EqualTo(1m));
            Assert.That(configuration.ValidationRange.Max, Is.EqualTo(10m));
        });
    }

    [Test]
    public void Can_Serialize_Number_As_Numeric_Value()
    {
        var configuration = new MultiUrlPickerConfiguration { ValidationLimit = new NumberRange { Min = 2, Max = 5 } };

        var serialized = _serializer.Serialize(configuration);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(serialized.Contains("\"validationLimit\":"), serialized);
            Assert.IsTrue(serialized.Contains("\"min\":2"), serialized);
            Assert.IsTrue(serialized.Contains("\"max\":5"), serialized);
        });
    }
}
