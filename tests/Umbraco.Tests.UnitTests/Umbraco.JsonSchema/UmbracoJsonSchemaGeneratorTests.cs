// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using System.Text.Json.Serialization;
using NJsonSchema;
using NUnit.Framework;
using Umbraco.JsonSchema;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.JsonSchema;

[TestFixture]
public class UmbracoJsonSchemaGeneratorTests
{
    [Test]
    public void Can_Generate_TimeSpan_Without_Format()
    {
        NJsonSchema.JsonSchema schema = Generate();

        Assert.Multiple(() =>
        {
            Assert.That(schema.Properties[nameof(FixtureSettings.Timeout)].Format, Is.Null);
            Assert.That(schema.Properties[nameof(FixtureSettings.OptionalTimeout)].Format, Is.Null);
        });
    }

    [Test]
    public void Can_Generate_Other_Formats()
    {
        NJsonSchema.JsonSchema schema = Generate();

        Assert.Multiple(() =>
        {
            Assert.That(schema.Properties[nameof(FixtureSettings.Timestamp)].Format, Is.EqualTo(JsonFormatStrings.DateTime));
            Assert.That(schema.Properties[nameof(FixtureSettings.Key)].Format, Is.EqualTo(JsonFormatStrings.Guid));
        });
    }

    [Test]
    public void Can_Generate_Enums_As_Strings()
    {
        NJsonSchema.JsonSchema schema = Generate();

        NJsonSchema.JsonSchema property = schema.Properties[nameof(FixtureSettings.Level)].ActualSchema;

        Assert.Multiple(() =>
        {
            Assert.That(property.Type, Is.EqualTo(JsonObjectType.String));
            Assert.That(property.Enumeration, Does.Contain(nameof(FixtureLevel.High)));
        });
    }

    [Test]
    public void Can_Generate_Default_Values()
    {
        NJsonSchema.JsonSchema schema = Generate();

        Assert.That(schema.Properties[nameof(FixtureSettings.Count)].Default, Is.EqualTo(5));
    }

    [Test]
    public void Can_Generate_Renamed_Properties()
    {
        NJsonSchema.JsonSchema schema = Generate();

        Assert.Multiple(() =>
        {
            Assert.That(schema.Properties, Does.ContainKey("renamed"));
            Assert.That(schema.Properties, Does.Not.ContainKey(nameof(FixtureSettings.Renamed)));
        });
    }

    [Test]
    public void Can_Generate_Inherited_Properties_Without_Inheritance_Hierarchy()
    {
        NJsonSchema.JsonSchema schema = Generate();

        Assert.Multiple(() =>
        {
            Assert.That(schema.Properties, Does.ContainKey(nameof(FixtureSettingsBase.Inherited)));
            Assert.That(schema.AllOf, Is.Empty);
            Assert.That(schema.InheritedSchema, Is.Null);
        });
    }

    [Test]
    public void Can_Generate_Additional_Properties()
    {
        NJsonSchema.JsonSchema schema = Generate();

        Assert.That(schema.AllowAdditionalProperties, Is.True);
    }

    [Test]
    public void Cannot_Generate_Read_Only_Properties()
    {
        NJsonSchema.JsonSchema schema = Generate();

        Assert.That(schema.Properties, Does.Not.ContainKey(nameof(FixtureSettings.ReadOnly)));
    }

    [Test]
    public void Cannot_Generate_Obsolete_Properties()
    {
        NJsonSchema.JsonSchema schema = Generate();

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.That(schema.Properties, Does.Not.ContainKey(nameof(FixtureSettings.Obsoleted)));
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Test]
    public void Cannot_Generate_Ignored_Properties()
    {
        NJsonSchema.JsonSchema schema = Generate();

        Assert.That(schema.Properties, Does.Not.ContainKey(nameof(FixtureSettings.Ignored)));
    }

    [Test]
    public void Cannot_Generate_Duration_Format_For_Any_Setting()
    {
        var json = new UmbracoJsonSchemaGenerator().Generate(typeof(UmbracoCmsSchema)).ToJson();

        Assert.That(json, Does.Not.Contain($"\"{JsonFormatStrings.Duration}\""));
    }

    private static NJsonSchema.JsonSchema Generate()
        => new UmbracoJsonSchemaGenerator().Generate(typeof(FixtureSettings));

    private enum FixtureLevel
    {
        Low,
        High,
    }

    private abstract class FixtureSettingsBase
    {
        public string? Inherited { get; set; }
    }

    private sealed class FixtureSettings : FixtureSettingsBase
    {
        public TimeSpan Timeout { get; set; }

        public TimeSpan? OptionalTimeout { get; set; }

        public DateTime Timestamp { get; set; }

        public Guid Key { get; set; }

        public FixtureLevel Level { get; set; }

        [DefaultValue(5)]
        public int Count { get; set; }

        [JsonPropertyName("renamed")]
        public string? Renamed { get; set; }

        public string? ReadOnly { get; }

        [Obsolete("Scheduled for removal in Umbraco 19.")]
        public string? Obsoleted { get; set; }

        [JsonIgnore]
        public string? Ignored { get; set; }
    }
}
