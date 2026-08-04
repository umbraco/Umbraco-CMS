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

    private static IEnumerable<TestCaseData> OperationIdCases()
    {
        // A minor version would otherwise leak a dot into the ID. Substituted rather than removed, so
        // it cannot collide with a genuine "11" version.
        yield return new TestCaseData(
            new OperationIdCase
            {
                HttpMethod = "PUT",
                RelativePath = "umbraco/management/api/v1.1/document/{id}/validate",
                MethodName = nameof(TestController.MinorVersion),
            },
            "PutDocumentByIdValidate1_1").SetName("Substitutes the dot in a minor API version");

        // The Delivery API's operation IDs are published to headless consumers, so they keep the dot
        // until a major. See the TODO in OperationIdHandler.
        yield return new TestCaseData(
            new OperationIdCase
            {
                HttpMethod = "GET",
                RelativePath = "umbraco/delivery/api/v2/content",
                MethodName = nameof(TestController.DeliveryVersion),
                ControllerType = typeof(QueryContentApiController),
            },
            "GetContent2.0").SetName("Leaves the Delivery API version suffix untouched");

        yield return new TestCaseData(
            new OperationIdCase
            {
                HttpMethod = "GET",
                RelativePath = "umbraco/management/api/v10/thing",
            },
            "GetThing").SetName("Strips the route prefix of a multi-digit API version");

        yield return new TestCaseData(
            new OperationIdCase
            {
                HttpMethod = "PUT",
                RelativePath = "umbraco/management/api/v1/media/{id}/validate",
            },
            "PutMediaByIdValidate").SetName("Omits the version suffix for the default API version");

        // An explicit route name wins outright when it already starts with the HTTP method...
        yield return new TestCaseData(
            new OperationIdCase
            {
                HttpMethod = "GET",
                RelativePath = "umbraco/management/api/v1/relation/type/{id}",
                RouteName = "GetRelationByRelationTypeId",
            },
            "GetRelationByRelationTypeId").SetName("Uses an explicit route name verbatim");

        // ...and is prefixed with it when it does not.
        yield return new TestCaseData(
            new OperationIdCase
            {
                HttpMethod = "GET",
                RelativePath = "umbraco/management/api/v1/relation/type/{id}",
                RouteName = "RelationByRelationTypeId",
            },
            "GetRelationByRelationTypeId").SetName("Prefixes an explicit route name that omits the HTTP method");
    }

    [TestCaseSource(nameof(OperationIdCases))]
    public void Can_Generate_Operation_Id(OperationIdCase testCase, string expected)
        => Assert.AreEqual(expected, CreateSut().Handle(CreateApiDescription(testCase)));

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
        ApiDescription apiDescription =
            CreateApiDescription(new OperationIdCase { HttpMethod = "GET", RelativePath = string.Empty });

        Assert.Throws<InvalidOperationException>(() => CreateSut().Handle(apiDescription));
    }

    private static ApiDescription CreateApiDescription(OperationIdCase testCase)
        => new()
        {
            HttpMethod = testCase.HttpMethod,
            RelativePath = testCase.RelativePath,
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = testCase.ControllerType.GetTypeInfo(),
                MethodInfo = typeof(TestController).GetMethod(testCase.MethodName)!,
                AttributeRouteInfo = testCase.RouteName is null
                    ? null
                    : new AttributeRouteInfo { Name = testCase.RouteName },
            },
        };

    /// <summary>
    ///     The controller type only matters for its namespace, which decides whether the version suffix
    ///     is sanitised (Management API) or left alone (Delivery API). Real controllers are used so the
    ///     tests track the namespaces that actually ship.
    /// </summary>
    internal sealed class OperationIdCase
    {
        public string HttpMethod { get; init; } = "GET";

        public string RelativePath { get; init; } = string.Empty;

        public string MethodName { get; init; } = nameof(TestController.DefaultVersion);

        public Type ControllerType { get; init; } = typeof(ValidateUpdateDocumentController);

        public string? RouteName { get; init; }
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
