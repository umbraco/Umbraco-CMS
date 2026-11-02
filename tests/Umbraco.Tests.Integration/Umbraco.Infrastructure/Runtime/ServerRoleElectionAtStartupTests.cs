// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Runtime;

/// <summary>
/// Verifies that CoreRuntime resolves the server role before publishing UmbracoApplicationStartingNotification,
/// closing the startup timing gap where IServerRoleAccessor.CurrentServerRole was structurally guaranteed to
/// still be ServerRole.Unknown at that point (TouchServerJob, the only other thing that advances it, does not
/// start its countdown until hosted services run - which is after this notification has already been awaited
/// to completion).
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Boot = true)]
internal sealed class ServerRoleElectionAtStartupTests : UmbracoIntegrationTest
{
    // Populated by ServerRoleCapture during the [SetUp] boot - see CustomTestSetup - so it already holds its
    // value by the time each [Test] method body runs.
    private readonly List<ServerRole> _capturedRoles = [];

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.Services.AddSingleton(_capturedRoles);
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, ServerRoleCapture>();
    }

    [Test]
    public void CurrentServerRole_IsResolved_BeforeUmbracoApplicationStartingNotification_Handlers_Run()
    {
        Assert.That(_capturedRoles, Has.Count.EqualTo(1), "The probe handler must have run exactly once during boot.");

        // Assert the concrete resolved value (not just "not Unknown") so a regression to the wrong role is
        // also caught: a single, freshly-schema'd test database with no other registered servers is Single.
        Assert.That(
            _capturedRoles[0],
            Is.EqualTo(ServerRole.Single),
            "The server role must already be resolved by the time an UmbracoApplicationStartingNotification handler runs.");

        // Cross-check against the live accessor: the resolved role still holds once boot has fully completed.
        var serverRoleAccessor = GetRequiredService<IServerRoleAccessor>();
        Assert.That(serverRoleAccessor.CurrentServerRole, Is.EqualTo(ServerRole.Single));
    }

    private sealed class ServerRoleCapture(List<ServerRole> capturedRoles, IServerRoleAccessor serverRoleAccessor)
        : INotificationHandler<UmbracoApplicationStartingNotification>
    {
        public void Handle(UmbracoApplicationStartingNotification notification) =>
            capturedRoles.Add(serverRoleAccessor.CurrentServerRole);
    }
}
