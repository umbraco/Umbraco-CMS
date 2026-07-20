using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

/// <summary>
/// Integration tests for BackOfficeApplicationManager to verify OpenIddict application registration behavior.
/// These tests simulate balanced environment scenarios where multiple servers share the same database.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
public class BackOfficeApplicationManagerTests : UmbracoTestServerTestBase
{
    protected override void CustomTestAuthSetup(IServiceCollection services)
    {
        // We do not want fake authentication for these tests
    }

    [SetUp]
    public override void Setup()
    {
        // Configure ModelsBuilder to avoid boot failure
        InMemoryConfiguration["Umbraco:CMS:ModelsBuilder:ModelsMode"] = "Nothing";
        base.Setup();
    }

    /// <summary>
    /// Tests issue #21138: In balanced environments with multiple servers sharing a database,
    /// redirect URIs should be accumulated across server initializations, not overwritten.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_MultipleServerUrls_PreservesAllRedirectUris()
    {
        // Arrange: Get services within scope (OpenIddict services are scoped)
        using var scope = Services.CreateScope();
        var backOfficeAppManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
        var openIddictAppManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Act: Initialize with first server URL (simulating server1 startup)
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync(
            [new Uri("https://server1.local/")]);

        // Assert: Verify server1 URI exists
        var application = await openIddictAppManager.FindByClientIdAsync(
            Constants.OAuthClientIds.BackOffice);
        Assert.That(application, Is.Not.Null, "Back-office application should be created");

        var redirectUris = await openIddictAppManager.GetRedirectUrisAsync(application);
        Assert.That(
            redirectUris,
            Does.Contain(new Uri("https://server1.local/umbraco/oauth_complete")),
            "Server1 redirect URI should exist after first initialization");

        // Act: Initialize with second server URL (simulating server2 startup on same database)
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync(
            [new Uri("https://server2.local/")]);

        // Assert: BOTH server1 and server2 URIs should exist
        // This tests the bug: faulty behavior overwrote server1 URI
        application = await openIddictAppManager.FindByClientIdAsync(
            Constants.OAuthClientIds.BackOffice);
        redirectUris = await openIddictAppManager.GetRedirectUrisAsync(application);

        Assert.That(
            redirectUris,
            Does.Contain(new Uri("https://server1.local/umbraco/oauth_complete")),
            "Server1 redirect URI should still exist after server2 initialization (BUG: This will fail until #21138 is fixed)");
        Assert.That(
            redirectUris,
            Does.Contain(new Uri("https://server2.local/umbraco/oauth_complete")),
            "Server2 redirect URI should exist after server2 initialization");

        // Act: Add third server (further validation)
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync(
            [new Uri("https://cms.domain.com/")]);

        // Assert: ALL three server URIs should exist
        application = await openIddictAppManager.FindByClientIdAsync(
            Constants.OAuthClientIds.BackOffice);
        redirectUris = await openIddictAppManager.GetRedirectUrisAsync(application);

        var redirectUriList = redirectUris.ToList();
        Assert.That(redirectUriList, Has.Count.EqualTo(3), "Should have redirect URIs for all three servers");
        Assert.That(
            redirectUriList,
            Does.Contain(new Uri("https://server1.local/umbraco/oauth_complete")),
            "Server1 redirect URI should still exist");
        Assert.That(
            redirectUriList,
            Does.Contain(new Uri("https://server2.local/umbraco/oauth_complete")),
            "Server2 redirect URI should still exist");
        Assert.That(
            redirectUriList,
            Does.Contain(new Uri("https://cms.domain.com/umbraco/oauth_complete")),
            "Server3 redirect URI should exist");
    }

    /// <summary>
    /// Tests that re-initializing with the same server URL does not create duplicate redirect URIs.
    /// This simulates a server restarting
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_DuplicateServerUrl_DoesNotCreateDuplicateUris()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var backOfficeAppManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
        var openIddictAppManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var serverUrl = new Uri("https://server1.local/");

        // Act: Initialize with same URL twice
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync([serverUrl]);
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync([serverUrl]);

        // Assert: Should only have one redirect URI
        var application = await openIddictAppManager.FindByClientIdAsync(
            Constants.OAuthClientIds.BackOffice);
        var redirectUris = await openIddictAppManager.GetRedirectUrisAsync(application);
        var redirectUriList = redirectUris.ToList();

        Assert.That(redirectUriList, Has.Count.EqualTo(1), "Should not create duplicate redirect URIs");
        Assert.That(
            redirectUriList,
            Does.Contain(new Uri("https://server1.local/umbraco/oauth_complete")));
    }

    /// <summary>
    /// Tests that PostLogoutRedirectUris are also preserved across multiple server initializations.
    /// Note: The current implementation adds both oauth_complete and logout paths to PostLogoutRedirectUris.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_MultipleServerUrls_PreservesAllPostLogoutRedirectUris()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var backOfficeAppManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
        var openIddictAppManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Act: Initialize with the first server
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync(
            [new Uri("https://server1.local/")]);

        // Assert: Verify server1 post-logout URIs exist
        var application = await openIddictAppManager.FindByClientIdAsync(
            Constants.OAuthClientIds.BackOffice);
        var postLogoutUris = await openIddictAppManager.GetPostLogoutRedirectUrisAsync(application);

        Assert.That(
            postLogoutUris,
            Does.Contain(new Uri("https://server1.local/umbraco/oauth_complete")),
            "Server1 oauth_complete post-logout URI should exist");
        Assert.That(
            postLogoutUris,
            Does.Contain(new Uri("https://server1.local/umbraco/logout")),
            "Server1 logout post-logout URI should exist");

        // Act: Initialize with the second server
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync(
            [new Uri("https://server2.local/")]);

        // Assert: BOTH server URIs should exist in post-logout redirects
        application = await openIddictAppManager.FindByClientIdAsync(
            Constants.OAuthClientIds.BackOffice);
        postLogoutUris = await openIddictAppManager.GetPostLogoutRedirectUrisAsync(application);
        var postLogoutUriList = postLogoutUris.ToList();

        // Server 1 URIs (2 paths per server)
        Assert.That(
            postLogoutUriList,
            Does.Contain(new Uri("https://server1.local/umbraco/oauth_complete")),
            "Server1 oauth_complete should still exist (BUG: This will fail until #21138 is fixed)");
        Assert.That(
            postLogoutUriList,
            Does.Contain(new Uri("https://server1.local/umbraco/logout")),
            "Server1 logout should still exist (BUG: This will fail until #21138 is fixed)");

        // Server 2 URIs (2 paths per server)
        Assert.That(
            postLogoutUriList,
            Does.Contain(new Uri("https://server2.local/umbraco/oauth_complete")),
            "Server2 oauth_complete should exist");
        Assert.That(
            postLogoutUriList,
            Does.Contain(new Uri("https://server2.local/umbraco/logout")),
            "Server2 logout should exist");

        Assert.That(postLogoutUriList, Has.Count.EqualTo(4), "Should have 2 post-logout URIs per server (4 total)");
    }

    /// <summary>
    /// Tests that multiple URIs can be passed in a single call and all are registered.
    /// </summary>
    [Test]
    public async Task EnsureBackOfficeApplicationAsync_MultipleUrlsInSingleCall_RegistersAllUris()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var backOfficeAppManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
        var openIddictAppManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Act: Pass multiple URLs in a single initialization call
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync(
        [
            new Uri("https://server1.local/"),
            new Uri("https://server2.local/"),
            new Uri("https://cms.domain.com/")
        ]);

        // Assert: All URIs should be registered
        var application = await openIddictAppManager.FindByClientIdAsync(
            Constants.OAuthClientIds.BackOffice);
        var redirectUris = await openIddictAppManager.GetRedirectUrisAsync(application!);
        var redirectUriList = redirectUris.ToList();

        Assert.That(redirectUriList, Has.Count.EqualTo(3), "Should register all three URIs");
        Assert.That(redirectUriList, Does.Contain(new Uri("https://server1.local/umbraco/oauth_complete")));
        Assert.That(redirectUriList, Does.Contain(new Uri("https://server2.local/umbraco/oauth_complete")));
        Assert.That(redirectUriList, Does.Contain(new Uri("https://cms.domain.com/umbraco/oauth_complete")));
    }
}
