using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.SchemaLockdown;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaLockdownAuthorizationHandlerTests
{
    private class DocumentTypeController
    {
        public void Post()
        {
        }

        [SchemaOperation(SchemaOperation.Read)]
        public void PostButReadOnly()
        {
        }
    }

    private static SchemaLockdownAuthorizationHandler CreateHandler(bool enabled, RuntimeLevel runtimeLevel)
    {
        var accessor = new SchemaLockdownMatrixAccessor(
            Options.Create(new SchemaLockdownSettings { Enabled = enabled }),
            new SchemaLockdownConfiguratorCollection(() => []));

        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        return new SchemaLockdownAuthorizationHandler(accessor, runtimeState.Object);
    }

    private static SchemaLockdownEntityTypeRequirement CreateRequirement(
        string entityType = Constants.UdiEntityType.DocumentType)
        => new(entityType);

    // Mirrors what the authorization middleware hands a handler: the HttpContext as the resource, with the routed
    // endpoint reachable through IEndpointFeature and carrying the action descriptor as metadata.
    private static HttpContext CreateHttpContext(string httpMethod, string? actionName = nameof(DocumentTypeController.Post))
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = httpMethod;

        if (actionName is not null)
        {
            httpContext.Features.Set<IEndpointFeature>(new EndpointFeatureStub(CreateEndpoint(actionName)));
        }

        return httpContext;
    }

    private static Endpoint CreateEndpoint(string actionName)
    {
        MethodInfo method = typeof(DocumentTypeController).GetMethod(actionName)!;
        var descriptor = new ControllerActionDescriptor
        {
            ControllerTypeInfo = typeof(DocumentTypeController).GetTypeInfo(),
            MethodInfo = method,
            ActionName = actionName,
        };

        return new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(descriptor), actionName);
    }

    private static AuthorizationHandlerContext CreateContext(
        SchemaLockdownEntityTypeRequirement requirement,
        object? resource)
        => new([requirement], user: null!, resource);

    private static AuthorizationFilterContext CreateFilterContext(HttpContext httpContext)
        => new(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            []);

    [Test]
    public async Task Read_Request_Succeeds_Even_While_Locked_Down()
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(requirement, CreateHttpContext(HttpMethods.Get));

        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    [TestCase("POST")]
    [TestCase("PUT")]
    [TestCase("DELETE")]
    public async Task Mutating_Request_Succeeds_When_The_Cell_Is_Allowed(string httpMethod)
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(requirement, CreateHttpContext(httpMethod));

        await CreateHandler(enabled: false, RuntimeLevel.Run).HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    [TestCase("POST")]
    [TestCase("PUT")]
    [TestCase("DELETE")]
    public async Task Mutating_Request_Fails_When_The_Cell_Is_Blocked(string httpMethod)
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(requirement, CreateHttpContext(httpMethod));

        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.HasSucceeded, Is.False);
            Assert.That(context.HasFailed, Is.True);
        });
    }

    // The requirement's entity type is the only thing distinguishing these two, so a handler ignoring it would let
    // one of them through.
    [Test]
    public async Task Mutating_Request_Is_Decided_By_The_Requirement_Entity_Type()
    {
        SchemaLockdownEntityTypeRequirement locked = CreateRequirement();
        SchemaLockdownEntityTypeRequirement unlocked = CreateRequirement(Constants.UdiEntityType.Webhook);

        AuthorizationHandlerContext lockedContext = CreateContext(locked, CreateHttpContext(HttpMethods.Post));
        AuthorizationHandlerContext unlockedContext = CreateContext(unlocked, CreateHttpContext(HttpMethods.Post));

        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(lockedContext);
        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(unlockedContext);

        Assert.Multiple(() =>
        {
            Assert.That(lockedContext.HasSucceeded, Is.False);
            Assert.That(unlockedContext.HasSucceeded, Is.True);
        });
    }

    [Test]
    public async Task Declared_Read_Operation_Overrides_The_Post_Verb()
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(
            requirement,
            CreateHttpContext(HttpMethods.Post, nameof(DocumentTypeController.PostButReadOnly)));

        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    [Test]
    public async Task Blocked_Request_Fails_When_The_Resource_Is_An_Authorization_Filter_Context()
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(
            requirement,
            CreateFilterContext(CreateHttpContext(HttpMethods.Post)));

        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.HasSucceeded, Is.False);
            Assert.That(context.HasFailed, Is.True);
        });
    }

    [Test]
    public async Task Read_Request_Succeeds_When_The_Resource_Is_An_Authorization_Filter_Context()
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(
            requirement,
            CreateFilterContext(CreateHttpContext(HttpMethods.Get)));

        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    // A resource carrying no request cannot be classified, so it is treated as a mutation.
    [TestCase(null)]
    [TestCase(nameof(DocumentTypeController.Post))]
    public async Task Resource_Without_A_Request_Fails_While_Locked_Down(string? actionName)
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(
            requirement,
            actionName is null ? null : CreateEndpoint(actionName));

        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.HasSucceeded, Is.False);
            Assert.That(context.HasFailed, Is.True);
        });
    }

    // Without a request there is no verb, so the declared operation is all there is to go on.
    [Test]
    public async Task Declared_Read_Operation_Applies_To_A_Resource_Without_A_Request()
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(
            requirement,
            CreateEndpoint(nameof(DocumentTypeController.PostButReadOnly)));

        await CreateHandler(enabled: true, RuntimeLevel.Run).HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    [Test]
    public async Task Blocked_Request_Still_Fails_While_Awaiting_An_Attended_Upgrade()
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(requirement, CreateHttpContext(HttpMethods.Post));

        await CreateHandler(enabled: true, RuntimeLevel.Upgrade).HandleAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.HasSucceeded, Is.False);
            Assert.That(context.HasFailed, Is.True);
        });
    }

    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrading)]
    [TestCase(RuntimeLevel.BootFailed)]
    public async Task Blocked_Request_Succeeds_When_The_Runtime_Is_Not_Serving_Requests_Normally(RuntimeLevel runtimeLevel)
    {
        SchemaLockdownEntityTypeRequirement requirement = CreateRequirement();
        AuthorizationHandlerContext context = CreateContext(requirement, CreateHttpContext(HttpMethods.Post));

        await CreateHandler(enabled: true, runtimeLevel).HandleAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.HasSucceeded, Is.True);
            Assert.That(context.HasFailed, Is.False);
        });
    }

    private sealed class EndpointFeatureStub : IEndpointFeature
    {
        public EndpointFeatureStub(Endpoint endpoint) => Endpoint = endpoint;

        public Endpoint? Endpoint { get; set; }
    }
}
