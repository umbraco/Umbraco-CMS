using System.Linq.Expressions;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Tests.Integration.Testing;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.TestServerTest.Fixtures;
using Umbraco.Cms.Tests.Integration.Testing.Fixtures;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

/// <summary>
///     Boots a single Umbraco web application shared by all test fixtures in the
///     <c>Umbraco.Cms.Tests.Integration.ManagementApi</c> namespace and its sub-namespaces.
///     Individual test fixture classes get their own fresh database via <see cref="TestDatabaseSwapper"/>.
/// </summary>
[SetUpFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
public class ManagementApiSetUpFixture : UmbracoTestServerFixture
{
    /// <summary>
    ///     The shared fixture instance, accessible by all test fixtures in this namespace.
    /// </summary>
    public static ManagementApiSetUpFixture Instance { get; private set; }

    /// <summary>
    ///     The shared HttpClient, reused across all fixture classes.
    ///     Each fixture class clears auth headers in its own [SetUp].
    /// </summary>
    public HttpClient SharedClient { get; private set; }

    /// <summary>
    ///     The shared service provider from the running host.
    /// </summary>
    public IServiceProvider SharedServices => Services;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Instance = this;

        // Initialize the database swapper — this changes the host startup to use the swapper
        // for initial DB setup instead of the normal UseTestDatabase flow
        DatabaseSwapper = new TestDatabaseSwapper();

        InMemoryConfiguration["Umbraco:CMS:ModelsBuilder:ModelsMode"] = "Nothing";
        InMemoryConfiguration["Umbraco:CMS:Hosting:Debug"] = "true";

        BuildAndStartWebApplication();

        // Create a shared client (real auth, no test auth handler)
        SharedClient = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost/", UriKind.Absolute),
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        SharedClient?.Dispose();
        DetachDatabase();
        DisposeClientAndFactory();
        await CleanupFactories();
        Instance = null;
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Override EF Core context registrations to use transient options.
        // The default registrations use pooled factories / singleton options, which cache the
        // connection string from initial setup. When we swap databases, EF Core must read the
        // CURRENT connection string, not the cached one.

        // Replace UmbracoDbContext: remove the pooled factory, re-register with transient options
        builder.Services.RemoveAll<IDbContextFactory<UmbracoDbContext>>();
        builder.Services.RemoveAll<DbContextOptions<UmbracoDbContext>>();
        builder.Services.RemoveAll<UmbracoDbContext>();

        builder.Services.AddDbContext<UmbracoDbContext>(
            (sp, options) =>
            {
                var connStrings = sp.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;
                if (!string.IsNullOrEmpty(connStrings.ConnectionString) && !string.IsNullOrEmpty(connStrings.ProviderName))
                {
                    options.UseDatabaseProvider(connStrings.ProviderName, connStrings.ConnectionString);
                }

                options.UseOpenIddict();
            },
            optionsLifetime: ServiceLifetime.Transient);

        builder.Services.AddDbContextFactory<UmbracoDbContext>(
            (sp, options) =>
            {
                var connStrings = sp.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;
                if (!string.IsNullOrEmpty(connStrings.ConnectionString) && !string.IsNullOrEmpty(connStrings.ProviderName))
                {
                    options.UseDatabaseProvider(connStrings.ProviderName, connStrings.ConnectionString);
                }

                options.UseOpenIddict();
            });

        // Replace TestUmbracoDbContext: change from Singleton to Transient options
        builder.Services.RemoveAll<DbContextOptions<TestUmbracoDbContext>>();

        builder.Services.AddDbContext<TestUmbracoDbContext>(
            (sp, options) =>
            {
                var connStrings = sp.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;
                var testDatabaseType = builder.Config.GetValue<TestDatabaseSettings.TestDatabaseType>("Tests:Database:DatabaseType");
                if (testDatabaseType is TestDatabaseSettings.TestDatabaseType.Sqlite)
                {
                    options.UseSqlite(connStrings.ConnectionString);
                }
                else
                {
                    options.UseSqlServer(connStrings.ConnectionString);
                }
            },
            optionsLifetime: ServiceLifetime.Transient);
    }

    protected override void Configure(IApplicationBuilder app)
    {
        // Capture unhandled exceptions and include details in the response body for debugging
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var fullException = ex.ToString();
                Console.WriteLine($"[ManagementApiSetUpFixture] Unhandled exception for {context.Request.Path}: {fullException}");
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync(fullException);
                }
            }
        });
        base.Configure(app);
    }

    protected override void CustomTestAuthSetup(IServiceCollection services)
    {
        // We do not want fake auth — use real OAuth PKCE flow
    }

    /// <summary>
    ///     Resolves a ManagementApi URL for the given controller method.
    ///     Used by <see cref="ManagementApiTest{T}"/> to build URLs without needing direct LinkGenerator access.
    /// </summary>
    public string GetManagementApiUrl<T>(Expression<Func<T, object>> methodSelector)
        where T : ManagementApiControllerBase
    {
        MethodInfo? method = ExpressionHelper.GetMethodInfo(methodSelector);
        IDictionary<string, object?> methodParams = ExpressionHelper.GetMethodParams(methodSelector) ?? new Dictionary<string, object?>();

        // Remove the CancellationToken from the method params
        methodParams.Remove(methodParams.FirstOrDefault(x => x.Value is CancellationToken).Key);
        methodParams["version"] = method?.GetCustomAttribute<MapToApiVersionAttribute>()?.Versions[0].MajorVersion.ToString();
        return LinkGenerator.GetUmbracoControllerUrl(method.Name, ControllerExtensions.GetControllerName<T>(), null, methodParams);
    }
}
