using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// Contains unit tests for back office serialization in Umbraco CMS API management.
/// </summary>
[TestFixture]
public class BackOfficeSerializationTests
{
    private JsonOptions jsonOptions;

    /// <summary>
    /// Configures the <see cref="JsonOptions"/> used for Umbraco backoffice serialization tests by initializing
    /// the required type information resolver and applying the backoffice-specific JSON configuration.
    /// This method is called before each test to ensure consistent serialization settings.
    /// </summary>
    [SetUp]
    public void SetupOptions()
    {
        var typeInfoResolver = new UmbracoJsonTypeInfoResolver(TestHelper.GetTypeFinder());
        var configurationOptions = new ConfigureUmbracoBackofficeJsonOptions(typeInfoResolver);
        var options = new JsonOptions();
        configurationOptions.Configure(global::Umbraco.Cms.Core.Constants.JsonOptionsNames.BackOffice, options);
        jsonOptions = options;
    }

    /// <summary>
    /// Tests that the serialization output uses camel case naming.
    /// </summary>
    [Test]
    public void Will_Serialize_To_Camel_Case()
    {
        var objectToSerialize = new UnNestedJsonTestValue();

        var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);

        Assert.AreEqual("{\"stringValue\":\"theValue\"}", json);
    }

    // the limit is 64, but it seems like the functional limit is that minus 1
    /// <summary>
    /// Verifies that serialization of nested objects succeeds up to the maximum supported depth and fails beyond it.
    /// </summary>
    /// <param name="depth">The nesting depth of the object to serialize.</param>
    /// <param name="shouldPass">True if serialization is expected to succeed at the specified depth; false if it should fail.</param>
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

    /// <summary>
    /// Tests that ValidationProblemDetails are serialized with casing aligned with MVC conventions.
    /// </summary>
    [Test]
    public void Will_Serialize_ValidationProblemDetails_To_Casing_Aligned_With_Mvc()
    {
        var objectToSerialize = new TestValueWithValidationProblemDetail();

        var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);

        var expectedJson = "{\"problemDetails\":{\"type\":\"Test type\",\"title\":\"Test title\",\"status\":400,\"detail\":\"Test detail\",\"instance\":\"Test instance\",\"errors\":[{\"$.testError1\":[\"Test error 1a\",\"Test error 1b\"]},{\"$.testError2\":[\"Test error 2a\"]},{\"$.testError3.testError3a\":[\"Test error 3b\"]}],\"traceId\":\"traceValue\"}}";
        Assert.AreEqual(expectedJson, json);
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

    /// <summary>
    /// Test value for unnested JSON serialization.
    /// </summary>
    public class UnNestedJsonTestValue
    {
    /// <summary>
    /// Gets or sets the string value.
    /// </summary>
        public string StringValue { get; set; } = "theValue";
    }

    /// <summary>
    /// Represents a test value used for nested JSON serialization tests in the backoffice API.
    /// </summary>
    public class NestedJsonTestValue
    {
    /// <summary>
    /// Gets or sets the hierarchical level of this <see cref="NestedJsonTestValue"/> instance within the nested JSON structure.
    /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the inner nested JSON test value.
        /// </summary>
        public NestedJsonTestValue? Inner { get; set; }
    }

    private class TestValueWithValidationProblemDetail
    {
    /// <summary>
    /// Gets or sets a <see cref="ValidationProblemDetails"/> instance pre-populated with test values.
    /// This property is used to provide a consistent set of validation problem details for unit testing serialization behavior.
    /// The instance includes preset values for <c>Title</c>, <c>Detail</c>, <c>Status</c>, <c>Type</c>, <c>Instance</c>, custom <c>Extensions</c>, and multiple <c>Errors</c>.
    /// </summary>
        public ValidationProblemDetails ProblemDetails { get; set; } = new()
        {
            Title = "Test title",
            Detail = "Test detail",
            Status = 400,
            Type = "Test type",
            Instance = "Test instance",
            Extensions =
            {
                ["traceId"] = "traceValue",
                ["someOtherExtension"] = "someOtherExtensionValue",
            },
            Errors =
            {
                ["TestError1"] = ["Test error 1a", "Test error 1b"],
                ["TestError2"] = ["Test error 2a"],
                ["TestError3.TestError3a"] = ["Test error 3b"],
            }
        };
    }
}
