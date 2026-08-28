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

    // Mirrors CreateDocumentTypeTemplateController: a POST that creates a template and updates the document type
    // it belongs to, so the two entity types resolve to different operations.
    private class TemplateForDocumentTypeController
    {
        [SchemaOperation(Constants.UdiEntityType.DocumentType, SchemaOperation.Update)]
        public void Post()
        {
        }
    }

    private sealed class LockDocumentTypes : ISchemaLockdownConfigurator
    {
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.BlockMutations(Constants.UdiEntityType.DocumentType);
    }

    private sealed class LockDocumentTypeCreationOnly : ISchemaLockdownConfigurator
    {
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Create);
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

    // An endpoint touching more than one entity type is denied if any of them denies the operation, so the most
    // restrictive answer wins. Creating a template for a document type is the case in the codebase.
    [Test]
    public async Task Every_Declared_Entity_Type_Has_To_Permit_The_Operation()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter(Constants.UdiEntityType.Webhook, Constants.UdiEntityType.DocumentType)
            .OnAuthorizationAsync(context);

        AssertForbidden(context);
    }

    [Test]
    public async Task Denial_Names_The_Entity_Type_That_Denied_It()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter(Constants.UdiEntityType.Webhook, Constants.UdiEntityType.DocumentType)
            .OnAuthorizationAsync(context);

        Assert.That(
            context.HttpContext.Response.Headers[Constants.Headers.SchemaLockdown].ToString(),
            Is.EqualTo($"{Constants.UdiEntityType.DocumentType}:create"));
    }

    [Test]
    public async Task Several_Declared_Entity_Types_Are_Permitted_When_None_Denies()
    {
        AuthorizationFilterContext context = CreateContext(HttpMethods.Post, lockDocumentTypes: true);

        await CreateFilter(Constants.UdiEntityType.Webhook, Constants.UdiEntityType.Language)
            .OnAuthorizationAsync(context);

        AssertPermitted(context);
    }

    // A declaration naming an entity type decides that one's operation, so an endpoint doing different things to
    // each of its types is judged on what it actually does to each.
    [Test]
    public async Task An_Entity_Type_Named_By_A_Declaration_Uses_That_Operation()
    {
        AuthorizationFilterContext context = CreateContext(
            HttpMethods.Post,
            new LockDocumentTypeCreationOnly(),
            typeof(TemplateForDocumentTypeController));

        await CreateFilter(Constants.UdiEntityType.DocumentType, Constants.UdiEntityType.Template)
            .OnAuthorizationAsync(context);

        // The verb says create and document type creation is denied, but the declaration says this updates the
        // document type, and updating it is not.
        AssertPermitted(context);
    }

    [Test]
    public async Task An_Entity_Type_Without_A_Declaration_Falls_Back_To_The_Verb()
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
                Is.EqualTo($"{Constants.UdiEntityType.Template}:create"));
        });
    }

    private sealed class LockTemplateCreation : ISchemaLockdownConfigurator
    {
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Block(Constants.UdiEntityType.Template, SchemaOperation.Create);
    }

    // Mirrors what MVC hands an authorization filter: the routed action descriptor, and an HttpContext whose
    // RequestServices is where the filter reaches its dependencies.
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
