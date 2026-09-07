using System;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
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
        const string Stored = """{"a":1}""";

        var value = ToEditor(valueType, Stored);

        Assert.AreEqual(Stored, value);
        Assert.IsInstanceOf<string>(value);
    }

    [Test]
    public void ToEditor_YieldsATimeAsItsTimeOfDay()
        => Assert.AreEqual("10:30:00", ToEditor(ValueTypes.Time, new DateTime(2026, 9, 7, 10, 30, 0)));

    [Test]
    public void ToEditor_YieldsADateTimeAsAFullTimestamp()
        => Assert.AreEqual("2026-09-07 10:30:00", ToEditor(ValueTypes.DateTime, new DateTime(2026, 9, 7, 10, 30, 0)));

    [Test]
    public void ToEditor_YieldsAnEmptyStringForNoValue()
        => Assert.AreEqual(string.Empty, ToEditor(ValueTypes.Text, null));

    private static object? ToEditor(string valueType, object? storedValue)
    {
        var attribute = new DataEditorAttribute("Test.Label") { ValueType = valueType };
        var editor = new LabelPropertyEditorBase.LabelPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            attribute);

        var property = Mock.Of<IProperty>(x => x.GetValue(null, null, false) == storedValue);

        return editor.ToEditor(property);
    }
}
