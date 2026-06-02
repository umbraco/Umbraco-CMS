using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.DependencyInjection;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.DependencyInjection;

[TestFixture]
internal sealed class OpenApiSchemaServiceExtensionsTests
{
    [Test]
    public void ReplaceOpenApiSchemaService_Replaces_Keyed_Registration_For_Document()
    {
        // Arrange
        const string documentName = "test-doc";
        const string jsonOptionsName = "test-json-opts";
        var services = new ServiceCollection();
        services.AddOpenApi(documentName);

        ServiceDescriptor originalDescriptor = services.Single(sd =>
            sd.ServiceType.FullName == OpenApiSchemaServiceExtensions.OpenApiSchemaServiceFullName
            && Equals(sd.ServiceKey, documentName));

        // Act
        services.ReplaceOpenApiSchemaService(documentName, jsonOptionsName);

        // Assert — exactly one registration remains for the internal type keyed by document name,
        // but it is the replacement (factory-based, not the original).
        ServiceDescriptor replacedDescriptor = services.Single(sd =>
            sd.ServiceType.FullName == OpenApiSchemaServiceExtensions.OpenApiSchemaServiceFullName
            && Equals(sd.ServiceKey, documentName));

        Assert.AreNotSame(originalDescriptor, replacedDescriptor);
        Assert.IsNotNull(replacedDescriptor.KeyedImplementationFactory);
        Assert.IsNull(replacedDescriptor.KeyedImplementationInstance);
        Assert.IsNull(replacedDescriptor.KeyedImplementationType);
    }

    [Test]
    public void ReplaceOpenApiSchemaService_Leaves_Other_Document_Registrations_Intact()
    {
        // Arrange
        const string targetDocument = "target-doc";
        const string otherDocument = "other-doc";
        var services = new ServiceCollection();
        services.AddOpenApi(targetDocument);
        services.AddOpenApi(otherDocument);

        ServiceDescriptor otherOriginal = services.Single(sd =>
            sd.ServiceType.FullName == OpenApiSchemaServiceExtensions.OpenApiSchemaServiceFullName
            && Equals(sd.ServiceKey, otherDocument));

        // Act
        services.ReplaceOpenApiSchemaService(targetDocument, "json-opts");

        // Assert — the other document's registration is untouched.
        ServiceDescriptor otherAfter = services.Single(sd =>
            sd.ServiceType.FullName == OpenApiSchemaServiceExtensions.OpenApiSchemaServiceFullName
            && Equals(sd.ServiceKey, otherDocument));
        Assert.AreSame(otherOriginal, otherAfter);
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

    [Test]
    public void ReplaceOpenApiSchemaService_Throws_When_Document_Name_Does_Not_Match()
    {
        // Arrange — a different document was registered.
        var services = new ServiceCollection();
        services.AddOpenApi("registered-doc");

        // Act + Assert
        Assert.Throws<InvalidOperationException>(
            () => services.ReplaceOpenApiSchemaService("unregistered-doc", "json-opts"));
    }
}
