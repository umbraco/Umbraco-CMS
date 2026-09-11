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
        Assert.That(document.Components, Is.Not.Null);
        Assert.That(document.Components.SecuritySchemes, Is.Not.Null);
        Assert.That(document.Components.SecuritySchemes.ContainsKey("Backoffice-User"), Is.True);

        var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["Backoffice-User"];
        Assert.That(scheme.Type, Is.EqualTo(SecuritySchemeType.OAuth2));
        Assert.That(scheme.Name, Is.EqualTo("Umbraco"));
        Assert.That(scheme.In, Is.EqualTo(ParameterLocation.Header));
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
        Assert.That(scheme.Flows, Is.Not.Null);
        Assert.That(scheme.Flows.AuthorizationCode, Is.Not.Null);
        Assert.That(scheme.Flows.AuthorizationCode.AuthorizationUrl, Is.Not.Null);
        Assert.That(scheme.Flows.AuthorizationCode.TokenUrl, Is.Not.Null);
    }

    [Test]
    public async Task TransformAsync_Document_Adds_Security_Requirement()
    {
        // Arrange
        var document = new OpenApiDocument();

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.That(document.Security, Is.Not.Null);
        Assert.That(document.Security, Has.Count.EqualTo(1));
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
        Assert.That(document.Components.SecuritySchemes, Has.Count.EqualTo(2));
        Assert.That(document.Components.SecuritySchemes.ContainsKey("ExistingScheme"), Is.True);
        Assert.That(document.Components.SecuritySchemes.ContainsKey("Backoffice-User"), Is.True);
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
        Assert.That(operation.Responses?.ContainsKey(StatusCodes.Status401Unauthorized.ToString()) ?? false, Is.False);
        Assert.That(operation.Security, Is.Not.Null);
        Assert.That(operation.Security, Is.Empty);
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
        Assert.That(operation.Responses?.ContainsKey(StatusCodes.Status401Unauthorized.ToString()) ?? false, Is.False);
        Assert.That(operation.Security, Is.Not.Null);
        Assert.That(operation.Security, Is.Empty);
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
        Assert.That(operation.Responses, Is.Not.Null);
        Assert.That(operation.Responses.ContainsKey(StatusCodes.Status401Unauthorized.ToString()), Is.True);
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
        Assert.That(operation.Security, Is.Not.Null);
        Assert.That(operation.Security, Has.Count.EqualTo(1));
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
        Assert.That(operation.Responses, Is.Not.Null);
        Assert.That(operation.Responses.ContainsKey(StatusCodes.Status401Unauthorized.ToString()), Is.True);
        Assert.That(operation.Responses.ContainsKey(StatusCodes.Status403Forbidden.ToString()), Is.True);
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
        Assert.That(operation.Responses, Is.Not.Null);
        Assert.That(operation.Responses.ContainsKey(StatusCodes.Status401Unauthorized.ToString()), Is.True);
        Assert.That(operation.Responses.ContainsKey(StatusCodes.Status403Forbidden.ToString()), Is.True);
    }

    #endregion

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
