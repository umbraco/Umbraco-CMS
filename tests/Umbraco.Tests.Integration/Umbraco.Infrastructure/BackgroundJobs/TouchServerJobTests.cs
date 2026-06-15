// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.BackgroundJobs;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class TouchServerJobTests : UmbracoIntegrationTest
{
    private const string ApplicationUrl = "https://example.com/";

    private IServerRegistrationService ServerRegistrationService => GetRequiredService<IServerRegistrationService>();

    [Test]
    public async Task RunJobAsync_With_No_Application_Url_Registers_Server_And_Resolves_Role_To_Single()
    {
        // No application URL is available, so the job has to fall back to a placeholder address.
        TouchServerJob job = CreateJob(ApplicationMainUrl(null));

        // Before the job runs nothing has been registered, so the role cannot be determined.
        Assert.That(ServerRegistrationService.GetCurrentServerRole(), Is.EqualTo(ServerRole.Unknown));

        await job.RunJobAsync(CancellationToken.None);

        // The server is registered despite having no application URL, so election resolves the role to Single.
        Assert.That(ServerRegistrationService.GetCurrentServerRole(), Is.EqualTo(ServerRole.Single));

        IServerRegistration[] activeServers = ServerRegistrationService.GetActiveServers(refresh: true).ToArray();
        Assert.That(activeServers, Has.Length.EqualTo(1));
        Assert.That(activeServers[0].ServerAddress, Is.EqualTo(Environment.MachineName));
    }

    [Test]
    public async Task RunJobAsync_With_Application_Url_Registers_Server_With_That_Url()
    {
        TouchServerJob job = CreateJob(ApplicationMainUrl(new Uri(ApplicationUrl)));

        await job.RunJobAsync(CancellationToken.None);

        Assert.That(ServerRegistrationService.GetCurrentServerRole(), Is.EqualTo(ServerRole.Single));

        IServerRegistration[] activeServers = ServerRegistrationService.GetActiveServers(refresh: true).ToArray();
        Assert.That(activeServers, Has.Length.EqualTo(1));
        Assert.That(activeServers[0].ServerAddress, Is.EqualTo(ApplicationUrl));
    }

    [Test]
    public async Task RunJobAsync_Replaces_Placeholder_With_Real_Url_Once_It_Becomes_Available()
    {
        // The application URL is not known on the first touch (e.g. detection is on but no request has arrived yet),
        // but becomes available later (e.g. detected from a request).
        Uri? applicationMainUrl = null;
        var hostingEnvironment = new Mock<IHostingEnvironment>();
        hostingEnvironment.SetupGet(x => x.ApplicationMainUrl).Returns(() => applicationMainUrl);
        TouchServerJob job = CreateJob(hostingEnvironment.Object);

        await job.RunJobAsync(CancellationToken.None);

        // First touch: registered with the placeholder.
        Assert.That(
            ServerRegistrationService.GetActiveServers(refresh: true).Single().ServerAddress, Is.EqualTo(Environment.MachineName));

        applicationMainUrl = new Uri(ApplicationUrl);
        await job.RunJobAsync(CancellationToken.None);

        // Second touch: the same (single) registration now holds the real URL, and the role is still Single.
        IServerRegistration[] activeServers = ServerRegistrationService.GetActiveServers(refresh: true).ToArray();
        Assert.That(activeServers, Has.Length.EqualTo(1));
        Assert.That(activeServers[0].ServerAddress, Is.EqualTo(ApplicationUrl));
        Assert.That(ServerRegistrationService.GetCurrentServerRole(), Is.EqualTo(ServerRole.Single));
    }

    private static IHostingEnvironment ApplicationMainUrl(Uri? url)
    {
        var hostingEnvironment = new Mock<IHostingEnvironment>();
        hostingEnvironment.SetupGet(x => x.ApplicationMainUrl).Returns(url);
        return hostingEnvironment.Object;
    }

    private TouchServerJob CreateJob(IHostingEnvironment hostingEnvironment) => new(
        ServerRegistrationService,
        hostingEnvironment,
        GetRequiredService<ILogger<TouchServerJob>>(),
        GetRequiredService<IOptionsMonitor<GlobalSettings>>(),
        new ElectedServerRoleAccessor(ServerRegistrationService));
}
