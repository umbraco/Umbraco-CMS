// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Fixtures;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Boots a single Umbraco host shared by all test fixtures in the
///     <c>Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services</c> namespace.
///     Individual test fixture classes get their own fresh database via <see cref="TestDatabaseSwapper"/>.
/// </summary>
[SetUpFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
public class ServiceTestsSetUpFixture : UmbracoIntegrationFixture
{
    /// <summary>
    ///     The shared fixture instance, accessible by all test fixtures in this namespace.
    /// </summary>
    public static ServiceTestsSetUpFixture Instance { get; private set; }

    /// <summary>
    ///     The shared service provider from the running host.
    /// </summary>
    public IServiceProvider SharedServices => Services;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Instance = this;

        DatabaseSwapper = new TestDatabaseSwapper();

        InMemoryConfiguration["Umbraco:CMS:ModelsBuilder:ModelsMode"] = "Nothing";
        InMemoryConfiguration["Umbraco:CMS:Hosting:Debug"] = "true";

        BuildAndStartHost();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        DatabaseSwapper.DetachCurrentDatabase(Services, Configuration, TestHelper);
        StopAndDisposeHost();
        Instance = null;
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Register the RelateOnCopyNotificationHandler needed by ContentEditingServiceTests.Copy
        builder.AddNotificationAsyncHandler<ContentCopiedNotification, RelateOnCopyNotificationHandler>();

        // Register scaffolded notification handlers needed by ContentBlueprintEditingServiceTests.GetScaffold
        builder.AddNotificationHandler<ContentScaffoldedNotification, ContentBlueprintEditingServiceTests.ContentScaffoldedNotificationHandler>();
        builder.AddNotificationHandler<ContentScaffoldedNotification, BlockListPropertyNotificationHandler>();
        builder.AddNotificationHandler<ContentScaffoldedNotification, BlockGridPropertyNotificationHandler>();
        builder.AddNotificationHandler<ContentScaffoldedNotification, RichTextPropertyNotificationHandler>();

        // Override EF Core context registrations to use transient options.
        // The default registrations use pooled factories / singleton options, which cache the
        // connection string from initial setup. When we swap databases, EF Core must read the
        // CURRENT connection string, not the cached one.
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
}
