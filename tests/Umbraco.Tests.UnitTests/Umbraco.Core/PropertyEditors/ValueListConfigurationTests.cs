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
    private static readonly IConfigurationEditorJsonSerializer _serializer =
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
            Items = [new ValueListOption { Value = "a" }, new ValueListOption { Value = "b", Label = "B" }],
        };

        AssertJsonEquals("""{"items":[{"value":"a","label":null},{"value":"b","label":"B"}]}""", Write(configuration));
    }

    [Test]
    public void Reads_No_Options_When_The_Key_Is_Missing()
    {
        ValueListConfiguration configuration = Read("""{"foo":1}""");

        Assert.That(configuration.Items, Is.Empty);
    }

    [Test]
    public void Keeps_Labels_When_Read_And_Written()
    {
        const string Json = """{"items":[{"value":"a","label":"A"},{"value":"b","label":null}]}""";

        AssertJsonEquals(Json, Write(Read(Json)));
    }

    [Test]
    public void Treats_Empty_Label_As_No_Label()
        => Assert.That(new ValueListOption { Value = "a", Label = string.Empty }.Label, Is.Null);

    private static IDictionary<string, object> Deserialize(string json)
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
