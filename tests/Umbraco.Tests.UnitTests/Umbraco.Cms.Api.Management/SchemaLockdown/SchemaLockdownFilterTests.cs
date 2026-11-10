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

        [SchemaOperation([])]
        public void PostButReadOnly()
        {
        }
    }

    // A POST that creates a template and updates the document type.
    private class TemplateForDocumentTypeController
    {
        [SchemaOperation([Constants.Permissions.Template.Create, Constants.Permissions.DocumentType.Update])]
        public void Post()
        {
        }
    }

    private sealed class LockDocumentTypes : ISchemaLockdownConfigurator
    {
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Deny(Constants.Permissions.DocumentType.Create, Constants.Permissions.DocumentType.Update, Constants.Permissions.DocumentType.Delete);
    }

    private sealed class LockDocumentTypeCreationOnly : ISchemaLockdownConfigurator
    {
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Deny(Constants.Permissions.DocumentType.Create);
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
    public async Task Declaring_No_Permissions_Overrides_The_Post_Verb()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            lockDocumentTypes: true,
            actionName: nameof(DocumentTypeController.PostButReadOnly));

        await CreateFilter().OnAuthorizationAsync(context);

        AssertPermitted(context);
    }

    [Test]
    public async Task Denial_Names_The_Denied_Permission()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Put, lockDocumentTypes: true);

        await CreateFilter().OnAuthorizationAsync(context);

        Assert.That(
            context.HttpContext.Response.Headers[Constants.Headers.SchemaLockdown].ToString(),
            Is.EqualTo(Constants.Permissions.DocumentType.Update));
    }

    [Test]
    public async Task An_Unrecognised_Verb_Is_Denied_As_An_Update()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Trace, lockDocumentTypes: true);

        await CreateFilter().OnAuthorizationAsync(context);

        Assert.Multiple(() =>
        {
            AssertForbidden(context);
            Assert.That(
                context.HttpContext.Response.Headers[Constants.Headers.SchemaLockdown].ToString(),
                Is.EqualTo(Constants.Permissions.DocumentType.Update));
        });
    }

    [Test]
    public async Task Denial_Names_The_Permission_Whatever_Case_The_Entity_Type_Was_Declared_In()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter("Document-Type").OnAuthorizationAsync(context);

        Assert.That(
            context.HttpContext.Response.Headers[Constants.Headers.SchemaLockdown].ToString(),
            Is.EqualTo(Constants.Permissions.DocumentType.Create));
    }

    [Test]
    public async Task Every_Declared_Entity_Type_Has_To_Permit_The_Operation()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter(Constants.UdiEntityType.Webhook, Constants.UdiEntityType.DocumentType)
            .OnAuthorizationAsync(context);

        AssertForbidden(context);
    }

    [Test]
    public async Task Denial_Names_The_Permission_Of_The_Entity_Type_That_Denied_It()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter(Constants.UdiEntityType.Webhook, Constants.UdiEntityType.DocumentType)
            .OnAuthorizationAsync(context);

        Assert.That(
            context.HttpContext.Response.Headers[Constants.Headers.SchemaLockdown].ToString(),
            Is.EqualTo(Constants.Permissions.DocumentType.Create));
    }

    [Test]
    public async Task Several_Declared_Entity_Types_Are_Permitted_When_None_Denies()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter(Constants.UdiEntityType.Webhook, Constants.UdiEntityType.Language)
            .OnAuthorizationAsync(context);

        AssertPermitted(context);
    }

    [Test]
    public async Task An_Entity_Type_Named_By_A_Declaration_Uses_That_Operation()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            new LockDocumentTypeCreationOnly(),
            typeof(TemplateForDocumentTypeController));

        await CreateFilter(Constants.UdiEntityType.DocumentType, Constants.UdiEntityType.Template)
            .OnAuthorizationAsync(context);

        // The verb says create, but the declaration says this updates the document type, which isn't denied.
        AssertPermitted(context);
    }

    [Test]
    public async Task Every_Declared_Permission_Has_To_Be_Permitted()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            new LockTemplateCreation(),
            typeof(TemplateForDocumentTypeController));

        await CreateFilter(Constants.UdiEntityType.DocumentType, Constants.UdiEntityType.Template)
            .OnAuthorizationAsync(context);

        Assert.Multiple(() =>
        {
            AssertForbidden(context);
            Assert.That(
                context.HttpContext.Response.Headers[Constants.Headers.SchemaLockdown].ToString(),
                Is.EqualTo(Constants.Permissions.Template.Create));
        });
    }

    private sealed class LockTemplateCreation : ISchemaLockdownConfigurator
    {
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Deny(Constants.Permissions.Template.Create);
    }

    private static AuthorizationFilterContext CreateContext(
        string httpMethod,
        bool lockDocumentTypes,
        string actionName = nameof(DocumentTypeController.Post))
        => CreateContext(
            httpMethod,
            lockDocumentTypes ? new LockDocumentTypes() : null,
            typeof(DocumentTypeController),
            actionName);

    private static AuthorizationFilterContext CreateContext(
        string httpMethod,
        ISchemaLockdownConfigurator? configurator,
        Type controller,
        string actionName = "Post")
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = CreateServices(configurator),
        };
        httpContext.Request.Method = httpMethod;

        var descriptor = new ControllerActionDescriptor
        {
            ControllerTypeInfo = controller.GetTypeInfo(),
            MethodInfo = controller.GetMethod(actionName)!,
            ActionName = actionName,
        };

        return new AuthorizationFilterContext(new ActionContext(httpContext, new RouteData(), descriptor), []);
    }

    private static IServiceProvider CreateServices(ISchemaLockdownConfigurator? configurator)
    {
        ISchemaLockdownConfigurator[] configurators = configurator is null ? [] : [configurator];
        var restrictions = new SchemaRestrictions(new SchemaLockdownConfiguratorCollection(() => configurators));

        var services = new ServiceCollection();
        services.AddSingleton<ISchemaRestrictions>(restrictions);

        return services.BuildServiceProvider();
    }

    private static SchemaEntityTypeAttribute CreateFilter(params string[] entityTypes)
        => new(entityTypes.Length > 0 ? entityTypes : [Constants.UdiEntityType.DocumentType]);

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
