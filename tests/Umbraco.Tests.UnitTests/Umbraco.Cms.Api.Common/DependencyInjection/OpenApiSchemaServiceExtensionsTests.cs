using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.DependencyInjection;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.DependencyInjection;

[TestFixture]
internal sealed class OpenApiSchemaServiceExtensionsTests
{
    [Test]
    public void ReplaceOpenApiSchemaService_Registers_A_Factory_Keyed_By_Document_Name()
    {
        // Arrange
        const string documentName = "test-doc";
        var services = new ServiceCollection();
        services.AddOpenApi(documentName);
        List<ServiceDescriptor> sharedBefore = SchemaServiceDescriptors(services)
            .Where(sd => Equals(sd.ServiceKey, KeyedService.AnyKey))
            .ToList();

        // Act
        services.ReplaceOpenApiSchemaService(documentName, "test-json-opts");

        // Assert — exactly one registration is keyed by the document name and it is the factory-based replacement,
        // while any registration shared between documents is left untouched.
        ServiceDescriptor replacement = SchemaServiceDescriptors(services).Single(sd => Equals(sd.ServiceKey, documentName));
        Assert.IsNotNull(replacement.KeyedImplementationFactory);
        Assert.IsNull(replacement.KeyedImplementationInstance);
        Assert.IsNull(replacement.KeyedImplementationType);
        CollectionAssert.AreEqual(
            sharedBefore,
            SchemaServiceDescriptors(services).Where(sd => Equals(sd.ServiceKey, KeyedService.AnyKey)).ToList());
    }

    [Test]
    public void ReplaceOpenApiSchemaService_Replaces_An_Existing_Document_Keyed_Registration()
    {
        // Arrange
        const string documentName = "test-doc";
        var services = new ServiceCollection();
        services.AddOpenApi(documentName);
        services.ReplaceOpenApiSchemaService(documentName, "first-json-opts");
        ServiceDescriptor first = SchemaServiceDescriptors(services).Single(sd => Equals(sd.ServiceKey, documentName));

        // Act
        services.ReplaceOpenApiSchemaService(documentName, "second-json-opts");

        // Assert — still exactly one registration for the document, and it is the newer one.
        ServiceDescriptor second = SchemaServiceDescriptors(services).Single(sd => Equals(sd.ServiceKey, documentName));
        Assert.AreNotSame(first, second);
    }

    [Test]
    public void ReplaceOpenApiSchemaService_Only_Affects_Resolution_For_The_Named_Document()
    {
        // Arrange
        const string targetDocument = "target-doc";
        const string otherDocument = "other-doc";
        var services = new ServiceCollection();
        services.AddOpenApi(targetDocument);
        services.AddOpenApi(otherDocument);
        var replacementResolved = false;
        services.ReplaceOpenApiSchemaService(targetDocument, _ =>
        {
            replacementResolved = true;
            return new JsonOptions();
        });
        Type schemaServiceType = SchemaServiceDescriptors(services).First().ServiceType;
        using ServiceProvider provider = services.BuildServiceProvider();

        // Act + Assert — the other document still gets the stock service; the target gets the replacement.
        provider.GetRequiredKeyedService(schemaServiceType, otherDocument);
        Assert.IsFalse(replacementResolved);

        provider.GetRequiredKeyedService(schemaServiceType, targetDocument);
        Assert.IsTrue(replacementResolved);
    }

    [Test]
    public void ReplaceOpenApiSchemaService_Throws_When_AddOpenApi_Not_Called()
    {
        // Arrange — AddOpenApi deliberately not called.
        var services = new ServiceCollection();

        // Act + Assert
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => services.ReplaceOpenApiSchemaService("missing-doc", "json-opts"))!;
        StringAssert.Contains(OpenApiSchemaServiceExtensions.OpenApiSchemaServiceFullName, ex.Message);
        StringAssert.Contains("missing-doc", ex.Message);
    }

    private static IEnumerable<ServiceDescriptor> SchemaServiceDescriptors(IServiceCollection services)
        => services.Where(sd => sd.ServiceType.FullName == OpenApiSchemaServiceExtensions.OpenApiSchemaServiceFullName);
}
