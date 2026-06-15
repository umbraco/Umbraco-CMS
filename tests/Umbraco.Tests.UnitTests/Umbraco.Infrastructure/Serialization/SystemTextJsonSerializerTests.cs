using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Serialization;

[TestFixture]
public class SystemTextJsonSerializerTests
{
    [Test]
    public void TryDeserialize_Can_Handle_JsonObject()
    {
        var json = JsonNode.Parse("{\"myProperty\":\"value\"}");
        var subject = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        Assert.That(subject.TryDeserialize(json!, out MyItem myItem), Is.True);
        Assert.That(myItem.MyProperty, Is.EqualTo("value"));
    }

    [Test]
    public void TryDeserialize_Can_Handle_JsonArray()
    {
        var json = JsonNode.Parse("[{\"myProperty\":\"value1\"},{\"myProperty\":\"value2\"}]");
        var subject = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        Assert.That(subject.TryDeserialize(json!, out MyItem[] myItems), Is.True);
        Assert.That(myItems.Length, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(myItems[0].MyProperty, Is.EqualTo("value1"));
            Assert.That(myItems[1].MyProperty, Is.EqualTo("value2"));
        });
    }

    [Test]
    public void TryDeserialize_Can_Handle_JsonString()
    {
        var json = "{\"myProperty\":\"value\"}";
        var subject = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        Assert.That(subject.TryDeserialize(json, out MyItem myItem), Is.True);
        Assert.That(myItem.MyProperty, Is.EqualTo("value"));
    }

    [Test]
    public void TryDeserialize_Cannot_Handle_RandomString()
    {
        var subject = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        Assert.That(subject.TryDeserialize<MyItem>("something something", out _), Is.False);
    }

    private class MyItem
    {
        public required string MyProperty { get; set; }
    }
}
