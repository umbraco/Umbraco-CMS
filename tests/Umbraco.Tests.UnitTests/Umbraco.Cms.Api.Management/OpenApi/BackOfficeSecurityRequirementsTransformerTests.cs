using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi.Transformers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.OpenApi;

[TestFixture]
public class BackOfficeSecurityRequirementsTransformerTests
{
    private BackOfficeSecurityRequirementsTransformer _transformer = null!;
    private IServiceProvider _services = null!;

    [SetUp]
    public void SetUp()
    {
        _transformer = new BackOfficeSecurityRequirementsTransformer();
        _services = new ServiceCollection().BuildServiceProvider();
    }

    #region Document Transformer Tests

    [Test]
    public async Task TransformAsync_Document_Creates_OAuth2_Security_Scheme()
    {
        // Arrange
        var document = new OpenApiDocument();

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.IsNotNull(document.Components);
        Assert.IsNotNull(document.Components.SecuritySchemes);
        Assert.IsTrue(document.Components.SecuritySchemes.ContainsKey("Backoffice-User"));

        var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["Backoffice-User"];
        Assert.AreEqual(SecuritySchemeType.OAuth2, scheme.Type);
        Assert.AreEqual("Umbraco", scheme.Name);
        Assert.AreEqual(ParameterLocation.Header, scheme.In);
    }

    [Test]
    public async Task TransformAsync_Document_Configures_AuthorizationCode_Flow()
    {
        // Arrange
        var document = new OpenApiDocument();

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        var scheme = (OpenApiSecurityScheme)document.Components!.SecuritySchemes!["Backoffice-User"];
        Assert.IsNotNull(scheme.Flows);
        Assert.IsNotNull(scheme.Flows.AuthorizationCode);
        Assert.IsNotNull(scheme.Flows.AuthorizationCode.AuthorizationUrl);
        Assert.IsNotNull(scheme.Flows.AuthorizationCode.TokenUrl);
    }

    [Test]
    public async Task TransformAsync_Document_Adds_Security_Requirement()
    {
        // Arrange
        var document = new OpenApiDocument();

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.IsNotNull(document.Security);
        Assert.AreEqual(1, document.Security.Count);
    }

    [Test]
    public async Task TransformAsync_Document_Preserves_Existing_Components()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["ExistingScheme"] = new OpenApiSecurityScheme { Name = "Existing" },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, document.Components.SecuritySchemes.Count);
        Assert.IsTrue(document.Components.SecuritySchemes.ContainsKey("ExistingScheme"));
        Assert.IsTrue(document.Components.SecuritySchemes.ContainsKey("Backoffice-User"));
    }

    #endregion

    #region Operation Transformer Tests

    [Test]
    public async Task TransformAsync_Operation_Overrides_Security_For_AllowAnonymous_Methods()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var methodInfo = typeof(TestControllerWithAllowAnonymousMethod).GetMethod(nameof(TestControllerWithAllowAnonymousMethod.AnonymousAction))!;
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = methodInfo,
                ControllerTypeInfo = typeof(TestControllerWithAllowAnonymousMethod).GetTypeInfo(),
                EndpointMetadata = EndpointMetadataOf(methodInfo),
            },
        };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert - Should not add 401 response, and security must be an empty list to override document-level security
        Assert.IsFalse(operation.Responses?.ContainsKey(StatusCodes.Status401Unauthorized.ToString()) ?? false);
        Assert.IsNotNull(operation.Security);
        Assert.IsEmpty(operation.Security);
    }

    [Test]
    public async Task TransformAsync_Operation_Overrides_Security_For_AllowAnonymous_Controllers()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var methodInfo = typeof(AllowAnonymousController).GetMethod(nameof(AllowAnonymousController.SomeAction))!;
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = methodInfo,
                ControllerTypeInfo = typeof(AllowAnonymousController).GetTypeInfo(),
                EndpointMetadata = EndpointMetadataOf(methodInfo),
            },
        };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert - Should not add 401 response, and security must be an empty list to override document-level security
        Assert.IsFalse(operation.Responses?.ContainsKey(StatusCodes.Status401Unauthorized.ToString()) ?? false);
        Assert.IsNotNull(operation.Security);
        Assert.IsEmpty(operation.Security);
    }

    [Test]
    public async Task TransformAsync_Operation_Adds_401_Response_For_Authorized_Endpoints()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var methodInfo = typeof(AuthorizedController).GetMethod(nameof(AuthorizedController.SecureAction))!;
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = methodInfo,
                ControllerTypeInfo = typeof(AuthorizedController).GetTypeInfo(),
                EndpointMetadata = EndpointMetadataOf(methodInfo),
            },
        };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.IsNotNull(operation.Responses);
        Assert.IsTrue(operation.Responses.ContainsKey(StatusCodes.Status401Unauthorized.ToString()));
    }

    [Test]
    public async Task TransformAsync_Operation_Adds_Security_Requirement_For_Authorized_Endpoints()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var methodInfo = typeof(AuthorizedController).GetMethod(nameof(AuthorizedController.SecureAction))!;
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = methodInfo,
                ControllerTypeInfo = typeof(AuthorizedController).GetTypeInfo(),
                EndpointMetadata = EndpointMetadataOf(methodInfo),
            },
        };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.IsNotNull(operation.Security);
        Assert.AreEqual(1, operation.Security.Count);
    }

    [Test]
    public async Task TransformAsync_Operation_Adds_403_Response_For_Multiple_Authorize_Attributes()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var methodInfo = typeof(MultipleAuthorizeController).GetMethod(nameof(MultipleAuthorizeController.RestrictedAction))!;
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = methodInfo,
                ControllerTypeInfo = typeof(MultipleAuthorizeController).GetTypeInfo(),
                EndpointMetadata = EndpointMetadataOf(methodInfo),
            },
        };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert - Should have both 401 and 403
        Assert.IsNotNull(operation.Responses);
        Assert.IsTrue(operation.Responses.ContainsKey(StatusCodes.Status401Unauthorized.ToString()));
        Assert.IsTrue(operation.Responses.ContainsKey(StatusCodes.Status403Forbidden.ToString()));
    }

    [Test]
    public async Task TransformAsync_Operation_Adds_403_Response_For_Controller_With_IAuthorizationService_Injection()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var methodInfo = typeof(ControllerWithAuthorizationServiceInjection).GetMethod(nameof(ControllerWithAuthorizationServiceInjection.SomeAction))!;
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = methodInfo,
                ControllerTypeInfo = typeof(ControllerWithAuthorizationServiceInjection).GetTypeInfo(),
                EndpointMetadata = EndpointMetadataOf(methodInfo),
            },
        };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert - Should have both 401 and 403 because IAuthorizationService is injected
        Assert.IsNotNull(operation.Responses);
        Assert.IsTrue(operation.Responses.ContainsKey(StatusCodes.Status401Unauthorized.ToString()));
        Assert.IsTrue(operation.Responses.ContainsKey(StatusCodes.Status403Forbidden.ToString()));
    }


    [Test]
    public async Task TransformAsync_Operation_Overrides_Security_When_AllowAnonymous_Is_Only_In_Endpoint_Metadata()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var methodInfo = typeof(AuthorizedController).GetMethod(nameof(AuthorizedController.SecureAction))!;
        OpenApiOperationTransformerContext context = CreateContext(methodInfo, new AllowAnonymousAttribute());

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.IsFalse(operation.Responses?.ContainsKey(StatusCodes.Status401Unauthorized.ToString()) ?? false);
        Assert.IsNotNull(operation.Security);
        Assert.IsEmpty(operation.Security);
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task TransformAsync_Operation_Adds_403_Response_Only_When_Endpoint_Metadata_Has_More_AuthorizeAttributes_Than_The_Base_Controller(int numberOfAuthorizeAttributes, bool expectForbidden)
    {
        // Arrange
        var operation = new OpenApiOperation();
        var methodInfo = typeof(AuthorizedController).GetMethod(nameof(AuthorizedController.SecureAction))!;
        object[] endpointMetadata = Enumerable.Range(0, numberOfAuthorizeAttributes)
            .Select(_ => (object)new AuthorizeAttribute())
            .ToArray();
        OpenApiOperationTransformerContext context = CreateContext(methodInfo, endpointMetadata);

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.AreEqual(expectForbidden, operation.Responses?.ContainsKey(StatusCodes.Status403Forbidden.ToString()) ?? false);
    }

    #endregion

    // Mirrors how ASP.NET Core builds endpoint metadata: controller attributes, then action attributes.
    private static IList<object> EndpointMetadataOf(MethodInfo methodInfo)
        => (methodInfo.DeclaringType?.GetCustomAttributes(true) ?? [])
            .Concat(methodInfo.GetCustomAttributes(true))
            .ToList();

    private OpenApiOperationTransformerContext CreateContext(MethodInfo methodInfo, params object[] endpointMetadata)
        => new()
        {
            Document = new OpenApiDocument(),
            Description = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    MethodInfo = methodInfo,
                    ControllerTypeInfo = methodInfo.DeclaringType!.GetTypeInfo(),
                    EndpointMetadata = endpointMetadata,
                },
            },
            DocumentName = "test",
            ApplicationServices = _services,
        };

    #region Test Helper Classes

    private class TestControllerWithAllowAnonymousMethod : Controller
    {
        [AllowAnonymous]
        public IActionResult AnonymousAction() => Ok();
    }

    [AllowAnonymous]
    private class AllowAnonymousController : Controller
    {
        public IActionResult SomeAction() => Ok();
    }

    private class AuthorizedController : Controller
    {
        public IActionResult SecureAction() => Ok();
    }

    [Authorize]
    [Authorize(Policy = "Policy1")]
    [Authorize(Policy = "Policy2")]
    private class MultipleAuthorizeController : Controller
    {
        public IActionResult RestrictedAction() => Ok();
    }

    private class ControllerWithAuthorizationServiceInjection : Controller
    {
        public ControllerWithAuthorizationServiceInjection(IAuthorizationService authorizationService)
        {
        }

        public IActionResult SomeAction() => Ok();
    }

    #endregion
}
