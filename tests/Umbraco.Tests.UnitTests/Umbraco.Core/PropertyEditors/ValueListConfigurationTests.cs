// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class ValueListConfigurationTests
{
    private static readonly SystemTextConfigurationEditorJsonSerializer _serializer =
        new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    [Test]
    public void Reads_Options_With_Labels()
    {
        ValueListConfiguration configuration = Read("""{"items":[{"value":"a","label":"A"}]}""");

        Assert.That(configuration.Items, Has.Count.EqualTo(1));
        Assert.That(configuration.Items[0].Value, Is.EqualTo("a"));
        Assert.That(configuration.Items[0].Label, Is.EqualTo("A"));
    }

    [Test]
    public void Rejects_Plain_String_Items()
        => Assert.Throws<InvalidOperationException>(() => Read("""{"items":["a","b"]}"""));

    [Test]
    public void Reads_Empty_Options()
    {
        ValueListConfiguration configuration = Read("""{"items":[]}""");

        Assert.That(configuration.Items, Is.Empty);
    }

    [Test]
    public void Writes_Options_As_Objects()
    {
        var configuration = new ValueListConfiguration
        {
            Items = [new ValueListOption { Value = "a", Label = "A" }, new ValueListOption { Value = "b", Label = "B" }],
        };

        AssertJsonEquals("""{"items":[{"value":"a","label":"A"},{"value":"b","label":"B"}]}""", Write(configuration));
    }

    [Test]
    public void Reads_No_Options_When_The_Key_Is_Missing()
    {
        ValueListConfiguration configuration = Read("""{"foo":1}""");

        Assert.That(configuration.Items, Is.Empty);
    }

    private static IEnumerable<TestCaseData> ValueListConfigurationEditors()
    {
        yield return new TestCaseData(new ValueListConfigurationEditor(Mock.Of<IIOHelper>(), _serializer)).SetArgDisplayNames("ValueList");
        yield return new TestCaseData(new DropDownFlexibleConfigurationEditor(Mock.Of<IIOHelper>(), _serializer)).SetArgDisplayNames("DropDownFlexible");
        yield return new TestCaseData(new SingleDropDownConfigurationEditor(Mock.Of<IIOHelper>(), _serializer)).SetArgDisplayNames("SingleDropDown");
    }

    [TestCaseSource(nameof(ValueListConfigurationEditors))]
    public void Reads_Plain_String_Items_As_Options_Labelled_With_Their_Value(IConfigurationEditor configurationEditor)
        => AssertJsonEquals(
            """{"items":[{"value":"a","label":"a"},{"value":"b","label":"b"}],"other":1}""",
            _serializer.Serialize(configurationEditor.FromDatabase("""{"items":["a","b"],"other":1}""", _serializer)));

    [TestCaseSource(nameof(ValueListConfigurationEditors))]
    public void Reads_Option_Objects_As_They_Are(IConfigurationEditor configurationEditor)
        => AssertJsonEquals(
            """{"items":[{"value":"a","label":"a"},{"value":"b","label":"B"},{"value":"c"}]}""",
            _serializer.Serialize(configurationEditor.FromDatabase("""{"items":["a",{"value":"b","label":"B"},{"value":"c"}]}""", _serializer)));

    private static Dictionary<string, object> Deserialize(string json)
        => _serializer.Deserialize<Dictionary<string, object>>(json)!;

    private static ValueListConfiguration Read(string json)
    {
        var configurationEditor = new ValueListConfigurationEditor(Mock.Of<IIOHelper>(), _serializer);
        return (ValueListConfiguration)configurationEditor.ToConfigurationObject(Deserialize(json), _serializer);
    }

    private static string Write(ValueListConfiguration configuration)
    {
        var configurationEditor = new ValueListConfigurationEditor(Mock.Of<IIOHelper>(), _serializer);
        return _serializer.Serialize(configurationEditor.FromConfigurationObject(configuration, _serializer));
    }

    private static void AssertJsonEquals(string expected, string actual)
        => Assert.That(JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual)), Is.True, $"Expected {expected} but got {actual}");
}
