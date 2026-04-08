// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Testing.SeedProfiles;

/// <summary>
///     Seed profile that performs the Umbraco unattended install and registers the
///     OpenIddict backoffice application. This is the same work that
///     <see cref="Fixtures.TestDatabaseSwapper.SwapDatabaseAsync"/> does after attaching a
///     schema database.
///     <para>
///         Using this as a <see cref="DatabaseSeedProfileAttribute"/> allows fixtures to
///         restore a snapshot of the installed state instead of running the full install
///         every time.
///     </para>
/// </summary>
public class InstalledSchemaSeed : ITestDatabaseSeedProfile
{
    /// <inheritdoc />
    public string SeedKey => "__installed_schema__";

    /// <inheritdoc />
    public async Task SeedAsync(IServiceProvider services)
    {
        // Determine runtime level so the system knows we're in "Install" state
        var state = services.GetRequiredService<IRuntimeState>();
        state.DetermineRuntimeLevel();

        // Run the unattended install — creates default user, data types, etc.
        services.GetRequiredService<IEventAggregator>()
            .Publish(new UnattendedInstallNotification());

        // Register the OpenIddict backoffice application in the database.
        // Normally this happens via BackOfficeAuthorizationInitializationMiddleware on first request,
        // but since we're seeding before any request, we do it explicitly.
        using var scope = services.CreateScope();
        var backOfficeAppManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync([new Uri("https://localhost/")]);
    }
}
