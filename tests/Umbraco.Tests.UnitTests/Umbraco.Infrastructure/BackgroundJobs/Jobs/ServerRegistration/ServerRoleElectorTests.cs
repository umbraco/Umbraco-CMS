// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

[TestFixture]
public class ServerRoleElectorTests
{
    [Test]
    public async Task TryElectOnceAsync_WhenElectedServerRoleAccessor_TouchesServer()
    {
        var registrationService = new Mock<IServerRegistrationService>();
        ServerRoleElector sut = CreateSut(registrationService.Object, useElection: true);

        await sut.TryElectOnceAsync(CancellationToken.None);

        registrationService.Verify(x => x.TouchServer(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    /// <summary>
    /// A custom (non-ElectedServerRoleAccessor) role accessor - e.g. supplied by a host via SetServerRegistrar{T}(),
    /// or election disabled for a single server - means there is nothing here for this server to elect, so the
    /// attempt must no-op rather than forcing an unwanted database write.
    /// </summary>
    [Test]
    public async Task TryElectOnceAsync_WhenNotElectedServerRoleAccessor_DoesNotTouchServer()
    {
        var registrationService = new Mock<IServerRegistrationService>();
        ServerRoleElector sut = CreateSut(registrationService.Object, useElection: false);

        await sut.TryElectOnceAsync(CancellationToken.None);

        registrationService.Verify(x => x.TouchServer(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    /// <summary>
    /// A failure while attempting the election (e.g. a genuinely read-only database) must never propagate -
    /// callers publish UmbracoApplicationStartingNotification immediately afterwards and must not have boot
    /// fail over this.
    /// </summary>
    [Test]
    public void TryElectOnceAsync_WhenTouchServerThrows_DoesNotPropagate()
    {
        var registrationService = new Mock<IServerRegistrationService>();
        registrationService
            .Setup(x => x.TouchServer(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Throws(new InvalidOperationException("database is read-only"));
        ServerRoleElector sut = CreateSut(registrationService.Object, useElection: true);

        Assert.DoesNotThrowAsync(() => sut.TryElectOnceAsync(CancellationToken.None));
    }

    private static ServerRoleElector CreateSut(IServerRegistrationService registrationService, bool useElection)
    {
        IServerRoleAccessor serverRoleAccessor = useElection
            ? new ElectedServerRoleAccessor(registrationService)
            : new SingleServerRoleAccessor();

        return new ServerRoleElector(
            serverRoleAccessor,
            registrationService,
            Mock.Of<IHostingEnvironment>(),
            new TestOptionsMonitor<GlobalSettings>(new GlobalSettings()),
            NullLogger<ServerRoleElector>.Instance);
    }
}
