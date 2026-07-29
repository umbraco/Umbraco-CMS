using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
internal sealed class OperationIdHandlerTests
{
    private static OperationIdHandler CreateSut()
        => new(Options.Create(new ApiVersioningOptions { DefaultApiVersion = new ApiVersion(1, 0) }));

    private static ApiDescription CreateApiDescription(string httpMethod, string relativePath, string methodName)
        => new()
        {
            HttpMethod = httpMethod,
            RelativePath = relativePath,
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = typeof(TestController).GetTypeInfo(),
                MethodInfo = typeof(TestController).GetMethod(methodName)!,
            },
        };

    [Test]
    public void Strips_the_route_prefix_of_a_minor_api_version()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "PUT",
            "umbraco/management/api/v1.1/document/{id}/validate",
            nameof(TestController.MinorVersion));

        Assert.AreEqual("PutDocumentByIdValidate11", CreateSut().Handle(apiDescription));
    }

    [Test]
    public void Strips_the_route_prefix_of_a_multi_digit_api_version()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "GET",
            "umbraco/management/api/v10/thing",
            nameof(TestController.DefaultVersion));

        Assert.AreEqual("GetThing", CreateSut().Handle(apiDescription));
    }

    [Test]
    public void Does_not_append_a_suffix_for_the_default_api_version()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "PUT",
            "umbraco/management/api/v1/media/{id}/validate",
            nameof(TestController.DefaultVersion));

        Assert.AreEqual("PutMediaByIdValidate", CreateSut().Handle(apiDescription));
    }

    private sealed class TestController
    {
        [MapToApiVersion("1.1")]
        public void MinorVersion()
        {
        }

        [MapToApiVersion("1.0")]
        public void DefaultVersion()
        {
        }
    }
}
