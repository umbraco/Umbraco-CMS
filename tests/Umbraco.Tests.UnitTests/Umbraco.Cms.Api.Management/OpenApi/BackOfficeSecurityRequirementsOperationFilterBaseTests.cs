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

    [Test]
    public void Does_Not_Apply_Security_Requirement_When_Mapped_To_Another_Api()
    {
        OpenApiOperation operation = Apply(typeof(UnmappedController), new MapToApiAttribute("other"));

        Assert.Multiple(() =>
        {
            Assert.That(operation.Security, Is.Null.Or.Empty);
            Assert.That(operation.Responses, Is.Null.Or.Empty);
        });
    }

    [TestCase("other", ApiName, true)]
    [TestCase(ApiName, "other", false)]
    public void Uses_The_Last_MapToApi_In_Endpoint_Metadata(string first, string last, bool expectSecurityRequirement)
    {
        OpenApiOperation operation = Apply(typeof(UnmappedController), new MapToApiAttribute(first), new MapToApiAttribute(last));

        Assert.That(operation.Security?.Count > 0, Is.EqualTo(expectSecurityRequirement));
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public void Adds_Forbidden_Response_Only_When_Endpoint_Has_More_AuthorizeAttributes_Than_The_Base_Controller(int numberOfAuthorizeAttributes, bool expectForbidden)
    {
        object[] endpointMetadata = Enumerable.Range(0, numberOfAuthorizeAttributes)
            .Select(_ => (object)new AuthorizeAttribute())
            .Prepend(new MapToApiAttribute(ApiName))
            .ToArray();

        OpenApiOperation operation = Apply(typeof(UnmappedController), endpointMetadata);

        Assert.That(
            operation.Responses?.ContainsKey(StatusCodes.Status403Forbidden.ToString()) ?? false,
            Is.EqualTo(expectForbidden));
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
