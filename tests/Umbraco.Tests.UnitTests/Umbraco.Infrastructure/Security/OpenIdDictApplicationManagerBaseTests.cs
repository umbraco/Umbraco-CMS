// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

/// <summary>
/// Unit tests for <see cref="OpenIdDictApplicationManagerBase"/>, covering the conditions under which
/// a stored application is considered unchanged and therefore not written again (#23544).
/// </summary>
[TestFixture]
public class OpenIdDictApplicationManagerBaseTests
{
    private const string ClientId = "test-client";

    private Mock<IOpenIddictApplicationManager> _mockApplicationManager = null!;
    private object _storedApplication = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApplicationManager = new Mock<IOpenIddictApplicationManager>();
        _storedApplication = new object();

        _mockApplicationManager
            .Setup(x => x.FindByClientIdAsync(ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_storedApplication);

        _mockApplicationManager
            .Setup(x => x.GetDisplayNameAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test application");

        _mockApplicationManager
            .Setup(x => x.GetClientTypeAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ClientTypes.Public);

        _mockApplicationManager
            .Setup(x => x.GetPermissionsAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create(
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token));

        _mockApplicationManager
            .Setup(x => x.GetRedirectUrisAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create("https://server1.local/umbraco/oauth_complete"));

        _mockApplicationManager
            .Setup(x => x.GetPostLogoutRedirectUrisAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableArray.Create("https://server1.local/umbraco/logout"));

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
    /// A descriptor asking for a value the store does not hold is a change, so it must be written.
    /// </summary>
    /// <remarks>
    /// Only the descriptor-specifies-a-value direction is asserted. The store substitutes a default
    /// for an unset consent type, so a descriptor that leaves it unset is not expressing a removal
    /// and there is no cleared case to assert.
    /// </remarks>
    [TestCase(null, OpenIddictConstants.ConsentTypes.Explicit, TestName = "ConsentType added")]
    public async Task CreateOrUpdate_ConsentTypeDiffersFromStored_Updates(string? stored, string? descriptorValue)
    {
        _mockApplicationManager
            .Setup(x => x.GetConsentTypeAsync(_storedApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        OpenIddictApplicationDescriptor descriptor = MatchingDescriptor();
        descriptor.ConsentType = descriptorValue;

        var sut = new TestApplicationManager(_mockApplicationManager.Object);
        await sut.CreateOrUpdateAsync(descriptor);

        VerifyUpdated(Times.Once());
    }

    /// <summary>
    /// The same, for the remaining readable metadata: state the store holds and the descriptor does
    /// not is a removal, and a removal is a change.
    /// </summary>
    [TestCaseSource(nameof(MetadataHeldByTheStoreButNotTheDescriptor))]
    public async Task CreateOrUpdate_StoredMetadataAbsentFromDescriptor_Updates(
        Action<Mock<IOpenIddictApplicationManager>, object> stubStoredValue)
    {
        stubStoredValue(_mockApplicationManager, _storedApplication);
        var sut = new TestApplicationManager(_mockApplicationManager.Object);

        await sut.CreateOrUpdateAsync(MatchingDescriptor());

        VerifyUpdated(Times.Once());
    }

    private static IEnumerable<TestCaseData> MetadataHeldByTheStoreButNotTheDescriptor()
    {
        yield return Case(
            "Requirements",
            (mock, stored) => mock
                .Setup(x => x.GetRequirementsAsync(stored, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableArray.Create(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange)));

        yield return Case(
            "DisplayNames",
            (mock, stored) => mock
                .Setup(x => x.GetDisplayNamesAsync(stored, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableDictionary<CultureInfo, string>.Empty
                    .Add(CultureInfo.GetCultureInfo("da-DK"), "Testprogram")));

        yield return Case(
            "Properties",
            (mock, stored) => mock
                .Setup(x => x.GetPropertiesAsync(stored, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableDictionary<string, JsonElement>.Empty
                    .Add("custom", JsonDocument.Parse("\"value\"").RootElement)));

        static TestCaseData Case(string name, Action<Mock<IOpenIddictApplicationManager>, object> stubStoredValue)
            => new TestCaseData(stubStoredValue).SetArgDisplayNames(name);
    }

    /// <summary>
    /// The other direction: state the descriptor carries and the store does not is an addition, and
    /// an addition is a change.
    /// </summary>
    [TestCaseSource(nameof(MetadataHeldByTheDescriptorButNotTheStore))]
    public async Task CreateOrUpdate_DescriptorMetadataAbsentFromStore_Updates(Action<OpenIddictApplicationDescriptor> mutate)
    {
        OpenIddictApplicationDescriptor descriptor = MatchingDescriptor();
        mutate(descriptor);

        var sut = new TestApplicationManager(_mockApplicationManager.Object);

        await sut.CreateOrUpdateAsync(descriptor);

        VerifyUpdated(Times.Once());
    }

    private static IEnumerable<TestCaseData> MetadataHeldByTheDescriptorButNotTheStore()
    {
        // Native rather than Web: the store defaults to Web, so asking for Web is not a change.
        yield return Case("ApplicationType", d => d.ApplicationType = OpenIddictConstants.ApplicationTypes.Native);
        yield return Case("Requirements", d => d.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange));
        yield return Case("DisplayNames", d => d.DisplayNames[CultureInfo.GetCultureInfo("da-DK")] = "Testprogram");
        yield return Case("Properties", d => d.Properties["custom"] = JsonDocument.Parse("\"value\"").RootElement);

        static TestCaseData Case(string name, Action<OpenIddictApplicationDescriptor> mutate)
            => new TestCaseData(mutate).SetArgDisplayNames(name);
    }

    /// <summary>
    /// Every property on the descriptor is either compared, used to find the application, or
    /// deliberately treated as always-write.
    /// </summary>
    /// <remarks>
    /// This asserts a count rather than behaviour on purpose. A package bump that adds a property
    /// lands it in none of those buckets, where the comparison would treat it as matching and skip
    /// a write the caller asked for. That failure is silent, so this fails the build instead and
    /// forces the new property to be classified.
    ///
    /// If this fails after an OpenIddict upgrade, decide which bucket the new property belongs in,
    /// add it to the comparison or to <c>HasStateThatCannotBeCompared</c>, then update the count.
    /// </remarks>
    [Test]
    public void OpenIddictApplicationDescriptor_HasTheExpectedNumberOfProperties()
    {
        const int classifiedProperties = 14;

        var actual = typeof(OpenIddictApplicationDescriptor).GetProperties().Length;

        Assert.That(
            actual,
            Is.EqualTo(classifiedProperties),
            $"OpenIddictApplicationDescriptor now has {actual} properties rather than {classifiedProperties}. "
            + "Classify the new property in OpenIdDictApplicationManagerBase before updating this count.");
    }

    /// <summary>
    /// A descriptor equal to the stored application must not be written again.
    /// </summary>
    [Test]
    public async Task CreateOrUpdate_DescriptorMatchesStoredApplication_DoesNotUpdate()
    {
        var sut = new TestApplicationManager(_mockApplicationManager.Object);

        await sut.CreateOrUpdateAsync(MatchingDescriptor());

        VerifyUpdated(Times.Never());
    }

    /// <summary>
    /// Permissions are a set, so the same permissions in a different order are not a change.
    /// </summary>
    [Test]
    public async Task CreateOrUpdate_PermissionsInDifferentOrder_DoesNotUpdate()
    {
        OpenIddictApplicationDescriptor descriptor = MatchingDescriptor();
        descriptor.Permissions.Clear();
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);

        var sut = new TestApplicationManager(_mockApplicationManager.Object);

        await sut.CreateOrUpdateAsync(descriptor);

        VerifyUpdated(Times.Never());
    }

    /// <summary>
    /// Descriptor state that cannot be compared against the stored application must always be written,
    /// so that a derived manager setting it never has its change silently dropped.
    /// </summary>
    [TestCaseSource(nameof(UncomparableDescriptorStateCases))]
    public async Task CreateOrUpdate_DescriptorSetsUncomparableState_Updates(Action<OpenIddictApplicationDescriptor> mutate)
    {
        OpenIddictApplicationDescriptor descriptor = MatchingDescriptor();
        mutate(descriptor);

        var sut = new TestApplicationManager(_mockApplicationManager.Object);

        await sut.CreateOrUpdateAsync(descriptor);

        VerifyUpdated(Times.Once());
    }

    /// <summary>
    /// Another instance writing first must not fail this request.
    /// </summary>
    [Test]
    public async Task CreateOrUpdate_UpdateConflictsOnce_RetriesAndSucceeds()
    {
        _mockApplicationManager
            .SetupSequence(x => x.UpdateAsync(_storedApplication, It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Throws(new OpenIddictExceptions.ConcurrencyException("conflict"))
            .Returns(ValueTask.CompletedTask);

        var sut = new TestApplicationManager(_mockApplicationManager.Object);

        await sut.CreateOrUpdateAsync(ChangedDescriptor());

        VerifyUpdated(Times.Exactly(2));
    }

    /// <summary>
    /// A conflict that does not clear must surface rather than leave the application unregistered.
    /// </summary>
    [Test]
    public void CreateOrUpdate_UpdateAlwaysConflicts_Rethrows()
    {
        _mockApplicationManager
            .Setup(x => x.UpdateAsync(_storedApplication, It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Throws(new OpenIddictExceptions.ConcurrencyException("conflict"));

        var sut = new TestApplicationManager(_mockApplicationManager.Object);

        Assert.ThrowsAsync<OpenIddictExceptions.ConcurrencyException>(
            async () => await sut.CreateOrUpdateAsync(ChangedDescriptor()));
    }

    /// <summary>
    /// A retry must rebuild the descriptor from current state. Re-sending the descriptor built before
    /// the conflict would discard whatever the instance that won the race had just written.
    /// </summary>
    [Test]
    public async Task CreateOrUpdate_UpdateConflicts_RebuildsDescriptorBeforeRetrying()
    {
        _mockApplicationManager
            .SetupSequence(x => x.UpdateAsync(_storedApplication, It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Throws(new OpenIddictExceptions.ConcurrencyException("conflict"))
            .Returns(ValueTask.CompletedTask);

        var built = 0;
        var sut = new TestApplicationManager(_mockApplicationManager.Object);

        await sut.CreateOrUpdateAsync(_ =>
        {
            built++;
            OpenIddictApplicationDescriptor descriptor = MatchingDescriptor();
            descriptor.DisplayName = $"Attempt {built}";
            return Task.FromResult(descriptor);
        });

        Assert.That(built, Is.EqualTo(2), "The descriptor should be rebuilt for the retry");
        _mockApplicationManager.Verify(
            x => x.UpdateAsync(
                _storedApplication,
                It.Is<OpenIddictApplicationDescriptor>(d => d.DisplayName == "Attempt 2"),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "The retry should write the rebuilt descriptor, not the one built before the conflict");
    }

    private static OpenIddictApplicationDescriptor ChangedDescriptor()
    {
        OpenIddictApplicationDescriptor descriptor = MatchingDescriptor();
        descriptor.DisplayName = "Renamed application";
        return descriptor;
    }

    private static IEnumerable<TestCaseData> UncomparableDescriptorStateCases()
    {
        yield return Case("ClientSecret", d => d.ClientSecret = "a-secret");
        yield return Case("JsonWebKeySet", d => d.JsonWebKeySet = new JsonWebKeySet());

        static TestCaseData Case(string name, Action<OpenIddictApplicationDescriptor> mutate)
            => new TestCaseData(mutate).SetArgDisplayNames(name);
    }

    private static OpenIddictApplicationDescriptor MatchingDescriptor()
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = ClientId,
            DisplayName = "Test application",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
            },
        };

        descriptor.RedirectUris.Add(new Uri("https://server1.local/umbraco/oauth_complete"));
        descriptor.PostLogoutRedirectUris.Add(new Uri("https://server1.local/umbraco/logout"));

        return descriptor;
    }

    private void VerifyUpdated(Times times) =>
        _mockApplicationManager.Verify(
            x => x.UpdateAsync(_storedApplication, It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()),
            times);

    private sealed class TestApplicationManager : OpenIdDictApplicationManagerBase
    {
        public TestApplicationManager(IOpenIddictApplicationManager applicationManager)
            : base(applicationManager, NullLogger.Instance)
        {
        }

        public Task CreateOrUpdateAsync(OpenIddictApplicationDescriptor descriptor, CancellationToken cancellationToken = default)
            => CreateOrUpdate(descriptor, cancellationToken);

        public Task CreateOrUpdateAsync(Func<CancellationToken, Task<OpenIddictApplicationDescriptor>> descriptorFactory, CancellationToken cancellationToken = default)
            => CreateOrUpdate(descriptorFactory, cancellationToken);
    }
}
