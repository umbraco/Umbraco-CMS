using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Verifies that a label presents a string for every value type it can hold, so that what an editor sees matches the
/// type the editor declares.
/// </summary>
[TestFixture]
public class LabelPropertyValueEditorTests
{
    [TestCase(ValueTypes.String, "plain text")]
    [TestCase(ValueTypes.Text, "plain text")]
    [TestCase(ValueTypes.Bigint, "9007199254740993")]
    public void ToEditor_YieldsTheStoredString(string valueType, string stored)
        => Assert.AreEqual(stored, ToEditor(valueType, stored));

    [TestCase(ValueTypes.Text)]
    [TestCase(ValueTypes.String)]
    public void ToEditor_YieldsAJsonShapedValueAsAString(string valueType)
    {
        const string stored = """{"a":1}""";

        var value = ToEditor(valueType, stored);

        Assert.AreEqual(stored, value);
        Assert.IsInstanceOf<string>(value);
    }

    [Test]
    public void ToEditor_YieldsATimeAsItsTimeOfDay()
        => Assert.AreEqual("10:30:00", TimeToEditor(new DateTime(2026, 9, 7, 10, 30, 0)));

    [Test]
    public void ToEditor_YieldsAnEmptyStringForATimeWithNoValue()
        => Assert.AreEqual(string.Empty, TimeToEditor(null));

    [Test]
    public void ToEditor_YieldsADateTimeAsAFullTimestamp()
        => Assert.AreEqual("2026-09-07 10:30:00", ToEditor(ValueTypes.DateTime, new DateTime(2026, 9, 7, 10, 30, 0)));

    [Test]
    public void ToEditor_YieldsAnEmptyStringForNoValue()
        => Assert.AreEqual(string.Empty, ToEditor(ValueTypes.Text, null));

    private static object? ToEditor(string valueType, object? storedValue)
    {
        var editor = new LabelPropertyEditorBase.LabelPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            Attribute(valueType));

        return editor.ToEditor(Property(storedValue));
    }

    private static object? TimeToEditor(object? storedValue)
    {
        var editor = new TimeLabelPropertyEditor.TimeLabelPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            Attribute(ValueTypes.Time));

        return editor.ToEditor(Property(storedValue));
    }

    private static DataEditorAttribute Attribute(string valueType)
        => new("Test.Label") { ValueType = valueType };

    private static IProperty Property(object? storedValue)
        => Mock.Of<IProperty>(x => x.GetValue(null, null, false) == storedValue);
}
