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
    public void Can_Strip_Route_Prefix_Of_Minor_Api_Version()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "PUT",
            "umbraco/management/api/v1.1/document/{id}/validate",
            nameof(TestController.MinorVersion));

        Assert.AreEqual("PutDocumentByIdValidate11", CreateSut().Handle(apiDescription));
    }

    [Test]
    public void Can_Strip_Route_Prefix_Of_Multi_Digit_Api_Version()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "GET",
            "umbraco/management/api/v10/thing",
            nameof(TestController.DefaultVersion));

        Assert.AreEqual("GetThing", CreateSut().Handle(apiDescription));
    }

    [Test]
    public void Can_Omit_Version_Suffix_For_Default_Api_Version()
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
