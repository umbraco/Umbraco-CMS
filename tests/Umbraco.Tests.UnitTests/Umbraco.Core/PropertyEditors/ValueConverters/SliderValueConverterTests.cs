// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.ValueConverters;

[TestFixture]
public class SliderValueConverterTests
{
    private SliderValueConverter _slider;
    private RangeSliderValueConverter _rangeSlider;

    [SetUp]
    public void SetUp()
    {
        _slider = new SliderValueConverter();
        _rangeSlider = new RangeSliderValueConverter();
    }

    [Test]
    public void Each_Converter_Converts_Only_Its_Own_Editor()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_slider.IsConverter(PropertyType(Constants.PropertyEditors.Aliases.Slider)), Is.True);
            Assert.That(_slider.IsConverter(PropertyType(Constants.PropertyEditors.Aliases.RangeSlider)), Is.False);
            Assert.That(_rangeSlider.IsConverter(PropertyType(Constants.PropertyEditors.Aliases.RangeSlider)), Is.True);
            Assert.That(_rangeSlider.IsConverter(PropertyType(Constants.PropertyEditors.Aliases.Slider)), Is.False);
        });
    }

    [Test]
    public void Takes_Its_Value_Type_From_The_Editor()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                _slider.GetPropertyValueType(PropertyType(Constants.PropertyEditors.Aliases.Slider)),
                Is.EqualTo(typeof(decimal)));
            Assert.That(
                _rangeSlider.GetPropertyValueType(PropertyType(Constants.PropertyEditors.Aliases.RangeSlider)),
                Is.EqualTo(typeof(Range<decimal>)));
        });
    }

    [TestCase("5", 5)]
    [TestCase("5.5", 5.5)]
    [TestCase("", 0)]
    [TestCase(null, 0)]
    [TestCase("not a number", 0)]
    public void Reads_A_Single_Value(string? stored, decimal expected)
        => Assert.That(ConvertSingle(stored), Is.EqualTo(expected));

    [Test]
    public void Reads_A_Single_Value_Invariantly()
        => Assert.That(ConvertSingle("1234.5"), Is.EqualTo(1234.5m));

    [Test]
    public void Reads_The_Low_End_Of_A_Value_Stored_As_A_Range()
    {
        // A data type that held a range before the two editors were separated still has values in it.
        Assert.That(ConvertSingle("1,5"), Is.EqualTo(1m));
    }

    [Test]
    public void Reads_A_Range()
    {
        Range<decimal> converted = ConvertRange("1,5");

        Assert.Multiple(() =>
        {
            Assert.That(converted.Minimum, Is.EqualTo(1m));
            Assert.That(converted.Maximum, Is.EqualTo(5m));
        });
    }

    [Test]
    public void Reads_Both_Ends_Of_A_Value_Stored_As_A_Single_Value()
    {
        // The reverse case: a data type that held a single value has both ends of the range at that value.
        Range<decimal> converted = ConvertRange("5");

        Assert.Multiple(() =>
        {
            Assert.That(converted.Minimum, Is.EqualTo(5m));
            Assert.That(converted.Maximum, Is.EqualTo(5m));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("not a number")]
    [TestCase("1,not a number")]
    [TestCase("1,2,3")]
    public void Reads_An_Unusable_Range_As_An_Empty_One(string? stored)
    {
        Range<decimal> converted = ConvertRange(stored);

        Assert.Multiple(() =>
        {
            Assert.That(converted.Minimum, Is.EqualTo(0m));
            Assert.That(converted.Maximum, Is.EqualTo(0m));
        });
    }

    private decimal ConvertSingle(string? stored)
        => (decimal)_slider.ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            PropertyType(Constants.PropertyEditors.Aliases.Slider),
            PropertyCacheLevel.Element,
            stored,
            preview: false)!;

    private Range<decimal> ConvertRange(string? stored)
        => (Range<decimal>)_rangeSlider.ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            PropertyType(Constants.PropertyEditors.Aliases.RangeSlider),
            PropertyCacheLevel.Element,
            stored,
            preview: false)!;

    private static IPublishedPropertyType PropertyType(string editorAlias)
        => Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == editorAlias);
}
