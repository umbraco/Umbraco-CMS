using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
/// Serves as the base class for managing OpenIddict application entities in Umbraco CMS, providing common functionality for application management operations.
/// </summary>
public abstract class OpenIdDictApplicationManagerBase
{
    private const int MaxCreateOrUpdateAttempts = 5;
    private const int RetryBackoffBaseMilliseconds = 50;

    private readonly ILogger _logger;

    /// <summary>
    /// Gets the OpenIddict application manager used to read and write application registrations.
    /// </summary>
    protected IOpenIddictApplicationManager ApplicationManager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenIdDictApplicationManagerBase"/> class.
    /// </summary>
    /// <param name="applicationManager">The OpenIddict application manager.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    protected OpenIdDictApplicationManagerBase(IOpenIddictApplicationManager applicationManager)
        : this(
            applicationManager,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<OpenIdDictApplicationManagerBase>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenIdDictApplicationManagerBase"/> class.
    /// </summary>
    /// <param name="applicationManager">The OpenIddict application manager.</param>
    /// <param name="logger">The logger used to report contention while registering an application.</param>
    protected OpenIdDictApplicationManagerBase(
        IOpenIddictApplicationManager applicationManager,
        ILogger logger)
    {
        ApplicationManager = applicationManager;
        _logger = logger;
    }

    /// <summary>
    /// Creates or updates an application from a fixed descriptor.
    /// </summary>
    /// <param name="clientDescriptor">The descriptor to apply.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    protected Task CreateOrUpdate(OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
        => CreateOrUpdate(_ => Task.FromResult(clientDescriptor), cancellationToken);

    /// <summary>
    /// Creates or updates an application, rebuilding the descriptor for each attempt so a descriptor
    /// derived from stored state is re-derived if another instance writes first.
    /// </summary>
    /// <param name="clientDescriptorFactory">Builds the descriptor to apply.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    protected async Task CreateOrUpdate(Func<CancellationToken, Task<OpenIddictApplicationDescriptor>> clientDescriptorFactory, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            OpenIddictApplicationDescriptor clientDescriptor = await clientDescriptorFactory(cancellationToken);

            try
            {
                await CreateOrUpdateOnce(clientDescriptor, cancellationToken);
                return;
            }
            catch (OpenIddictExceptions.ConcurrencyException exception)
            {
                if (attempt >= MaxCreateOrUpdateAttempts)
                {
                    _logger.LogError(
                        exception,
                        "Could not register the OpenIddict application {ClientId} after {MaxAttempts} attempts, as another instance wrote to it on every attempt.",
                        clientDescriptor.ClientId,
                        MaxCreateOrUpdateAttempts);
                    throw;
                }

                // Another instance wrote first, so retry, when we will rebuild the descriptor from current state.
                // The delay is randomised because every loser of the race throws at the same moment and we avoid
                // retrying in lockstep.
                var backoff = RetryBackoffBaseMilliseconds * (1 << (attempt - 1));
                _logger.LogDebug(
                    "Concurrent write registering OpenIddict application {ClientId}, retrying (attempt {Attempt} of {MaxAttempts}).",
                    clientDescriptor.ClientId,
                    attempt,
                    MaxCreateOrUpdateAttempts);
                await Task.Delay(Random.Shared.Next(backoff / 2, backoff + 1), cancellationToken);
            }
        }
    }

    private async Task CreateOrUpdateOnce(OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        var identifier = clientDescriptor.ClientId ??
                         throw new ApplicationException($"ClientId is missing for application: {clientDescriptor.DisplayName ?? "(no name)"}");
        var client = await ApplicationManager.FindByClientIdAsync(identifier, cancellationToken);
        if (client is null)
        {
            await ApplicationManager.CreateAsync(clientDescriptor, cancellationToken);
            return;
        }

        // Writing an unchanged application still rotates its concurrency token, so instances sharing
        // a database fail each other's requests over a write that changes nothing (#23544).
        if (await MatchesAsync(client, clientDescriptor, cancellationToken))
        {
            return;
        }

        await ApplicationManager.UpdateAsync(client, clientDescriptor, cancellationToken);
    }

    private async Task<bool> MatchesAsync(object client, OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        if (HasStateThatCannotBeCompared(clientDescriptor))
        {
            return false;
        }

        return await MatchesRegistrationAsync(client, clientDescriptor, cancellationToken)
               && await MatchesRedirectUrisAsync(client, clientDescriptor, cancellationToken)
               && await MatchesSettingsAsync(client, clientDescriptor, cancellationToken)
               && await MatchesMetadataAsync(client, clientDescriptor, cancellationToken);
    }

    /// <summary>
    /// State that cannot be read back from the store, so a descriptor carrying it is always written.
    /// </summary>
    /// <remarks>
    /// Only two qualify. Secrets are stored hashed, so a supplied secret can never be compared and
    /// skipping it would silently discard a rotated one. <see cref="OpenIddictApplicationDescriptor.JsonWebKeySet"/> has no value
    /// equality, and comparing a serialised form would be sensitive to key ordering and formatting.
    /// Everything else the descriptor carries is readable through the manager and is compared, so
    /// clearing a value is recognised as a change rather than skipped — except where the store
    /// substitutes a default for a value that was never set, which makes a removal indistinguishable
    /// from an unset value and so leaves it out of scope for comparison.
    /// </remarks>
    private static bool HasStateThatCannotBeCompared(OpenIddictApplicationDescriptor clientDescriptor)
        => clientDescriptor.ClientSecret is not null || clientDescriptor.JsonWebKeySet is not null;

    private async Task<bool> MatchesRegistrationAsync(object client, OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        var displayName = await ApplicationManager.GetDisplayNameAsync(client, cancellationToken);
        var clientType = await ApplicationManager.GetClientTypeAsync(client, cancellationToken);
        ImmutableArray<string> permissions = await ApplicationManager.GetPermissionsAsync(client, cancellationToken);

        return string.Equals(displayName, clientDescriptor.DisplayName, StringComparison.Ordinal)
               && string.Equals(clientType, clientDescriptor.ClientType, StringComparison.OrdinalIgnoreCase)
               && SetEquals(permissions, clientDescriptor.Permissions);
    }

    private async Task<bool> MatchesRedirectUrisAsync(object client, OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        ImmutableArray<string> redirectUris = await ApplicationManager.GetRedirectUrisAsync(client, cancellationToken);
        ImmutableArray<string> postLogoutRedirectUris = await ApplicationManager.GetPostLogoutRedirectUrisAsync(client, cancellationToken);

        return UriSetEquals(redirectUris, clientDescriptor.RedirectUris)
               && UriSetEquals(postLogoutRedirectUris, clientDescriptor.PostLogoutRedirectUris);
    }

    private async Task<bool> MatchesSettingsAsync(object client, OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        ImmutableDictionary<string, string> settings = await ApplicationManager.GetSettingsAsync(client, cancellationToken);

        return settings.Count == clientDescriptor.Settings.Count
               && settings.All(setting =>
                   clientDescriptor.Settings.TryGetValue(setting.Key, out var value)
                   && string.Equals(setting.Value, value, StringComparison.Ordinal));
    }

    /// <summary>
    /// Compares the descriptor state that is readable but not part of the core registration.
    /// </summary>
    /// <remarks>
    /// Read back from the store and compared, so a descriptor that clears one of these is recognised
    /// as a change rather than silently ignored. Values for which the store substitutes a default are
    /// the exception, and are compared only when the descriptor specifies one; see below.
    /// </remarks>
    private async Task<bool> MatchesMetadataAsync(object client, OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        var consentType = await ApplicationManager.GetConsentTypeAsync(client, cancellationToken);
        var applicationType = await ApplicationManager.GetApplicationTypeAsync(client, cancellationToken);
        ImmutableArray<string> requirements = await ApplicationManager.GetRequirementsAsync(client, cancellationToken);
        ImmutableDictionary<CultureInfo, string> displayNames = await ApplicationManager.GetDisplayNamesAsync(client, cancellationToken);
        ImmutableDictionary<string, JsonElement> properties = await ApplicationManager.GetPropertiesAsync(client, cancellationToken);

        // ConsentType and ApplicationType are compared only when the descriptor specifies a value: the
        // store substitutes its own default when none was set, so an unset descriptor value cannot be
        // told apart from a cleared one, and comparing against the default would report a change on
        // every call. Clearing those two is therefore not expressible. Requirements, DisplayNames and
        // Properties are stored as given, so an absent value there is a genuine removal.
        return (clientDescriptor.ConsentType is null
                || string.Equals(consentType, clientDescriptor.ConsentType, StringComparison.OrdinalIgnoreCase))
               && (clientDescriptor.ApplicationType is null
                   || string.Equals(applicationType, clientDescriptor.ApplicationType, StringComparison.OrdinalIgnoreCase))
               && SetEquals(requirements, clientDescriptor.Requirements)
               && DictionaryEquals(displayNames, clientDescriptor.DisplayNames, string.Equals)
               && DictionaryEquals(properties, clientDescriptor.Properties, JsonElementEquals);
    }

    private static bool DictionaryEquals<TKey, TValue>(
        ImmutableDictionary<TKey, TValue>? stored,
        IDictionary<TKey, TValue> expected,
        Func<TValue, TValue, bool> valueEquals)
        where TKey : notnull
    {
        // An unset collection can come back null rather than empty depending on the store, and the
        // two mean the same thing here.
        ImmutableDictionary<TKey, TValue> storedOrEmpty = stored ?? ImmutableDictionary<TKey, TValue>.Empty;

        return storedOrEmpty.Count == expected.Count
               && storedOrEmpty.All(entry => expected.TryGetValue(entry.Key, out TValue? value) && valueEquals(entry.Value, value));
    }

    // JsonElement has no value equality, and the raw text preserves what was actually stored.
    private static bool JsonElementEquals(JsonElement stored, JsonElement expected)
        => string.Equals(stored.GetRawText(), expected.GetRawText(), StringComparison.Ordinal);

    private static bool SetEquals(ImmutableArray<string> stored, ICollection<string> expected)
        => AsSet(stored, StringComparer.Ordinal).SetEquals(expected);

    private static bool UriSetEquals(ImmutableArray<string> stored, ICollection<Uri> expected)
    {
        HashSet<string> storedUris = AsSet(stored, StringComparer.OrdinalIgnoreCase);
        if (storedUris.Count != expected.Count)
        {
            return false;
        }

        return expected.All(uri => storedUris.Contains(uri.AbsoluteUri) || storedUris.Contains(uri.OriginalString));
    }

    private static HashSet<string> AsSet(ImmutableArray<string> values, StringComparer comparer)
        => values.IsDefaultOrEmpty ? new HashSet<string>(comparer) : values.ToHashSet(comparer);

    /// <summary>
    /// Deletes the application with the given client identifier, if it exists.
    /// </summary>
    /// <param name="identifier">The client identifier of the application to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    protected async Task Delete(string identifier, CancellationToken cancellationToken)
    {
        var client = await ApplicationManager.FindByClientIdAsync(identifier, cancellationToken);
        if (client is null)
        {
            return;
        }

        await ApplicationManager.DeleteAsync(client, cancellationToken);
    }
}
