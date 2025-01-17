using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Serialization;

[TestFixture]
public class JsonStringInternConverterTests
{
    [Test]
    public void Intern_Property_String()
    {
        var obj = new Test
        {
            Name = Guid.NewGuid().ToString(),
        };

        // Ensure the raw value is not interned
        Assert.IsNull(string.IsInterned(obj.Name));

        // Ensure the value is interned when deserializing using converter
        var json = JsonSerializer.Serialize(obj);
        obj = JsonSerializer.Deserialize<Test>(json);

        Assert.IsNotNull(string.IsInterned(obj.Name));

        // Ensure the value is interned when deserializing using options
        json = JsonSerializer.Serialize(Guid.NewGuid().ToString());
        var str = JsonSerializer.Deserialize<string>(json, new JsonSerializerOptions()
        {
            Converters =
            {
                new JsonStringInternConverter(),
            },
        });

        Assert.IsNotNull(string.IsInterned(str));
    }

    [Test]
    public void Intern_Property_Dictionary()
    {
        var obj = new Test
        {
            Values = new Dictionary<string, int>
            {
                [Guid.NewGuid().ToString()] = 0,
                [Guid.NewGuid().ToString()] = 1,
            },
        };

        // Ensure the raw values are not interned
        Assert.Multiple(() =>
        {
            foreach (string key in obj.Values.Keys)
            {
                Assert.IsNull(string.IsInterned(key));
            }
        });

        // Add value to dictionary to test case-insensitivity
        obj.Values.Add("Test", 3);

        // Ensure the value is interned when deserializing using converter
        var json = JsonSerializer.Serialize(obj);
        obj = JsonSerializer.Deserialize<Test>(json);

        Assert.Multiple(() =>
        {
            foreach (string key in obj.Values.Keys)
            {
                Assert.IsNotNull(string.IsInterned(key));
            }
        });

        // Check that the dictionary is case-insensitive
        Assert.IsTrue(obj.Values.ContainsKey("Test"));
        Assert.IsTrue(obj.Values.ContainsKey("test"));

        // Ensure the value is interned when deserializing using options
        json = JsonSerializer.Serialize(new Dictionary<string, int>
        {
            [Guid.NewGuid().ToString()] = 0,
            [Guid.NewGuid().ToString()] = 1,
            ["Test"] = 3
        });
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, int>>(json, new JsonSerializerOptions()
        {
            Converters =
            {
                new JsonDictionaryStringInternIgnoreCaseConverter<int>(),
            },
        });

        Assert.Multiple(() =>
        {
            foreach (string key in dictionary.Keys)
            {
                Assert.IsNotNull(string.IsInterned(key));
            }
        });

        // Check that the dictionary is case-insensitive
        Assert.IsTrue(dictionary.ContainsKey("Test"));
        Assert.IsTrue(dictionary.ContainsKey("test"));
    }

    public class Test
    {
        [JsonConverter(typeof(JsonStringInternConverter))]
        public string Name { get; set; }

        [JsonConverter(typeof(JsonDictionaryStringInternIgnoreCaseConverter<int>))]
        public Dictionary<string, int> Values { get; set; } = new();
    }
}
