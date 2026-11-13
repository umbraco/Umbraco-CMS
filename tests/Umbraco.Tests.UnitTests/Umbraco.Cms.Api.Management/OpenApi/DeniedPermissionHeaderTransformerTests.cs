using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.OpenApi.Transformers;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.OpenApi;

[TestFixture]
internal sealed class DeniedPermissionHeaderTransformerTests
{
    private static readonly string Forbidden = StatusCodes.Status403Forbidden.ToString();

    [Test]
    public async Task Documents_The_Forbidden_Response_With_The_Header_When_Environment_Permissions_Can_Deny_The_Operation()
    {
        OpenApiOperationTransformerContext context = CreateContext(typeof(GovernedController), "POST");
        var operation = new OpenApiOperation();

        await new DeniedPermissionHeaderTransformer().TransformAsync(operation, context, CancellationToken.None);

        IOpenApiResponse? forbidden = operation.Responses?.GetValueOrDefault(Forbidden);
        Assert.Multiple(() =>
        {
            Assert.That(forbidden?.Headers?.Keys, Does.Contain(Constants.Headers.DeniedPermission));
            Assert.That(
                forbidden?.Headers?[Constants.Headers.DeniedPermission].Description,
                Does.Contain(Constants.Permissions.Entities.DocumentType.Create));
        });
    }

    [Test]
    public async Task Documents_The_Header_For_An_Action_That_Declares_Its_Permissions_On_Its_Own()
    {
        OpenApiOperationTransformerContext context = CreateContext(typeof(DeclaringController), "POST");
        var operation = new OpenApiOperation();

        await new DeniedPermissionHeaderTransformer().TransformAsync(operation, context, CancellationToken.None);

        Assert.That(
            operation.Responses?.GetValueOrDefault(Forbidden)?.Headers?[Constants.Headers.DeniedPermission].Description,
            Does.Contain(Constants.Permissions.Entities.Template.Create));
    }

    [Test]
    public async Task Adds_The_Header_To_An_Already_Documented_Forbidden_Response()
    {
        OpenApiOperationTransformerContext context = CreateContext(typeof(GovernedController), "PUT");
        var documented = new OpenApiResponse { Description = "Documented elsewhere" };
        var operation = new OpenApiOperation { Responses = new OpenApiResponses { [Forbidden] = documented } };

        await new DeniedPermissionHeaderTransformer().TransformAsync(operation, context, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(operation.Responses[Forbidden], Is.SameAs(documented));
            Assert.That(documented.Headers?.Keys, Does.Contain(Constants.Headers.DeniedPermission));
        });
    }

    [TestCase(typeof(GovernedController), "GET")]
    [TestCase(typeof(GovernedReadController), "POST")]
    [TestCase(typeof(UngovernedController), "POST")]
    public async Task Leaves_An_Operation_Environment_Permissions_Cannot_Deny_Untouched(Type controllerType, string httpMethod)
    {
        OpenApiOperationTransformerContext context = CreateContext(controllerType, httpMethod);
        var operation = new OpenApiOperation();

        await new DeniedPermissionHeaderTransformer().TransformAsync(operation, context, CancellationToken.None);

        Assert.That(operation.Responses, Is.Null.Or.Empty);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Documents_One_Forbidden_Response_With_The_Header_Whichever_Transformer_Runs_First(bool securityTransformerFirst)
    {
        OpenApiOperationTransformerContext context = CreateContext(
            typeof(GovernedController),
            "PUT",
            new AuthorizeAttribute(),
            new AuthorizeAttribute(),
            new AuthorizeAttribute());
        var operation = new OpenApiOperation();
        IOpenApiOperationTransformer[] transformers = securityTransformerFirst
            ? [new BackOfficeSecurityRequirementsTransformer(), new DeniedPermissionHeaderTransformer()]
            : [new DeniedPermissionHeaderTransformer(), new BackOfficeSecurityRequirementsTransformer()];

        foreach (IOpenApiOperationTransformer transformer in transformers)
        {
            await transformer.TransformAsync(operation, context, CancellationToken.None);
        }

        Assert.That(operation.Responses?[Forbidden].Headers?.Keys, Does.Contain(Constants.Headers.DeniedPermission));
    }

    private static OpenApiOperationTransformerContext CreateContext(Type controllerType, string httpMethod, params object[] endpointMetadata)
    {
        MethodInfo methodInfo = controllerType.GetMethod("Action")!;
        return new OpenApiOperationTransformerContext
        {
            Document = new OpenApiDocument(),
            Description = new ApiDescription
            {
                HttpMethod = httpMethod,
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = controllerType.GetTypeInfo(),
                    MethodInfo = methodInfo,
                    EndpointMetadata = endpointMetadata,
                },
            },
            DocumentName = "test",
            ApplicationServices = new ServiceCollection().BuildServiceProvider(),
        };
    }

    [RequiresPermissionsFor(Constants.Permissions.Entities.DocumentType.Resource)]
    private sealed class GovernedController
    {
        public void Action()
        {
        }
    }

    [RequiresPermissionsFor(Constants.Permissions.Entities.DocumentType.Resource)]
    private sealed class GovernedReadController
    {
        [RequiresPermissions([])]
        public void Action()
        {
        }
    }

    private sealed class DeclaringController
    {
        [RequiresPermissions([Constants.Permissions.Entities.Template.Create])]
        public void Action()
        {
        }
    }

    private sealed class UngovernedController
    {
        public void Action()
        {
        }
    }
}
