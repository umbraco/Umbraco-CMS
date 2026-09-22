using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Handlers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Handlers;

[TestFixture]
public class InitializeMemberApplicationNotificationHandlerTests
{
    /// <summary>
    /// The handler's initialization guard is a static field shared across every instance for the lifetime of
    /// the process, so it must be reset between tests to keep them isolated from each other.
    /// </summary>
    [SetUp]
    public void ResetStaticInitializationState()
    {
        typeof(InitializeMemberApplicationNotificationHandler)
            .GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, false);
    }

    /// <summary>
    /// A server that cannot write to the OpenIddict store (e.g. a Subscriber configured against a read-only
    /// database connection) must never be able to fail the whole boot sequence over it — the awaited
    /// <c>UmbracoApplicationStartingNotification</c> publish must complete regardless.
    /// </summary>
    [Test]
    public void HandleAsync_WhenEnsureMemberApplicationThrows_DoesNotPropagate()
    {
        var memberApplicationManager = new Mock<IMemberApplicationManager>();
        memberApplicationManager
            .Setup(x => x.EnsureMemberApplicationAsync(It.IsAny<IEnumerable<Uri>>(), It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("cannot write to read-only database"));

        var sut = CreateSut(memberApplicationManager.Object, out _);

        Assert.DoesNotThrowAsync(() => sut.HandleAsync(
            new UmbracoApplicationStartingNotification(RuntimeLevel.Run, false),
            CancellationToken.None));
    }

    /// <summary>
    /// A failed initialization attempt must not be marked as done — a later restart (once the underlying issue
    /// is resolved) needs another chance to run <c>EnsureMemberApplicationAsync</c>.
    /// </summary>
    [Test]
    public async Task HandleAsync_WhenEnsureMemberApplicationThrows_DoesNotMarkAsInitialized()
    {
        var memberApplicationManager = new Mock<IMemberApplicationManager>();
        memberApplicationManager
            .Setup(x => x.EnsureMemberApplicationAsync(It.IsAny<IEnumerable<Uri>>(), It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("cannot write to read-only database"));

        var sut = CreateSut(memberApplicationManager.Object, out _);

        await sut.HandleAsync(new UmbracoApplicationStartingNotification(RuntimeLevel.Run, false), CancellationToken.None);

        var isInitialized = (bool)typeof(InitializeMemberApplicationNotificationHandler)
            .GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        Assert.IsFalse(isInitialized, "A failed attempt must not block a later restart from retrying.");
    }

    /// <summary>
    /// Regression guard: a successful initialization still calls through to the member application manager
    /// and marks initialization as complete, exactly as before the try/catch was added.
    /// </summary>
    [Test]
    public async Task HandleAsync_WhenSuccessful_CallsEnsureMemberApplicationAndMarksInitialized()
    {
        var loginRedirectUrls = new[] { new Uri("https://example.com/login") };
        var memberApplicationManager = new Mock<IMemberApplicationManager>();

        var sut = CreateSut(
            memberApplicationManager.Object,
            out _,
            loginRedirectUrls: loginRedirectUrls);

        await sut.HandleAsync(new UmbracoApplicationStartingNotification(RuntimeLevel.Run, false), CancellationToken.None);

        memberApplicationManager.Verify(
            x => x.EnsureMemberApplicationAsync(loginRedirectUrls, It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        var isInitialized = (bool)typeof(InitializeMemberApplicationNotificationHandler)
            .GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        Assert.IsTrue(isInitialized);
    }

    private static InitializeMemberApplicationNotificationHandler CreateSut(
        IMemberApplicationManager memberApplicationManager,
        out Mock<IServerRoleAccessor> serverRoleAccessor,
        IEnumerable<Uri>? loginRedirectUrls = null)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IMemberApplicationManager))).Returns(memberApplicationManager);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

        var deliveryApiSettings = new DeliveryApiSettings
        {
            MemberAuthorization = new DeliveryApiSettings.MemberAuthorizationSettings
            {
                AuthorizationCodeFlow = new DeliveryApiSettings.AuthorizationCodeFlowSettings
                {
                    Enabled = true,
                    LoginRedirectUrls = loginRedirectUrls ?? [new Uri("https://example.com/login")],
                },
            },
        };

        serverRoleAccessor = new Mock<IServerRoleAccessor>();
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.Single);

        return new InitializeMemberApplicationNotificationHandler(
            Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run),
            Options.Create(deliveryApiSettings),
            Mock.Of<ILogger<InitializeMemberApplicationNotificationHandler>>(),
            serviceScopeFactory.Object,
            Mock.Of<IMemberClientCredentialsManager>(),
            serverRoleAccessor.Object);
    }
}
