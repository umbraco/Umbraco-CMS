// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.TestServerTest;

/// <summary>
/// HTTP integration tests to verify that Umbraco can boot with different configurations.
/// These tests create a full web application and verify it starts successfully.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify the supported Umbraco configuration scenarios:
/// </para>
/// <list type="bullet">
/// <item><description>Full: AddBackOffice() + AddWebsite() + AddDeliveryApi()</description></item>
/// <item><description>Delivery-only: AddCore() + AddWebsite() + AddDeliveryApi() (no backoffice)</description></item>
/// <item><description>Core + Website: AddCore() + AddWebsite()</description></item>
/// <item><description>Core + Delivery: AddCore() + AddDeliveryApi()</description></item>
/// </list>
/// <para>
/// Note: AddBackOffice() without AddWebsite() is not a supported scenario because
/// the Management API depends on services registered by AddWebsite().
/// </para>
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
public class CoreConfigurationHttpTests : UmbracoIntegrationTestBase
{
    /// <summary>
    /// Gets the content root directory for the test project.
    /// Walks up the directory tree from the assembly location until we leave the bin/obj folders.
    /// </summary>
    private static string GetTestContentRoot()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var directory = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation)
            ?? throw new InvalidOperationException("Could not determine assembly directory."));

        // Walk up parent directories until we're no longer in a bin or obj folder
        while (directory.Parent is not null)
        {
            var name = directory.Name;
            if (name.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("obj", StringComparison.OrdinalIgnoreCase))
            {
                // Found bin/obj folder, return its parent (the project directory)
                return directory.Parent.FullName;
            }

            directory = directory.Parent;
        }

        // No bin/obj folder found, return the original directory
        return Path.GetDirectoryName(assemblyLocation)
            ?? throw new InvalidOperationException("Could not determine content root directory.");
    }

    private WebApplicationFactory<CoreConfigurationHttpTests> CreateFactory(
        Action<IUmbracoBuilder> configureUmbraco,
        Action<IApplicationBuilder> configureApp)
    {
        var contentRoot = GetTestContentRoot();

        return new UmbracoWebApplicationFactory<CoreConfigurationHttpTests>(() => CreateHostBuilder(configureUmbraco, configureApp))
            .WithWebHostBuilder(builder =>
            {
                builder.UseContentRoot(contentRoot);
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IWebProfilerRepository, TestWebProfilerRepository>();
                });
            });
    }

    private IHostBuilder CreateHostBuilder(
        Action<IUmbracoBuilder> configureUmbraco,
        Action<IApplicationBuilder> configureApp)
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureUmbracoDefaults()
            .ConfigureAppConfiguration((context, configBuilder) =>
            {
                context.HostingEnvironment = TestHelper.GetWebHostEnvironment();
                configBuilder.Sources.Clear();
                configBuilder.AddInMemoryCollection(InMemoryConfiguration);
                configBuilder.AddConfiguration(GlobalSetupTeardown.TestConfiguration);

                Configuration = configBuilder.Build();
            })
            .ConfigureWebHost(builder =>
            {
                builder.ConfigureServices((context, services) =>
                {
                    context.HostingEnvironment = TestHelper.GetWebHostEnvironment();
                    ConfigureServices(services, configureUmbraco);
                    services.AddUnique(CreateLoggerFactory());
                });

                builder.Configure(app => configureApp(app));
            })
            .UseDefaultServiceProvider(cfg =>
            {
                cfg.ValidateOnBuild = true;
                cfg.ValidateScopes = true;
            });

        return hostBuilder;
    }

    private void ConfigureServices(IServiceCollection services, Action<IUmbracoBuilder> configureUmbraco)
    {
        services.AddTransient<TestUmbracoDatabaseFactoryProvider>();

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            Configuration);

        services.AddLogger(TestHelper.GetWebHostEnvironment(), Configuration);

        var builder = new UmbracoBuilder(
            services,
            Configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        builder.Services.AddTransient<IHostedService>(sp =>
            new TestDatabaseHostedLifecycleService(() => UseTestDatabase(sp)));

        // Let the test configure Umbraco
        configureUmbraco(builder);

        builder.Build();
    }

    /// <summary>
    /// Verifies that full Umbraco (backoffice + website + delivery API) boots successfully.
    /// </summary>
    [Test]
    public async Task FullConfiguration_BootsSuccessfully()
    {
        // Arrange
        using var factory = CreateFactory(
            configureUmbraco: builder =>
            {
                builder
                    .AddBackOffice()
                    .AddWebsite()
                    .AddDeliveryApi()
                    .AddUmbracoSqlServerSupport()
                    .AddUmbracoSqliteSupport()
                    .AddComposers();
            },
            configureApp: app =>
            {
                app.UseUmbraco()
                    .WithMiddleware(u =>
                    {
                        u.UseBackOffice();
                        u.UseWebsite();
                    })
                    .WithEndpoints(u =>
                    {
                        u.UseBackOfficeEndpoints();
                        u.UseWebsiteEndpoints();
                    });
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost/", UriKind.Absolute),
        });

        // Act - Just verify that the app responds (any response means it booted)
        var response = await client.GetAsync("/");

        // Assert - Any HTTP response means the app started successfully
        // We don't care about the status code (could be 404, 200, etc.)
        Assert.That(response, Is.Not.Null, "Application should respond to requests");
        TestContext.WriteLine($"Full configuration: Received HTTP {(int)response.StatusCode} {response.StatusCode}");
    }

    /// <summary>
    /// Verifies that core + website (no backoffice) boots successfully.
    /// </summary>
    [Test]
    public async Task CoreWithWebsite_BootsSuccessfully()
    {
        // Arrange
        using var factory = CreateFactory(
            configureUmbraco: builder =>
            {
                builder
                    .AddCore()
                    .AddWebsite()
                    .AddUmbracoSqlServerSupport()
                    .AddUmbracoSqliteSupport()
                    .AddComposers();
            },
            configureApp: app =>
            {
                app.UseUmbraco()
                    .WithMiddleware(u =>
                    {
                        u.UseWebsite();
                    })
                    .WithEndpoints(u =>
                    {
                        u.UseWebsiteEndpoints();
                    });
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost/", UriKind.Absolute),
        });

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.That(response, Is.Not.Null, "Application should respond to requests");
        TestContext.WriteLine($"Core + Website configuration: Received HTTP {(int)response.StatusCode} {response.StatusCode}");

        // Verify backoffice marker is NOT registered
        var backofficeMarker = factory.Services.GetService<IBackOfficeEnabledMarker>();
        Assert.That(backofficeMarker, Is.Null, "IBackOfficeEnabledMarker should NOT be registered when using AddCore() without AddBackOffice()");
    }

    /// <summary>
    /// Verifies that core + delivery API (no backoffice, no website) boots successfully.
    /// </summary>
    [Test]
    public async Task CoreWithDeliveryApi_BootsSuccessfully()
    {
        // Arrange
        using var factory = CreateFactory(
            configureUmbraco: builder =>
            {
                builder
                    .AddCore()
                    .AddDeliveryApi()
                    .AddUmbracoSqlServerSupport()
                    .AddUmbracoSqliteSupport()
                    .AddComposers();
            },
            configureApp: app =>
            {
                app.UseUmbraco()
                    .WithMiddleware(u =>
                    {
                        // Delivery API doesn't need special middleware
                    })
                    .WithEndpoints(u =>
                    {
                        u.UseDeliveryApiEndpoints();
                    });
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost/", UriKind.Absolute),
        });

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.That(response, Is.Not.Null, "Application should respond to requests");
        TestContext.WriteLine($"Core + Delivery API configuration: Received HTTP {(int)response.StatusCode} {response.StatusCode}");

        // Verify backoffice marker is NOT registered
        var backofficeMarker = factory.Services.GetService<IBackOfficeEnabledMarker>();
        Assert.That(backofficeMarker, Is.Null, "IBackOfficeEnabledMarker should NOT be registered when using AddCore() without AddBackOffice()");
    }

    /// <summary>
    /// Verifies that the delivery-only scenario (core + website + delivery API, no backoffice) boots successfully.
    /// </summary>
    [Test]
    public async Task DeliveryOnlyScenario_BootsSuccessfully()
    {
        // Arrange
        using var factory = CreateFactory(
            configureUmbraco: builder =>
            {
                builder
                    .AddCore()
                    .AddWebsite()
                    .AddDeliveryApi()
                    .AddUmbracoSqlServerSupport()
                    .AddUmbracoSqliteSupport()
                    .AddComposers();
            },
            configureApp: app =>
            {
                app.UseUmbraco()
                    .WithMiddleware(u =>
                    {
                        u.UseWebsite();
                    })
                    .WithEndpoints(u =>
                    {
                        u.UseWebsiteEndpoints();
                        u.UseDeliveryApiEndpoints();
                    });
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost/", UriKind.Absolute),
        });

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.That(response, Is.Not.Null, "Application should respond to requests");
        TestContext.WriteLine($"Delivery-only scenario: Received HTTP {(int)response.StatusCode} {response.StatusCode}");

        // Verify backoffice marker is NOT registered
        var backofficeMarker = factory.Services.GetService<IBackOfficeEnabledMarker>();
        Assert.That(backofficeMarker, Is.Null, "IBackOfficeEnabledMarker should NOT be registered in delivery-only scenario");
    }

    /// <summary>
    /// Verifies that full backoffice configuration (with website) boots successfully.
    /// </summary>
    [Test]
    public async Task BackOfficeWithWebsite_BootsSuccessfully()
    {
        // Arrange
        using var factory = CreateFactory(
            configureUmbraco: builder =>
            {
                builder
                    .AddBackOffice()
                    .AddWebsite()
                    .AddUmbracoSqlServerSupport()
                    .AddUmbracoSqliteSupport()
                    .AddComposers();
            },
            configureApp: app =>
            {
                app.UseUmbraco()
                    .WithMiddleware(u =>
                    {
                        u.UseBackOffice();
                        u.UseWebsite();
                    })
                    .WithEndpoints(u =>
                    {
                        u.UseBackOfficeEndpoints();
                        u.UseWebsiteEndpoints();
                    });
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost/", UriKind.Absolute),
        });

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.That(response, Is.Not.Null, "Application should respond to requests");
        TestContext.WriteLine($"Backoffice + Website configuration: Received HTTP {(int)response.StatusCode} {response.StatusCode}");

        // Verify backoffice marker IS registered
        var backofficeMarker = factory.Services.GetService<IBackOfficeEnabledMarker>();
        Assert.That(backofficeMarker, Is.Not.Null, "IBackOfficeEnabledMarker should be registered when AddBackOffice() is called");
    }
}
