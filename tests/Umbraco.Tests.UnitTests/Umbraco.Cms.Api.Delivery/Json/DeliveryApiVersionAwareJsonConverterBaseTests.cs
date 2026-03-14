using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Json;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Json;

/// <summary>
/// Contains unit tests for the <see cref="DeliveryApiVersionAwareJsonConverterBase{T}"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class DeliveryApiVersionAwareJsonConverterBaseTests
{
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private Mock<IApiVersioningFeature> _apiVersioningFeatureMock;

    private void SetUpMocks(int apiVersion)
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _apiVersioningFeatureMock = new Mock<IApiVersioningFeature>();

        _apiVersioningFeatureMock
            .SetupGet(feature => feature.RequestedApiVersion)
            .Returns(new ApiVersion(apiVersion));

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set(_apiVersioningFeatureMock.Object);

        _httpContextAccessorMock
            .SetupGet(accessor => accessor.HttpContext)
            .Returns(httpContext);
    }

    /// <summary>
    /// Verifies that all properties are included in the JSON output when the HTTP context is not available.
    /// This ensures that the absence of an HTTP context does not restrict the serialization of any properties, regardless of the API version.
    /// </summary>
    /// <param name="apiVersion">The API version being simulated for the test.</param>
    /// <param name="expectedPropertyNames">An array of property names expected to be present in the JSON output.</param>
    [Test]
    [TestCase(1, new[] { "PropertyAll", "PropertyV1Max", "PropertyV2Max", "PropertyV2Only", "PropertyV2Min" })]
    [TestCase(2, new[] { "PropertyAll", "PropertyV1Max", "PropertyV2Max", "PropertyV2Only", "PropertyV2Min" })]
    [TestCase(3, new[] { "PropertyAll", "PropertyV1Max", "PropertyV2Max", "PropertyV2Only", "PropertyV2Min" })]
    public void Can_Include_All_Properties_When_HttpContext_Is_Not_Available(int apiVersion, string[] expectedPropertyNames)
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using var jsonWriter = new Utf8JsonWriter(memoryStream);

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _apiVersioningFeatureMock = new Mock<IApiVersioningFeature>();

        _apiVersioningFeatureMock
            .SetupGet(feature => feature.RequestedApiVersion)
            .Returns(new ApiVersion(apiVersion));

        _httpContextAccessorMock
            .SetupGet(accessor => accessor.HttpContext)
            .Returns((HttpContext)null);

        var sut = new TestJsonConverter(_httpContextAccessorMock.Object);

        // Act
        sut.Write(jsonWriter, new TestResponseModel(), new JsonSerializerOptions());
        jsonWriter.Flush();

        memoryStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(memoryStream);
        var output = reader.ReadToEnd();

        // Assert
        Assert.That(expectedPropertyNames.All(v => output.Contains(v, StringComparison.InvariantCulture)), Is.True);
    }

    /// <summary>
    /// Tests that the JSON output includes the correct properties based on the specified API version attribute.
    /// </summary>
    /// <param name="apiVersion">The API version to test against.</param>
    /// <param name="expectedPropertyNames">The property names expected to be included in the JSON output.</param>
    /// <param name="expectedDisallowedPropertyNames">The property names expected to be excluded from the JSON output.</param>
    [Test]
    [TestCase(1, new[] { "PropertyAll", "PropertyV1Max", "PropertyV2Max" }, new[] { "PropertyV2Min", "PropertyV2Only" })]
    [TestCase(2, new[] { "PropertyAll", "PropertyV2Min", "PropertyV2Only", "PropertyV2Max" }, new[] { "PropertyV1Max" })]
    [TestCase(3, new[] { "PropertyAll", "PropertyV2Min" }, new[] { "PropertyV1Max", "PropertyV2Only", "PropertyV2Max" })]
    public void Can_Include_Correct_Properties_Based_On_Version_Attribute(int apiVersion, string[] expectedPropertyNames, string[] expectedDisallowedPropertyNames)
    {
        var jsonOptions = new JsonSerializerOptions();
        var output = GetJsonOutput(apiVersion, jsonOptions);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(expectedPropertyNames.All(v => output.Contains(v, StringComparison.InvariantCulture)), Is.True);
            Assert.That(expectedDisallowedPropertyNames.All(v => output.Contains(v, StringComparison.InvariantCulture) is false), Is.True);
        });
    }

    /// <summary>
    /// Tests that properties are serialized correctly based on the API version attribute.
    /// </summary>
    /// <param name="apiVersion">The API version to test serialization against.</param>
    /// <param name="expectedPropertyNames">The expected property names to be serialized for the given API version.</param>
    [Test]
    [TestCase(1, new[] { "PropertyAll", "PropertyV1Max", "PropertyV2Max" })]
    [TestCase(2, new[] { "PropertyAll", "PropertyV2Min", "PropertyV2Only", "PropertyV2Max" })]
    [TestCase(3, new[] { "PropertyAll", "PropertyV2Min" })]
    public void Can_Serialize_Properties_Correctly_Based_On_Version_Attribute(int apiVersion, string[] expectedPropertyNames)
    {
        var jsonOptions = new JsonSerializerOptions();
        var output = GetJsonOutput(apiVersion, jsonOptions);

        // Verify values correspond to properties
        var jsonDoc = JsonDocument.Parse(output);
        var root = jsonDoc.RootElement;

        // Assert
        foreach (var propertyName in expectedPropertyNames)
        {
            var expectedValue = GetPropertyValue(propertyName);
            Assert.AreEqual(expectedValue, root.GetProperty(propertyName).GetString());
        }
    }

    /// <summary>
    /// Tests that the JSON serialization respects the property naming policy specified in the JSON options for different API versions.
    /// </summary>
    /// <param name="apiVersion">The API version to test against.</param>
    /// <param name="expectedPropertyNames">The property names expected to be present in the JSON output.</param>
    /// <param name="expectedDisallowedPropertyNames">The property names expected to be absent from the JSON output.</param>
    [Test]
    [TestCase(1, new[] { "propertyAll", "propertyV1Max", "propertyV2Max" }, new[] { "propertyV2Min", "propertyV2Only" })]
    [TestCase(2, new[] { "propertyAll", "propertyV2Min", "propertyV2Only", "propertyV2Max" }, new[] { "propertyV1Max" })]
    [TestCase(3, new[] { "propertyAll", "propertyV2Min" }, new[] { "propertyV1Max", "propertyV2Only", "propertyV2Max" })]
    public void Can_Respect_Property_Naming_Policy_On_Json_Options(int apiVersion, string[] expectedPropertyNames, string[] expectedDisallowedPropertyNames)
    {
        // Set up CamelCase naming policy
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var output = GetJsonOutput(apiVersion, jsonOptions);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(expectedPropertyNames.All(v => output.Contains(v, StringComparison.InvariantCulture)), Is.True);
            Assert.That(expectedDisallowedPropertyNames.All(v => output.Contains(v, StringComparison.InvariantCulture) is false), Is.True);
        });
    }

    /// <summary>
    /// Tests that the JSON output respects the property order based on the API version.
    /// </summary>
    /// <param name="apiVersion">The API version to test against.</param>
    /// <param name="expectedFirstPropertyName">The expected name of the first property in the JSON output.</param>
    /// <param name="expectedLastPropertyName">The expected name of the last property in the JSON output.</param>
    [Test]
    [TestCase(1, "PropertyV1Max", "PropertyAll")]
    [TestCase(2, "PropertyV2Min", "PropertyAll")]
    public void Can_Respect_Property_Order(int apiVersion, string expectedFirstPropertyName, string expectedLastPropertyName)
    {
        var jsonOptions = new JsonSerializerOptions();
        var output = GetJsonOutput(apiVersion, jsonOptions);

        // Parse the JSON to verify the order of properties
        using var jsonDocument = JsonDocument.Parse(output);
        var rootElement = jsonDocument.RootElement;

        var properties = rootElement.EnumerateObject().ToList();
        var firstProperty = properties.First();
        var lastProperty = properties.Last();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedFirstPropertyName, firstProperty.Name);
            Assert.AreEqual(expectedLastPropertyName, lastProperty.Name);
        });
    }

    private string GetJsonOutput(int apiVersion, JsonSerializerOptions jsonOptions)
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using var jsonWriter = new Utf8JsonWriter(memoryStream);

        SetUpMocks(apiVersion);
        var sut = new TestJsonConverter(_httpContextAccessorMock.Object);

        // Act
        sut.Write(jsonWriter, new TestResponseModel(), jsonOptions);
        jsonWriter.Flush();

        memoryStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(memoryStream);

        return reader.ReadToEnd();
    }

    private string GetPropertyValue(string propertyName)
    {
        var model = new TestResponseModel();
        return propertyName switch
        {
            nameof(TestResponseModel.PropertyAll) => model.PropertyAll,
            nameof(TestResponseModel.PropertyV1Max) => model.PropertyV1Max,
            nameof(TestResponseModel.PropertyV2Max) => model.PropertyV2Max,
            nameof(TestResponseModel.PropertyV2Min) => model.PropertyV2Min,
            nameof(TestResponseModel.PropertyV2Only) => model.PropertyV2Only,
            _ => throw new ArgumentException($"Unknown property name: {propertyName}"),
        };
    }
}

internal class TestJsonConverter : DeliveryApiVersionAwareJsonConverterBase<TestResponseModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestJsonConverter"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public TestJsonConverter(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }
}

internal class TestResponseModel
{
    /// <summary>
    /// Gets the value of the PropertyAll property, which is initialized to "all".
    /// Used for testing JSON serialization order and property values in the test response model.
    /// </summary>
    [JsonPropertyOrder(100)]
    public string PropertyAll { get; init; } = "all";

    /// <summary>
    /// Gets the property value for API version 1 maximum.
    /// </summary>
    [IncludeInApiVersion(maxVersion: 1)]
    public string PropertyV1Max { get; init; } = "v1";

    /// <summary>
    /// Gets the property value for API version 2 and above.
    /// </summary>
    [IncludeInApiVersion(2)]
    public string PropertyV2Min { get; init; } = "v2+";

    /// <summary>
    /// Gets the property that is only included in API version 2.
    /// </summary>
    [IncludeInApiVersion(2, 2)]
    public string PropertyV2Only { get; init; } = "v2";

    /// <summary>Gets the property included up to API version 2.</summary>
    [IncludeInApiVersion(maxVersion: 2)]
    public string PropertyV2Max { get; init; } = "up to v2";
}
