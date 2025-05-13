using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

[TestFixture]
public class BackOfficeSerializationTests
{
    private JsonOptions jsonOptions;

    [SetUp]
    public void SetupOptions()
    {
        var typeInfoResolver = new UmbracoJsonTypeInfoResolver(TestHelper.GetTypeFinder());
        var configurationOptions = new ConfigureUmbracoBackofficeJsonOptions(typeInfoResolver);
        var options = new JsonOptions();
        configurationOptions.Configure(global::Umbraco.Cms.Core.Constants.JsonOptionsNames.BackOffice, options);
        jsonOptions = options;
    }

    [Test]
    public void Will_Serialize_To_Camel_Case()
    {
        var objectToSerialize = new UnNestedJsonTestValue();

        var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);

        Assert.AreEqual("{\"stringValue\":\"theValue\"}", json);
    }

    // the limit is 64, but it seems like the functional limit is that minus 1
    [TestCase(1, true, TestName = "Can_Serialize_At_Min_Depth(1)")]
    [TestCase(48, true, TestName = "Can_Serialize_At_High_Depth(33)")]
    [TestCase(63, true, TestName = "Can_Serialize_To_Max_Depth(63)")]
    [TestCase(64, false, TestName = "Can_NOT_Serialize_Beyond_Max_Depth(64)")]
    public void Can_Serialize_To_Max_Depth(int depth, bool shouldPass)
    {
        var objectToSerialize = CreateNestedObject(depth);

        if (shouldPass)
        {
            var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);
            Assert.IsNotEmpty(json);
        }
        else
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions));
        }
    }

    private static NestedJsonTestValue CreateNestedObject(int levels)
    {
        var root = new NestedJsonTestValue { Level = 1 };
        var outer = root;
        for (var i = 2; i <= levels; i++)
        {
            var inner = new NestedJsonTestValue { Level = i };
            outer.Inner = inner;
            outer = inner;
        }

        return root;
    }

    public class UnNestedJsonTestValue
    {
        public string StringValue { get; set; } = "theValue";
    }

    public class NestedJsonTestValue
    {
        public int Level { get; set; }

        public NestedJsonTestValue? Inner { get; set; }
    }
}
