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

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.OpenApi;

[TestFixture]
internal sealed class BackOfficeSecurityRequirementsOperationFilterBaseTests
{
    private const string ApiName = "custom";

    [Test]
    public void Applies_Security_Requirement_When_MapToApi_Is_Only_In_Endpoint_Metadata()
    {
        OpenApiOperation operation = Apply(typeof(UnmappedController), new MapToApiAttribute(ApiName));

        Assert.Multiple(() =>
        {
            Assert.That(operation.Security, Has.Count.EqualTo(1));
            Assert.That(operation.Responses?.Keys, Does.Contain(StatusCodes.Status401Unauthorized.ToString()));
        });
    }

    [Test]
    public void Does_Not_Apply_Security_Requirement_When_AllowAnonymous_Is_In_Endpoint_Metadata()
    {
        OpenApiOperation operation = Apply(
            typeof(MappedController),
            typeof(MappedController).GetCustomAttribute<MapToApiAttribute>()!,
            new AllowAnonymousAttribute());

        Assert.Multiple(() =>
        {
            Assert.That(operation.Security, Is.Null.Or.Empty);
            Assert.That(operation.Responses?.Keys, Does.Not.Contain(StatusCodes.Status401Unauthorized.ToString()));
        });
    }

    private static OpenApiOperation Apply(Type controllerType, params object[] endpointMetadata)
    {
        MethodInfo methodInfo = controllerType.GetMethod("Action")!;
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                MethodInfo = methodInfo,
                EndpointMetadata = endpointMetadata,
            },
        };
        var context = new OperationFilterContext(
            apiDescription,
            Mock.Of<ISchemaGenerator>(),
            new SchemaRepository(),
            new OpenApiDocument(),
            methodInfo);

        var operation = new OpenApiOperation();
        new TestSecurityRequirementsOperationFilter().Apply(operation, context);
        return operation;
    }

    private sealed class TestSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
    {
        protected override string ApiName => BackOfficeSecurityRequirementsOperationFilterBaseTests.ApiName;
    }

    private sealed class UnmappedController
    {
        public void Action()
        {
        }
    }

    [MapToApi(ApiName)]
    private sealed class MappedController
    {
        public void Action()
        {
        }
    }
}
