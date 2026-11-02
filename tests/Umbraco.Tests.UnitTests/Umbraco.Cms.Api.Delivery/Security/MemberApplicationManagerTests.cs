// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Immutable;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Text.Json;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Delivery.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Security;

/// <summary>
/// Unit tests for <see cref="MemberApplicationManager"/>. Delivery API member registration shares the
/// create-or-update path with the back office, so it contends over the same row on instances that have
/// not yet settled on a server role (#23544).
/// </summary>
[TestFixture]
public class MemberApplicationManagerTests
{
    private static readonly Uri _loginRedirectUrl = new("https://client.local/callback");
    private static readonly Uri _logoutRedirectUrl = new("https://client.local/signed-out");

    private Mock<IOpenIddictApplicationManager> _mockApplicationManager = null!;
    private Mock<IRuntimeState> _mockRuntimeState = null!;
    private object _storedApplication = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApplicationManager = new Mock<IOpenIddictApplicationManager>();
        _mockRuntimeState = new Mock<IRuntimeState>();
        _storedApplication = new object();

        _mockRuntimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);

        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.Member, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_storedApplication);

        _mockApplicationManager
            .Setup(x => x.GetDisplayNameAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Umbraco member access");

        _mockApplicationManager
            .Setup(x => x.GetClientTypeAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ClientTypes.Public);

        _mockApplicationManager
            .Setup(x => x.GetPermissionsAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create(
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code));

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create(_loginRedirectUrl.AbsoluteUri));

        _mockApplicationManager
            .Setup(x => x.GetPostLogoutRedirectUrisAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create(_logoutRedirectUrl.AbsoluteUri));

        _mockApplicationManager
            .Setup(x => x.GetSettingsAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<string, string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetConsentTypeAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ConsentTypes.Explicit);
        _mockApplicationManager
            .Setup(x => x.GetApplicationTypeAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ApplicationTypes.Web);
        _mockApplicationManager
            .Setup(x => x.GetRequirementsAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray<string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetDisplayNamesAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<CultureInfo, string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetPropertiesAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<string, JsonElement>.Empty);

    }

    /// <summary>
    /// The member application is registered on application start by every instance that has not been
    /// elected a subscriber, so an unchanged registration must not be written again.
    /// </summary>
    [Test]
    public async Task EnsureMemberApplicationAsync_StoredApplicationMatchesDescriptor_DoesNotUpdate()
    {
        var sut = new MemberApplicationManager(_mockApplicationManager.Object, _mockRuntimeState.Object, NullLogger<MemberApplicationManager>.Instance);

        await sut.EnsureMemberApplicationAsync([_loginRedirectUrl], [_logoutRedirectUrl]);

        _mockApplicationManager.Verify(
            x => x.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// A newly configured redirect URL is a real change and must still be written.
    /// </summary>
    [Test]
    public async Task EnsureMemberApplicationAsync_AdditionalRedirectUrl_Updates()
    {
        var sut = new MemberApplicationManager(_mockApplicationManager.Object, _mockRuntimeState.Object, NullLogger<MemberApplicationManager>.Instance);

        await sut.EnsureMemberApplicationAsync(
            [_loginRedirectUrl, new Uri("https://other-client.local/callback")],
            [_logoutRedirectUrl]);

        _mockApplicationManager.Verify(
            x => x.UpdateAsync(_storedApplication, It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
