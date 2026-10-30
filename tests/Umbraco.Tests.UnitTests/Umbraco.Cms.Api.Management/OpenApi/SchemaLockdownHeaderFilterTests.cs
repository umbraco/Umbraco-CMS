using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Moq;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.OpenApi;

[TestFixture]
internal sealed class SchemaLockdownHeaderFilterTests
{
    private const string ApiName = "custom";

    private static readonly string Forbidden = StatusCodes.Status403Forbidden.ToString();

    [Test]
    public void Documents_The_Forbidden_Response_With_The_Header_When_Schema_Lockdown_Can_Deny_The_Operation()
    {
        OperationFilterContext context = CreateContext(typeof(GovernedController), "POST");
        var operation = new OpenApiOperation();

        new SchemaLockdownHeaderFilter().Apply(operation, context);

        IOpenApiResponse? forbidden = operation.Responses?.GetValueOrDefault(Forbidden);
        Assert.Multiple(() =>
        {
            Assert.That(forbidden?.Headers?.Keys, Does.Contain(Constants.Headers.SchemaLockdown));
            Assert.That(
                forbidden?.Headers?[Constants.Headers.SchemaLockdown].Description,
                Does.Contain("document-type:create"));
        });
    }

    [Test]
    public void Adds_The_Header_To_An_Already_Documented_Forbidden_Response()
    {
        OperationFilterContext context = CreateContext(typeof(GovernedController), "PUT");
        var documented = new OpenApiResponse { Description = "Documented elsewhere" };
        var operation = new OpenApiOperation { Responses = new OpenApiResponses { [Forbidden] = documented } };

        new SchemaLockdownHeaderFilter().Apply(operation, context);

        Assert.Multiple(() =>
        {
            Assert.That(operation.Responses[Forbidden], Is.SameAs(documented));
            Assert.That(documented.Headers?.Keys, Does.Contain(Constants.Headers.SchemaLockdown));
        });
    }

    [TestCase(typeof(GovernedController), "GET")]
    [TestCase(typeof(GovernedReadController), "POST")]
    [TestCase(typeof(UngovernedController), "POST")]
    public void Leaves_An_Operation_Schema_Lockdown_Cannot_Deny_Untouched(Type controllerType, string httpMethod)
    {
        OperationFilterContext context = CreateContext(controllerType, httpMethod);
        var operation = new OpenApiOperation();

        new SchemaLockdownHeaderFilter().Apply(operation, context);

        Assert.That(operation.Responses, Is.Null.Or.Empty);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Documents_One_Forbidden_Response_With_The_Header_Whichever_Filter_Runs_First(bool securityFilterFirst)
    {
        OperationFilterContext context = CreateContext(
            typeof(GovernedController),
            "PUT",
            new MapToApiAttribute(ApiName),
            new AuthorizeAttribute(),
            new AuthorizeAttribute(),
            new AuthorizeAttribute());
        var operation = new OpenApiOperation();
        IOperationFilter[] filters = securityFilterFirst
            ? [new TestSecurityRequirementsOperationFilter(), new SchemaLockdownHeaderFilter()]
            : [new SchemaLockdownHeaderFilter(), new TestSecurityRequirementsOperationFilter()];

        foreach (IOperationFilter filter in filters)
        {
            filter.Apply(operation, context);
        }

        Assert.That(operation.Responses?[Forbidden].Headers?.Keys, Does.Contain(Constants.Headers.SchemaLockdown));
    }

    private static OperationFilterContext CreateContext(Type controllerType, string httpMethod, params object[] endpointMetadata)
    {
        MethodInfo methodInfo = controllerType.GetMethod("Action")!;
        var apiDescription = new ApiDescription
        {
            HttpMethod = httpMethod,
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                MethodInfo = methodInfo,
                EndpointMetadata = endpointMetadata,
            },
        };
        return new OperationFilterContext(
            apiDescription,
            Mock.Of<ISchemaGenerator>(),
            new SchemaRepository(),
            new OpenApiDocument(),
            methodInfo);
    }

    private sealed class TestSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
    {
        protected override string ApiName => SchemaLockdownHeaderFilterTests.ApiName;
    }

    [SchemaEntityType(Constants.UdiEntityType.DocumentType)]
    private sealed class GovernedController
    {
        public void Action()
        {
        }
    }

    [SchemaEntityType(Constants.UdiEntityType.DocumentType)]
    private sealed class GovernedReadController
    {
        [SchemaOperation(SchemaOperation.Read)]
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
