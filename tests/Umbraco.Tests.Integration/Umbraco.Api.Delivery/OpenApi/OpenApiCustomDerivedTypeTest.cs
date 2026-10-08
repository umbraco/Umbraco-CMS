using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Delivery.OpenApi;

/// <summary>
/// Tests that the OpenAPI specification remains valid when a custom derived type
/// is registered via <see cref="ContentJsonTypeResolverBase.GetDerivedTypes"/>.
/// This simulates the extensibility scenario where a consumer adds a custom content response type.
/// </summary>
[TestFixture]
internal sealed class OpenApiCustomDerivedTypeTest : OpenApiTestBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // Replace the Delivery API JSON options to use our custom type resolver
        builder.Services.AddOptions<JsonOptions>(Constants.JsonOptionsNames.DeliveryApi)
            .Configure(options =>
            {
                options.JsonSerializerOptions.TypeInfoResolver = new TestDeliveryApiJsonTypeResolver();
            });
    }

    [Test]
    public async Task OpenApiDocument_IsValid_WithCustomDerivedType()
    {
        var openApiSpec = await FetchOpenApiSpecAsync();
        await ValidateOpenApiSpecAsync(openApiSpec);
    }

    [Test]
    public async Task OpenApiDocument_ContainsCustomDerivedTypeInDiscriminator()
    {
        var openApiSpec = await FetchOpenApiSpecAsync();
        var document = ParseOpenApiSpec(openApiSpec);

        var contentResponseSchema = document["components"]?["schemas"]?["IApiContentResponseModel"];
        Assert.That(contentResponseSchema, Is.Not.Null, "IApiContentResponseModel schema not found.");

        var discriminator = contentResponseSchema!["discriminator"];
        Assert.That(discriminator, Is.Not.Null, "Discriminator not found on IApiContentResponseModel.");

        Assert.That(discriminator!["propertyName"]?.GetValue<string>(), Is.EqualTo("$type"), "Discriminator property name should be '$type'.");

        var mapping = discriminator["mapping"];
        Assert.That(mapping, Is.Not.Null, "Discriminator mapping not found.");

        // Verify both the default and custom type are in the mapping
        Assert.That(mapping!["ApiContentResponse"], Is.Not.Null, "Default ApiContentResponse should be in the discriminator mapping.");
        Assert.That(mapping!["CustomApiContentResponse"], Is.Not.Null, "Custom CustomApiContentResponse should be in the discriminator mapping.");

        var anyOf = contentResponseSchema["anyOf"]?.AsArray();
        Assert.That(anyOf, Is.Not.Null, "anyOf not found on IApiContentResponseModel.");
        Assert.That(anyOf, Has.Count.EqualTo(2), "anyOf should contain entries for both the default and custom derived types.");
    }

    /// <summary>
    /// A custom content response type that extends <see cref="ApiContentResponse"/>.
    /// This simulates a consumer adding a custom type to the Delivery API.
    /// </summary>
    private class CustomApiContentResponse : ApiContentResponse
    {
        public CustomApiContentResponse(
            Guid id,
            string name,
            string contentType,
            DateTime createDate,
            DateTime updateDate,
            IApiContentRoute route,
            IDictionary<string, object?> properties,
            IDictionary<string, IApiContentRoute> cultures)
            : base(id, name, contentType, createDate, updateDate, route, properties, cultures)
        {
        }

        public string? CustomField { get; set; }
    }

    /// <summary>
    /// A test type resolver that adds <see cref="CustomApiContentResponse"/> as an additional
    /// derived type for <see cref="IApiContentResponse"/>.
    /// </summary>
    private class TestDeliveryApiJsonTypeResolver : ContentJsonTypeResolverBase
    {
        public override Type[] GetDerivedTypes(JsonTypeInfo jsonTypeInfo) =>
            jsonTypeInfo.Type switch
            {
                _ when jsonTypeInfo.Type == typeof(IApiContentResponse) =>
                    [typeof(ApiContentResponse), typeof(CustomApiContentResponse)],
                _ => base.GetDerivedTypes(jsonTypeInfo),
            };
    }
}
