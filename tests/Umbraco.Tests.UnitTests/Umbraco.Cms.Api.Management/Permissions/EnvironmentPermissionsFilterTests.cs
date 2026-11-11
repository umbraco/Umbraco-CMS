using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Permissions;

[TestFixture]
public class EnvironmentPermissionsFilterTests
{
    private class DocumentTypeController
    {
        public void Post()
        {
        }

        [RequiresPermissions([])]
        public void PostButReadOnly()
        {
        }
    }

    // A POST that creates a template and updates the document type.
    private class TemplateForDocumentTypeController
    {
        [RequiresPermissions([Constants.Permissions.Template.Create, Constants.Permissions.DocumentType.Update])]
        public void Post()
        {
        }
    }

    private sealed class LockDocumentTypes : IEnvironmentPermissionsConfigurator
    {
        public void Configure(IEnvironmentPermissionsBuilder builder)
            => builder.Deny(Constants.Permissions.DocumentType.Create, Constants.Permissions.DocumentType.Update, Constants.Permissions.DocumentType.Delete);
    }

    private sealed class LockDocumentTypeCreationOnly : IEnvironmentPermissionsConfigurator
    {
        public void Configure(IEnvironmentPermissionsBuilder builder)
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
    public async Task Decision_Follows_The_Declared_Prefix()
    {
        AuthorizationFilterContext locked = CreateContext(HttpMethods.Post, lockDocumentTypes: true);
        AuthorizationFilterContext unlocked = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter().OnAuthorizationAsync(locked);
        await CreateFilter(Constants.Permissions.Webhook.Prefix).OnAuthorizationAsync(unlocked);

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

        await RunFiltersAsync(CreateFilter(), context);

        AssertPermitted(context);
    }

    [Test]
    public async Task Denial_Names_The_Denied_Permission()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Put, lockDocumentTypes: true);

        await CreateFilter().OnAuthorizationAsync(context);

        Assert.That(
            context.HttpContext.Response.Headers[Constants.Headers.DeniedPermission].ToString(),
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
                context.HttpContext.Response.Headers[Constants.Headers.DeniedPermission].ToString(),
                Is.EqualTo(Constants.Permissions.DocumentType.Update));
        });
    }

    [Test]
    public async Task An_Entity_Type_Named_By_A_Declaration_Uses_That_Operation()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            new LockDocumentTypeCreationOnly(),
            typeof(TemplateForDocumentTypeController));

        await RunFiltersAsync(CreateFilter(), context);

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

        await RunFiltersAsync(CreateFilter(), context);

        Assert.Multiple(() =>
        {
            AssertForbidden(context);
            Assert.That(
                context.HttpContext.Response.Headers[Constants.Headers.DeniedPermission].ToString(),
                Is.EqualTo(Constants.Permissions.Template.Create));
        });
    }

    [Test]
    public async Task Requiring_Permissions_For_The_Entity_Type_Skips_An_Action_That_Declares_Its_Permissions()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            new LockTemplateCreation(),
            typeof(TemplateForDocumentTypeController));

        await CreateFilter(Constants.Permissions.Template.Prefix).OnAuthorizationAsync(context);

        AssertPermitted(context);
    }

    [Test]
    public async Task Declared_Permissions_Are_Enforced_On_Their_Own()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            new LockTemplateCreation(),
            typeof(TemplateForDocumentTypeController));

        await DeclaredFilter(context).OnAuthorizationAsync(context);

        Assert.Multiple(() =>
        {
            AssertForbidden(context);
            Assert.That(
                context.HttpContext.Response.Headers[Constants.Headers.DeniedPermission].ToString(),
                Is.EqualTo(Constants.Permissions.Template.Create));
        });
    }

    [Test]
    public async Task Declared_Permissions_Are_Permitted_When_None_Is_Denied()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            new LockDocumentTypeCreationOnly(),
            typeof(TemplateForDocumentTypeController));

        await DeclaredFilter(context).OnAuthorizationAsync(context);

        AssertPermitted(context);
    }

    private sealed class LockTemplateCreation : IEnvironmentPermissionsConfigurator
    {
        public void Configure(IEnvironmentPermissionsBuilder builder)
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
        IEnvironmentPermissionsConfigurator? configurator,
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

    private static ServiceProvider CreateServices(IEnvironmentPermissionsConfigurator? configurator)
    {
        IEnvironmentPermissionsConfigurator[] configurators = configurator is null ? [] : [configurator];
        var environmentPermissions = new EnvironmentPermissions(new EnvironmentPermissionsConfiguratorCollection(() => configurators));

        var services = new ServiceCollection();
        services.AddSingleton<IEnvironmentPermissions>(environmentPermissions);

        return services.BuildServiceProvider();
    }

    private static RequiresPermissionsForAttribute CreateFilter(string permissionPrefix = Constants.Permissions.DocumentType.Prefix)
        => new(permissionPrefix);

    private static RequiresPermissionsAttribute DeclaredFilter(AuthorizationFilterContext context)
        => ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo
            .GetCustomAttribute<RequiresPermissionsAttribute>()!;

    // Runs the controller's filter, then the action's declaration if it has one, as MVC does.
    private static async Task RunFiltersAsync(RequiresPermissionsForAttribute filter, AuthorizationFilterContext context)
    {
        await filter.OnAuthorizationAsync(context);

        RequiresPermissionsAttribute? declared = ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo
            .GetCustomAttribute<RequiresPermissionsAttribute>();
        if (context.Result is null && declared is not null)
        {
            await declared.OnAuthorizationAsync(context);
        }
    }

    private static void AssertPermitted(AuthorizationFilterContext context)
        => Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.Null);
            Assert.That(
                context.HttpContext.Response.Headers.ContainsKey(Constants.Headers.DeniedPermission),
                Is.False);
        });

    private static void AssertForbidden(AuthorizationFilterContext context)
        => Assert.That(
            (context.Result as StatusCodeResult)?.StatusCode,
            Is.EqualTo(StatusCodes.Status403Forbidden));
}
