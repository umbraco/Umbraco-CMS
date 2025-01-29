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
        var subject = new SystemTextJsonSerializer();
        Assert.IsTrue(subject.TryDeserialize(json!, out MyItem myItem));
        Assert.AreEqual("value", myItem.MyProperty);
    }

    [Test]
    public void TryDeserialize_Can_Handle_JsonArray()
    {
        var json = JsonNode.Parse("[{\"myProperty\":\"value1\"},{\"myProperty\":\"value2\"}]");
        var subject = new SystemTextJsonSerializer();
        Assert.IsTrue(subject.TryDeserialize(json!, out MyItem[] myItems));
        Assert.AreEqual(2, myItems.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("value1", myItems[0].MyProperty);
            Assert.AreEqual("value2", myItems[1].MyProperty);
        });
    }

    [Test]
    public void TryDeserialize_Can_Handle_JsonString()
    {
        var json = "{\"myProperty\":\"value\"}";
        var subject = new SystemTextJsonSerializer();
        Assert.IsTrue(subject.TryDeserialize(json, out MyItem myItem));
        Assert.AreEqual("value", myItem.MyProperty);
    }

    [Test]
    public void TryDeserialize_Cannot_Handle_RandomString()
    {
        var subject = new SystemTextJsonSerializer();
        Assert.IsFalse(subject.TryDeserialize<MyItem>("something something", out _));
    }

    private class MyItem
    {
        public required string MyProperty { get; set; }
    }
}
