using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Serialization;

[TestFixture]
public class JsonUdiConverterTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        Converters =
        {
            new JsonUdiConverter(),
        },
    };

    [Test]
    public void Can_Serialize()
    {
        var udi = new GuidUdi("document", Guid.Parse("3382d5433b5749d08919bc9961422a1f"));
        var artifact = new Test { Udi = udi };

        string serialized = JsonSerializer.Serialize(artifact, _jsonSerializerOptions);

        var expected = "{\"Udi\":\"umb://document/3382d5433b5749d08919bc9961422a1f\"}";
        Assert.That(serialized, Is.EqualTo(expected));
    }

    [Test]
    public void Can_Deserialize()
    {
        var serialized = "{\"Udi\":\"umb://document/3382d5433b5749d08919bc9961422a1f\"}";

        Test? deserialized = JsonSerializer.Deserialize<Test>(serialized, _jsonSerializerOptions);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Udi.Guid, Is.EqualTo(Guid.Parse("3382d5433b5749d08919bc9961422a1f")));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Will_Deserialize_To_Null_With_Null_Or_Whitepsace_Udi(string? serializedUdi)
    {
        var serializedUdiPart = serializedUdi is null ? "null" : $"\"{serializedUdi}\"";
        var serialized = "{\"Udi\":" + serializedUdiPart + "}";

        Test? deserialized = JsonSerializer.Deserialize<Test>(serialized, _jsonSerializerOptions);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Udi, Is.Null);
    }

    [Test]
    public void Throws_On_Invalid_Udi()
    {
        var serialized = "{\"Udi\":\"invalid-udi\"}";

        Assert.Throws<FormatException>(() =>
            JsonSerializer.Deserialize<Test>(serialized, _jsonSerializerOptions));
    }

    private class Test
    {
        public GuidUdi? Udi { get; set; }
    }
}
