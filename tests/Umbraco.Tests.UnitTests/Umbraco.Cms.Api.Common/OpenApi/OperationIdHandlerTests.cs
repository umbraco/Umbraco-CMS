using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Cms.Api.Management.Controllers.Document;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
internal sealed class OperationIdHandlerTests
{
    private static OperationIdHandler CreateSut()
        => new(Options.Create(new ApiVersioningOptions { DefaultApiVersion = new ApiVersion(1, 0) }));

    // The controller type only matters for its namespace, which decides whether the version suffix is
    // sanitised (Management API) or left alone (Delivery API). Real controllers are used so the tests
    // track the namespaces that actually ship.
    private static ApiDescription CreateApiDescription(
        string httpMethod,
        string relativePath,
        string methodName,
        Type? controllerType = null,
        string? routeName = null)
        => new()
        {
            HttpMethod = httpMethod,
            RelativePath = relativePath,
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = (controllerType ?? typeof(ValidateUpdateDocumentController)).GetTypeInfo(),
                MethodInfo = typeof(TestController).GetMethod(methodName)!,
                AttributeRouteInfo = routeName is null ? null : new AttributeRouteInfo { Name = routeName },
            },
        };

    [Test]
    public void Can_Substitute_The_Dot_In_A_Minor_Api_Version()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "PUT",
            "umbraco/management/api/v1.1/document/{id}/validate",
            nameof(TestController.MinorVersion));

        // Substituted rather than removed, so it cannot collide with a genuine "11" version.
        Assert.AreEqual("PutDocumentByIdValidate1_1", CreateSut().Handle(apiDescription));
    }

    [Test]
    public void Can_Leave_The_Delivery_Api_Version_Suffix_Untouched()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "GET",
            "umbraco/delivery/api/v2/content",
            nameof(TestController.DeliveryVersion),
            typeof(QueryContentApiController));

        // The Delivery API's operation IDs are published to headless consumers, so they keep the dot
        // until a major. See the TODO in OperationIdHandler.
        Assert.AreEqual("GetContent2.0", CreateSut().Handle(apiDescription));
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

    [Test]
    public void Can_Use_An_Explicit_Route_Name_Verbatim()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "GET",
            "umbraco/management/api/v1/relation/type/{id}",
            nameof(TestController.DefaultVersion),
            routeName: "GetRelationByRelationTypeId");

        Assert.AreEqual("GetRelationByRelationTypeId", CreateSut().Handle(apiDescription));
    }

    [Test]
    public void Can_Prefix_An_Explicit_Route_Name_That_Omits_The_Http_Method()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "GET",
            "umbraco/management/api/v1/relation/type/{id}",
            nameof(TestController.DefaultVersion),
            routeName: "RelationByRelationTypeId");

        Assert.AreEqual("GetRelationByRelationTypeId", CreateSut().Handle(apiDescription));
    }

    [Test]
    public void Cannot_Handle_An_Action_Descriptor_That_Is_Not_A_Controller_Action()
    {
        var apiDescription = new ApiDescription
        {
            HttpMethod = "GET",
            RelativePath = "umbraco/management/api/v1/thing",
            ActionDescriptor = new ActionDescriptor(),
        };

        Assert.Throws<ArgumentException>(() => CreateSut().Handle(apiDescription));
    }

    [Test]
    public void Cannot_Handle_A_Missing_Relative_Path()
    {
        ApiDescription apiDescription = CreateApiDescription(
            "GET",
            string.Empty,
            nameof(TestController.DefaultVersion));

        Assert.Throws<InvalidOperationException>(() => CreateSut().Handle(apiDescription));
    }

    private sealed class TestController
    {
        [MapToApiVersion("1.1")]
        public void MinorVersion()
        {
        }

        [MapToApiVersion("2.0")]
        public void DeliveryVersion()
        {
        }

        [MapToApiVersion("1.0")]
        public void DefaultVersion()
        {
        }
    }
}
