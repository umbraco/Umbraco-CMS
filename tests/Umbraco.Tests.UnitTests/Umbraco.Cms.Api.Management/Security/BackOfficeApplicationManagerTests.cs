// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Immutable;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Security;

/// <summary>
/// Unit tests for BackOfficeApplicationManager focusing on exception handling
/// and edge cases in the MergeWithExistingBackOfficeHostsAsync method.
/// </summary>
[TestFixture]
public class BackOfficeApplicationManagerTests
{
    private Mock<IOpenIddictApplicationManager> _mockApplicationManager = null!;
    private Mock<IWebHostEnvironment> _mockWebHostEnvironment = null!;
    private Mock<IRuntimeState> _mockRuntimeState = null!;
    private IOptions<SecuritySettings> _securitySettings = null!;
    private Mock<ILogger<BackOfficeApplicationManager>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApplicationManager = new Mock<IOpenIddictApplicationManager>();
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _mockRuntimeState = new Mock<IRuntimeState>();
        _mockLogger = new Mock<ILogger<BackOfficeApplicationManager>>();

        _securitySettings = Options.Create(new SecuritySettings
        {
            AuthorizeCallbackPathName = "umbraco/oauth_complete",
            AuthorizeCallbackLogoutPathName = "umbraco/logout"
        });

        // Default: RuntimeLevel allows execution
        _mockRuntimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);
    }

    /// <summary>
    /// Tests that when no existing application exists (first server startup),
    /// the method returns the new hosts without errors.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_NoExistingApplication_ReturnsNewHosts()
    {
        // Arrange
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);

        // Set environment to Production to avoid Swagger/Postman application creation
        _mockWebHostEnvironment
            .Setup(x => x.EnvironmentName)
            .Returns("Production");

        var sut = new BackOfficeApplicationManager(
            _mockApplicationManager.Object,
            _mockWebHostEnvironment.Object,
            _securitySettings,
            _mockRuntimeState.Object,
            _mockLogger.Object);

        var newHosts = new[] { new Uri("https://server1.local/") };

        // Act
        await sut.EnsureBackOfficeApplicationAsync(newHosts);

        // Assert - should create back-office application with new hosts
        _mockApplicationManager.Verify(
            x => x.CreateAsync(It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Should create exactly one application (back-office) in Production environment");
    }

    /// <summary>
    /// Tests that when existing redirect URIs contain invalid/malformed URIs,
    /// those invalid URIs are skipped gracefully without throwing exceptions.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_InvalidUriInExisting_SkipsInvalidUri()
    {
        // Arrange
        var mockApplication = new object(); // Mock application object
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        // Mix of valid and invalid URI strings in existing redirect URIs
        var existingRedirectUris = ImmutableArray.Create(
            "https://server1.local/umbraco/oauth_complete", // Valid
            "relative/path", // Invalid: not absolute
            "https://server2.local/umbraco/oauth_complete");  // Valid

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRedirectUris);

        var sut = CreateDefaultMockedBackofficeApplicationManager();

        var newHosts = new[] { new Uri("https://server3.local/") };

        // Act - should not throw exception
        await sut.EnsureBackOfficeApplicationAsync(newHosts);

        // Assert - should update application (skipping invalid URIs)
        _mockApplicationManager.Verify(
            x => x.UpdateAsync(mockApplication, It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that when new hosts contain invalid URIs,
    /// those invalid URIs are skipped without throwing exceptions.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_InvalidUriInNew_SkipsInvalidUri()
    {
        // Arrange
        var mockApplication = new object();
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        var existingRedirectUris = ImmutableArray.Create(
            "https://server1.local/umbraco/oauth_complete");

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRedirectUris);

        var sut = CreateDefaultMockedBackofficeApplicationManager();

        // Note: The method validates input and throws ArgumentException for non-absolute URIs
        // So this test verifies the validation works correctly
        var invalidHosts = new[] { new Uri("relative/path", UriKind.Relative) };

        // Act & Assert - should throw ArgumentException due to input validation
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await sut.EnsureBackOfficeApplicationAsync(invalidHosts));
    }

    /// <summary>
    /// Tests that when existing redirect URIs contain a mix of valid and invalid entries,
    /// only the valid entries are processed and merged with new hosts.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_MixOfValidAndInvalid_OnlyProcessesValid()
    {
        // Arrange
        var mockApplication = new object();
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        var existingRedirectUris = ImmutableArray.Create(
            "https://valid1.local/umbraco/oauth_complete",
            "relative", // Invalid: not absolute
            "https://valid2.local/umbraco/oauth_complete");

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRedirectUris);

        OpenIddictApplicationDescriptor? capturedDescriptor = null;
        _mockApplicationManager
            .Setup(x => x.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictApplicationDescriptor, CancellationToken>((_, descriptor, _) =>
                capturedDescriptor = descriptor)
            .Returns(ValueTask.CompletedTask);

        var sut = CreateDefaultMockedBackofficeApplicationManager();

        var newHosts = new[] { new Uri("https://new.local/") };

        // Act
        await sut.EnsureBackOfficeApplicationAsync(newHosts);

        // Assert
        Assert.That(capturedDescriptor, Is.Not.Null, "Descriptor should be captured");
        Assert.That(
            capturedDescriptor!.RedirectUris.Count,
            Is.EqualTo(3),
            "Should have 3 redirect URIs (2 existing valid + 1 new)");

        var redirectUriStrings = capturedDescriptor.RedirectUris.Select(u => u.ToString()).ToList();
        Assert.That(redirectUriStrings, Does.Contain("https://valid1.local/umbraco/oauth_complete"));
        Assert.That(redirectUriStrings, Does.Contain("https://valid2.local/umbraco/oauth_complete"));
        Assert.That(redirectUriStrings, Does.Contain("https://new.local/umbraco/oauth_complete"));
    }

    /// <summary>
    /// Tests that duplicate hosts (case-insensitive) are not added multiple times.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_DuplicateHosts_DeduplicatesCaseInsensitive()
    {
        // Arrange
        var mockApplication = new object();
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        var existingRedirectUris = ImmutableArray.Create(
            "https://SERVER1.LOCAL/umbraco/oauth_complete"); // Uppercase

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRedirectUris);

        OpenIddictApplicationDescriptor? capturedDescriptor = null;
        _mockApplicationManager
            .Setup(x => x.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictApplicationDescriptor, CancellationToken>((_, descriptor, _) =>
                capturedDescriptor = descriptor)
            .Returns(ValueTask.CompletedTask);

        var sut = CreateDefaultMockedBackofficeApplicationManager();

        var newHosts = new[] { new Uri("https://server1.local/") }; // Lowercase - should deduplicate

        // Act
        await sut.EnsureBackOfficeApplicationAsync(newHosts);

        // Assert
        Assert.That(capturedDescriptor, Is.Not.Null);
        Assert.That(
            capturedDescriptor!.RedirectUris.Count,
            Is.EqualTo(1),
            "Should have only 1 redirect URI (deduplicated by authority)");
    }

    /// <summary>
    /// Tests that when existing redirect URIs contain different paths for the same host,
    /// they are correctly merged by authority (not by full URI).
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_SameHostDifferentPaths_MergesByAuthority()
    {
        // Arrange
        var mockApplication = new object();
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        var existingRedirectUris = ImmutableArray.Create(
            "https://server1.local/some/old/path",
            "https://server1.local/another/old/path");

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRedirectUris);

        OpenIddictApplicationDescriptor? capturedDescriptor = null;
        _mockApplicationManager
            .Setup(x => x.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictApplicationDescriptor, CancellationToken>((_, descriptor, _) =>
                capturedDescriptor = descriptor)
            .Returns(ValueTask.CompletedTask);

        var sut = new BackOfficeApplicationManager(
            _mockApplicationManager.Object,
            _mockWebHostEnvironment.Object,
            _securitySettings,
            _mockRuntimeState.Object,
            _mockLogger.Object);

        var newHosts = new[] { new Uri("https://server1.local/") }; // Same host

        // Act
        await sut.EnsureBackOfficeApplicationAsync(newHosts);

        // Assert - should deduplicate by authority
        Assert.That(capturedDescriptor, Is.Not.Null);
        Assert.That(
            capturedDescriptor!.RedirectUris.Count,
            Is.EqualTo(1),
            "Should have only 1 redirect URI (deduplicated by authority, not full path)");
    }

    /// <summary>
    /// Tests that the method returns early when RuntimeLevel is below Upgrade.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_RuntimeLevelBelowUpgrade_ReturnsEarly()
    {
        // Arrange
        _mockRuntimeState.Setup(x => x.Level).Returns(RuntimeLevel.Install);

        var sut = CreateDefaultMockedBackofficeApplicationManager();

        var newHosts = new[] { new Uri("https://server1.local/") };

        // Act
        await sut.EnsureBackOfficeApplicationAsync(newHosts);

        // Assert - should not call application manager at all
        _mockApplicationManager.Verify(
            x => x.FindByClientIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that when SecuritySettings.BackOfficeHost is configured, the configured host
    /// is appended to the merged hosts rather than replacing them. In a shared-database
    /// environment, hosts from the DB, middleware, and settings must all be preserved.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_WithBackOfficeHostConfigured_PreservesExistingHosts()
    {
        // Arrange
        var configuredHost = new Uri("https://configured-host.local/");
        var securitySettingsWithHost = Options.Create(new SecuritySettings
        {
            BackOfficeHost = configuredHost,
            AuthorizeCallbackPathName = "umbraco/oauth_complete",
            AuthorizeCallbackLogoutPathName = "umbraco/logout"
        });

        var mockApplication = new object();
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        // Existing redirect URIs in the DB (from server1 that started previously)
        var existingRedirectUris = ImmutableArray.Create(
            "https://server1.local/umbraco/oauth_complete");

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRedirectUris);

        OpenIddictApplicationDescriptor? capturedDescriptor = null;
        _mockApplicationManager
            .Setup(x => x.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictApplicationDescriptor, CancellationToken>((_, descriptor, _) =>
                capturedDescriptor = descriptor)
            .Returns(ValueTask.CompletedTask);

        var sut = new BackOfficeApplicationManager(
            _mockApplicationManager.Object,
            _mockWebHostEnvironment.Object,
            securitySettingsWithHost,
            _mockRuntimeState.Object,
            _mockLogger.Object);

        // server2 is the host detected by the middleware on this instance
        var newHosts = new[] { new Uri("https://server2.local/") };

        // Act
        await sut.EnsureBackOfficeApplicationAsync(newHosts);

        // Assert - all three hosts must be present: server1 (DB), server2 (middleware), configured-host (settings)
        Assert.That(capturedDescriptor, Is.Not.Null, "Descriptor should be captured");
        Assert.That(
            capturedDescriptor!.RedirectUris.Count,
            Is.EqualTo(3),
            "Should have 3 redirect URIs (server1 from DB + server2 from middleware + configured-host from settings)");

        var redirectUriStrings = capturedDescriptor.RedirectUris.Select(u => u.ToString()).ToList();
        Assert.That(redirectUriStrings, Does.Contain("https://server1.local/umbraco/oauth_complete"));
        Assert.That(redirectUriStrings, Does.Contain("https://server2.local/umbraco/oauth_complete"));
        Assert.That(redirectUriStrings, Does.Contain("https://configured-host.local/umbraco/oauth_complete"));
    }

    /// <summary>
    /// Tests that when SecuritySettings.BackOfficeHost is set to a host that already exists
    /// in the merged hosts, it is not duplicated.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_WithBackOfficeHostAlreadyInHosts_DoesNotDuplicate()
    {
        // Arrange - BackOfficeHost is the same as what's already in the DB
        var configuredHost = new Uri("https://server1.local/");
        var securitySettingsWithHost = Options.Create(new SecuritySettings
        {
            BackOfficeHost = configuredHost,
            AuthorizeCallbackPathName = "umbraco/oauth_complete",
            AuthorizeCallbackLogoutPathName = "umbraco/logout"
        });

        var mockApplication = new object();
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        var existingRedirectUris = ImmutableArray.Create(
            "https://server1.local/umbraco/oauth_complete");

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRedirectUris);

        OpenIddictApplicationDescriptor? capturedDescriptor = null;
        _mockApplicationManager
            .Setup(x => x.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictApplicationDescriptor, CancellationToken>((_, descriptor, _) =>
                capturedDescriptor = descriptor)
            .Returns(ValueTask.CompletedTask);

        var sut = new BackOfficeApplicationManager(
            _mockApplicationManager.Object,
            _mockWebHostEnvironment.Object,
            securitySettingsWithHost,
            _mockRuntimeState.Object,
            _mockLogger.Object);

        // Pass the same host as middleware-detected
        var newHosts = new[] { new Uri("https://server1.local/") };

        // Act
        await sut.EnsureBackOfficeApplicationAsync(newHosts);

        // Assert - should have only 1 redirect URI (no duplication)
        Assert.That(capturedDescriptor, Is.Not.Null, "Descriptor should be captured");
        Assert.That(
            capturedDescriptor!.RedirectUris.Count,
            Is.EqualTo(1),
            "Should have only 1 redirect URI (no duplication when BackOfficeHost matches existing)");

        var redirectUriStrings = capturedDescriptor.RedirectUris.Select(u => u.ToString()).ToList();
        Assert.That(redirectUriStrings, Does.Contain("https://server1.local/umbraco/oauth_complete"));
    }

    /// <summary>
    /// Tests that a stored application already matching the descriptor is not written again.
    /// Every instance in a load balanced setup merges the existing hosts back into its descriptor,
    /// so the write it would issue changes nothing but the concurrency token (#23544).
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_StoredApplicationMatchesDescriptor_DoesNotUpdate()
    {
        // Arrange
        var mockApplication = new object();
        SetUpStoredBackOfficeApplication(mockApplication, "https://server1.local");

        _mockWebHostEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var sut = CreateDefaultMockedBackofficeApplicationManager();

        // Act
        await sut.EnsureBackOfficeApplicationAsync([new Uri("https://server1.local/")]);

        // Assert
        _mockApplicationManager.Verify(
            x => x.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "An unchanged descriptor must not be written, otherwise concurrent instances contend over the concurrency token");
    }

    /// <summary>
    /// Tests that a retry after a concurrency conflict re-reads the stored hosts. Replaying the
    /// descriptor built before the conflict would drop the host the winning instance just registered,
    /// leaving that server unable to complete a back-office login (#23544).
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_UpdateConflicts_MergesHostsRegisteredByTheWinningInstance()
    {
        // Arrange
        var mockApplication = new object();
        var storedRedirectUris = new List<string> { "https://server1.local/umbraco/oauth_complete" };

        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .Returns(() => ValueTask.FromResult(storedRedirectUris.ToImmutableArray()));

        var writes = new List<OpenIddictApplicationDescriptor>();
        _mockApplicationManager
            .Setup(x => x.UpdateAsync(mockApplication, It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Returns((object _, OpenIddictApplicationDescriptor descriptor, CancellationToken _) =>
            {
                writes.Add(descriptor);
                if (writes.Count > 1)
                {
                    return ValueTask.CompletedTask;
                }

                // Another instance wins the race and registers itself before this write lands.
                storedRedirectUris.Add("https://server3.local/umbraco/oauth_complete");
                throw new OpenIddictExceptions.ConcurrencyException("conflict");
            });

        _mockWebHostEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var sut = CreateDefaultMockedBackofficeApplicationManager();

        // Act
        await sut.EnsureBackOfficeApplicationAsync([new Uri("https://server2.local/")]);

        // Assert
        Assert.That(writes, Has.Count.EqualTo(2), "The conflicting write should be retried");

        var retriedUris = writes[1].RedirectUris.Select(uri => uri.ToString()).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(retriedUris, Does.Contain("https://server1.local/umbraco/oauth_complete"));
            Assert.That(retriedUris, Does.Contain("https://server2.local/umbraco/oauth_complete"));
            Assert.That(
                retriedUris,
                Does.Contain("https://server3.local/umbraco/oauth_complete"),
                "The retry must re-read the stored hosts, otherwise the winning instance's host is discarded");
        });
    }

    /// <summary>
    /// Tests that an application carrying a client secret is always written. OpenIddict stores secrets
    /// hashed, so a supplied secret can never be compared against the stored one and a rotated secret
    /// would otherwise be silently discarded (#23544).
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeClientCredentialsApplicationAsync_EverythingElseMatches_StillUpdates()
    {
        // Arrange
        const string ClientId = "my-client";
        var mockApplication = new object();

        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockApplication);

        _mockApplicationManager
            .Setup(x => x.GetDisplayNameAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync($"Umbraco client credentials back-office access: {ClientId}");

        _mockApplicationManager
            .Setup(x => x.GetClientTypeAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ClientTypes.Confidential);

        _mockApplicationManager
            .Setup(x => x.GetPermissionsAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create(
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials));

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray<string>.Empty);

        _mockApplicationManager
            .Setup(x => x.GetPostLogoutRedirectUrisAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray<string>.Empty);

        _mockApplicationManager
            .Setup(x => x.GetSettingsAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<string, string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetConsentTypeAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ConsentTypes.Explicit);
        _mockApplicationManager
            .Setup(x => x.GetApplicationTypeAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ApplicationTypes.Web);
        _mockApplicationManager
            .Setup(x => x.GetRequirementsAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray<string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetDisplayNamesAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<CultureInfo, string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetPropertiesAsync(mockApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<string, JsonElement>.Empty);


        var sut = CreateDefaultMockedBackofficeApplicationManager();

        // Act
        await sut.EnsureBackOfficeClientCredentialsApplicationAsync(ClientId, "rotated-secret");

        // Assert
        _mockApplicationManager.Verify(
            x => x.UpdateAsync(mockApplication, It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "A descriptor carrying a client secret must always be written, because the stored secret is hashed and cannot be compared");
    }

    /// <summary>
    /// Sets up the mocked application manager to return a stored back-office application whose
    /// state matches what <see cref="BackOfficeApplicationManager"/> builds for the given host.
    /// </summary>
    private void SetUpStoredBackOfficeApplication(object storedApplication, string authority)
    {
        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedApplication);

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create($"{authority}/umbraco/oauth_complete"));

        _mockApplicationManager
            .Setup(x => x.GetPostLogoutRedirectUrisAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create(
                $"{authority}/umbraco/oauth_complete",
                $"{authority}/umbraco/logout"));

        _mockApplicationManager
            .Setup(x => x.GetDisplayNameAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Umbraco back-office access");

        _mockApplicationManager
            .Setup(x => x.GetClientTypeAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ClientTypes.Public);

        _mockApplicationManager
            .Setup(x => x.GetPermissionsAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create(
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code));

        _mockApplicationManager
            .Setup(x => x.GetSettingsAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<string, string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetConsentTypeAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ConsentTypes.Explicit);
        _mockApplicationManager
            .Setup(x => x.GetApplicationTypeAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ApplicationTypes.Web);
        _mockApplicationManager
            .Setup(x => x.GetRequirementsAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray<string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetDisplayNamesAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<CultureInfo, string>.Empty);
        _mockApplicationManager
            .Setup(x => x.GetPropertiesAsync(storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<string, JsonElement>.Empty);
    }

    private BackOfficeApplicationManager CreateDefaultMockedBackofficeApplicationManager() =>
        new(
            _mockApplicationManager.Object,
            _mockWebHostEnvironment.Object,
            _securitySettings,
            _mockRuntimeState.Object,
            _mockLogger.Object);
}
