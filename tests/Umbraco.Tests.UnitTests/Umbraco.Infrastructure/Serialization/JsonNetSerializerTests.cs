// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Serialization;

[TestFixture]
public class JsonNetSerializerTests
{
    private IJsonSerializer Sut => new JsonNetSerializer();

    [Test]
    public void DeserializeSubset__Subset_as_integer()
    {
        var expected = 3;
        var key = "int";
        var full = $"{{\"{key}\": {expected}}}";

        var actual = Sut.DeserializeSubset<int>(full, key);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void DeserializeSubset__Subset_as_string()
    {
        var expected = "test";
        var key = "text";
        var full = $"{{\"{key}\": \"{expected}\"}}";

        var actual = Sut.DeserializeSubset<string>(full, key);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void DeserializeSubset__Subset_nested_value_as_string()
    {
        var expected = "test";
        var key = "text";
        var key2 = "text2";
        var full = $"{{\"{key}\": {{\"{key2}\": \"{expected}\"}}}}";

        var actual = Sut.DeserializeSubset<string>(full, key + "." + key2);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void DeserializeSubset__Subset_value_as_object()
    {
        var expected = new MyStruct { Key = "Test" };
        var key = nameof(MyStruct.Key);
        var full = $"{{\"{key}\": {JsonConvert.SerializeObject(expected)}";

        var actual = Sut.DeserializeSubset<MyStruct>(full, key);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void DeserializeSubset__Subset_value_as_array()
    {
        var expected = new[] { "test" };
        var key = "text";
        var full = $"{{\"{key}\": {JsonConvert.SerializeObject(expected)}";

        var actual = Sut.DeserializeSubset<string[]>(full, key);

        Assert.AreEqual(expected, actual);
    }

    private struct MyStruct
    {
        public string Key { get; set; }
    }
}
