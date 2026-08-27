using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaLockdownFilterTests
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

    private sealed class LockDocumentTypes : ISchemaLockdownConfigurator
    {
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.BlockMutations(Constants.UdiEntityType.DocumentType);
    }

    [Test]
    public async Task Read_Request_Is_Permitted_Even_While_Locked_Down()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Get, lockDocumentTypes: true);

        await CreateFilter().OnAuthorizationAsync(context);

        AssertPermitted(context);
    }

    [TestCase("POST")]
    [TestCase("PUT")]
    [TestCase("DELETE")]
    public async Task Mutating_Request_Is_Permitted_When_Nothing_Is_Blocked(string httpMethod)
    {
        AuthorizationFilterContext context = CreateContext(httpMethod, lockDocumentTypes: false);

        await CreateFilter().OnAuthorizationAsync(context);

        AssertPermitted(context);
    }

    [TestCase("POST")]
    [TestCase("PUT")]
    [TestCase("DELETE")]
    public async Task Mutating_Request_Is_Forbidden_When_Blocked(string httpMethod)
    {
        AuthorizationFilterContext context = CreateContext(httpMethod, lockDocumentTypes: true);

        await CreateFilter().OnAuthorizationAsync(context);

        AssertForbidden(context);
    }

    // The declared entity type is the only thing distinguishing these two, so a filter ignoring it would let one of
    // them through.
    [Test]
    public async Task Decision_Follows_The_Declared_Entity_Type()
    {
        AuthorizationFilterContext locked = CreateContext(HttpMethods.Post, lockDocumentTypes: true);
        AuthorizationFilterContext unlocked = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter().OnAuthorizationAsync(locked);
        await CreateFilter(Constants.UdiEntityType.Webhook).OnAuthorizationAsync(unlocked);

        Assert.Multiple(() =>
        {
            AssertForbidden(locked);
            AssertPermitted(unlocked);
        });
    }

    [Test]
    public async Task Declared_Read_Operation_Overrides_The_Post_Verb()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            lockDocumentTypes: true,
            actionName: nameof(DocumentTypeController.PostButReadOnly));

        await CreateFilter().OnAuthorizationAsync(context);

        AssertPermitted(context);
    }

    // The header is the only thing telling a developer which of the several reasons a 403 can have applied here.
    [Test]
    public async Task Denial_Names_The_Entity_Type_And_Operation()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Put, lockDocumentTypes: true);

        await CreateFilter().OnAuthorizationAsync(context);

        Assert.That(
            context.HttpContext.Response.Headers[Constants.Headers.SchemaLockdown].ToString(),
            Is.EqualTo($"{Constants.UdiEntityType.DocumentType}:update"));
    }

    [Test]
    public async Task Denial_Names_The_Entity_Type_In_Lower_Case_Whatever_Was_Declared()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter("Document-Type").OnAuthorizationAsync(context);

        Assert.That(
            context.HttpContext.Response.Headers[Constants.Headers.SchemaLockdown].ToString(),
            Is.EqualTo("document-type:create"));
    }

    // Mirrors what MVC hands an authorization filter: the routed action descriptor, and an HttpContext whose
    // RequestServices is where the filter reaches its dependencies.
    private static AuthorizationFilterContext CreateContext(
        string httpMethod,
        bool lockDocumentTypes,
        string actionName = nameof(DocumentTypeController.Post))
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = CreateServices(lockDocumentTypes),
        };
        httpContext.Request.Method = httpMethod;

        var descriptor = new ControllerActionDescriptor
        {
            ControllerTypeInfo = typeof(DocumentTypeController).GetTypeInfo(),
            MethodInfo = typeof(DocumentTypeController).GetMethod(actionName)!,
            ActionName = actionName,
        };

        return new AuthorizationFilterContext(new ActionContext(httpContext, new RouteData(), descriptor), []);
    }

    private static IServiceProvider CreateServices(bool lockDocumentTypes)
    {
        ISchemaLockdownConfigurator[] configurators = lockDocumentTypes ? [new LockDocumentTypes()] : [];
        var restrictions = new SchemaRestrictions(new SchemaLockdownConfiguratorCollection(() => configurators));

        var services = new ServiceCollection();
        services.AddSingleton<ISchemaRestrictions>(restrictions);

        return services.BuildServiceProvider();
    }

    private static SchemaEntityTypeAttribute CreateFilter(
        string entityType = Constants.UdiEntityType.DocumentType)
        => new(entityType);

    private static void AssertPermitted(AuthorizationFilterContext context)
        => Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.Null);
            Assert.That(
                context.HttpContext.Response.Headers.ContainsKey(Constants.Headers.SchemaLockdown),
                Is.False);
        });

    private static void AssertForbidden(AuthorizationFilterContext context)
        => Assert.That(
            (context.Result as StatusCodeResult)?.StatusCode,
            Is.EqualTo(StatusCodes.Status403Forbidden));
}
