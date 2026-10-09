// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.ValueConverters;

[TestFixture]
public class LabelValueConverterTests
{
    private LabelValueConverter _sut;

    [SetUp]
    public void SetUp() => _sut = new LabelValueConverter();

    [TestCase(Constants.PropertyEditors.Aliases.Label)]
    [TestCase(Constants.PropertyEditors.Aliases.LabelText)]
    [TestCase(Constants.PropertyEditors.Aliases.LabelInteger)]
    [TestCase(Constants.PropertyEditors.Aliases.LabelBigInt)]
    [TestCase(Constants.PropertyEditors.Aliases.LabelDecimal)]
    [TestCase(Constants.PropertyEditors.Aliases.LabelDateTime)]
    [TestCase(Constants.PropertyEditors.Aliases.LabelTime)]
    public void Converts_Every_Label_Editor(string editorAlias)
        => Assert.That(_sut.IsConverter(PropertyType(editorAlias)), Is.True);

    [TestCase(Constants.PropertyEditors.Aliases.TextBox)]
    [TestCase(Constants.PropertyEditors.Aliases.Integer)]
    [TestCase("Umbraco.Label.Something.Else")]
    public void Does_Not_Convert_Other_Editors(string editorAlias)
        => Assert.That(_sut.IsConverter(PropertyType(editorAlias)), Is.False);

    [TestCase(Constants.PropertyEditors.Aliases.Label, typeof(string))]
    [TestCase(Constants.PropertyEditors.Aliases.LabelText, typeof(string))]
    [TestCase(Constants.PropertyEditors.Aliases.LabelInteger, typeof(int))]
    [TestCase(Constants.PropertyEditors.Aliases.LabelBigInt, typeof(long))]
    [TestCase(Constants.PropertyEditors.Aliases.LabelDecimal, typeof(decimal))]
    [TestCase(Constants.PropertyEditors.Aliases.LabelDateTime, typeof(DateTime))]
    [TestCase(Constants.PropertyEditors.Aliases.LabelTime, typeof(TimeSpan))]
    public void Takes_Its_Value_Type_From_The_Editor(string editorAlias, Type expected)
        => Assert.That(_sut.GetPropertyValueType(PropertyType(editorAlias)), Is.EqualTo(expected));

    [Test]
    public void Keeps_A_String_As_It_Was_Stored()
    {
        // The reason this converter exists: without it a numeric-looking string is parsed as a number, losing its
        // leading zeros (U4-7929).
        var converted = Convert(Constants.PropertyEditors.Aliases.Label, "00123");

        Assert.That(converted, Is.EqualTo("00123"));
    }

    [TestCase("1234", 1234)]
    [TestCase("not a number", 0)]
    public void Converts_An_Integer_From_Its_Stored_String(string stored, int expected)
        => Assert.That(Convert(Constants.PropertyEditors.Aliases.LabelInteger, stored), Is.EqualTo(expected));

    [Test]
    public void Converts_An_Integer_Already_Stored_As_One()
        => Assert.That(Convert(Constants.PropertyEditors.Aliases.LabelInteger, 1234), Is.EqualTo(1234));

    [Test]
    public void Converts_A_Big_Integer_From_Its_Stored_String()
        => Assert.That(
            Convert(Constants.PropertyEditors.Aliases.LabelBigInt, "9007199254740993"),
            Is.EqualTo(9007199254740993L));

    [Test]
    public void Converts_A_Decimal_Invariantly()
        => Assert.That(Convert(Constants.PropertyEditors.Aliases.LabelDecimal, "56.78"), Is.EqualTo(56.78m));

    [Test]
    public void Converts_A_Decimal_Already_Stored_As_One()
        => Assert.That(Convert(Constants.PropertyEditors.Aliases.LabelDecimal, 56.78m), Is.EqualTo(56.78m));

    [Test]
    public void Converts_A_DateTime_Already_Stored_As_One()
    {
        var stored = new DateTime(2004, 05, 06, 07, 08, 09);

        Assert.That(Convert(Constants.PropertyEditors.Aliases.LabelDateTime, stored), Is.EqualTo(stored));
    }

    [Test]
    public void Converts_A_DateTime_From_Its_Stored_String()
        => Assert.That(
            Convert(Constants.PropertyEditors.Aliases.LabelDateTime, "2004-05-06T07:08:09"),
            Is.EqualTo(new DateTime(2004, 05, 06, 07, 08, 09)));

    [Test]
    public void Converts_An_Unparseable_DateTime_To_Its_Minimum()
        => Assert.That(
            Convert(Constants.PropertyEditors.Aliases.LabelDateTime, "not a date"),
            Is.EqualTo(DateTime.MinValue));

    [Test]
    public void Converts_A_Time_From_The_Time_Of_A_Stored_DateTime()
        => Assert.That(
            Convert(Constants.PropertyEditors.Aliases.LabelTime, new DateTime(1900, 01, 01).Add(new TimeSpan(02, 03, 04))),
            Is.EqualTo(new TimeSpan(02, 03, 04)));

    [Test]
    public void Converts_A_Time_From_Its_Stored_String()
        => Assert.That(
            Convert(Constants.PropertyEditors.Aliases.LabelTime, "02:03:04"),
            Is.EqualTo(new TimeSpan(02, 03, 04)));

    [Test]
    public void Converts_A_Missing_String_To_An_Empty_One()
        => Assert.That(Convert(Constants.PropertyEditors.Aliases.Label, null), Is.EqualTo(string.Empty));

    private object? Convert(string editorAlias, object? source)
        => _sut.ConvertSourceToIntermediate(
            Mock.Of<IPublishedElement>(),
            PropertyType(editorAlias),
            source,
            preview: false);

    private static IPublishedPropertyType PropertyType(string editorAlias)
        => Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == editorAlias);
}
