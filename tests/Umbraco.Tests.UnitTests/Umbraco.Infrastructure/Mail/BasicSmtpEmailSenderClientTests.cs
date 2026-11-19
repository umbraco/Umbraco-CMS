using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Mail;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Mail;

[TestFixture]
public class BasicSmtpEmailSenderClientTests
{
    [Test]
    public void VerifyConnectionAsync_Uses_Settings_Current_At_Call_Time()
    {
        var currentSettings = new GlobalSettings();
        var optionsMonitorMock = new Mock<IOptionsMonitor<GlobalSettings>>();
        optionsMonitorMock.SetupGet(x => x.CurrentValue).Returns(() => currentSettings);
        var client = new BasicSmtpEmailSenderClient(optionsMonitorMock.Object);

        currentSettings = new GlobalSettings
        {
            Smtp = new SmtpSettings { From = "from@test.com", Host = IPAddress.Loopback.ToString(), Port = GetClosedPort() },
        };

        Assert.CatchAsync<SocketException>(() => client.VerifyConnectionAsync());
    }

    private static int GetClosedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
