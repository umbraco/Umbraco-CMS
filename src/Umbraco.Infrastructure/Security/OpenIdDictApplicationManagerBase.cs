using System.Collections.Immutable;
using OpenIddict.Abstractions;

namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
/// Serves as the base class for managing OpenIddict application entities in Umbraco CMS, providing common functionality for application management operations.
/// </summary>
public abstract class OpenIdDictApplicationManagerBase
{
    private const int MaxCreateOrUpdateAttempts = 3;

    protected IOpenIddictApplicationManager ApplicationManager { get; }

    protected OpenIdDictApplicationManagerBase(IOpenIddictApplicationManager applicationManager)
        => ApplicationManager = applicationManager;

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
            try
            {
                await CreateOrUpdateOnce(clientDescriptorFactory, cancellationToken);
                return;
            }
            catch (OpenIddictExceptions.ConcurrencyException) when (attempt < MaxCreateOrUpdateAttempts)
            {
                // Another instance wrote first, so rebuild the descriptor from current state and retry.
            }
        }
    }

    private async Task CreateOrUpdateOnce(Func<CancellationToken, Task<OpenIddictApplicationDescriptor>> clientDescriptorFactory, CancellationToken cancellationToken)
    {
        OpenIddictApplicationDescriptor clientDescriptor = await clientDescriptorFactory(cancellationToken);
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
               && await MatchesSettingsAsync(client, clientDescriptor, cancellationToken);
    }

    /// <summary>
    /// Descriptor state that is not compared is treated as a change, so a derived manager setting it
    /// never has its write silently skipped. Secrets are stored hashed and can never be compared.
    /// </summary>
    private static bool HasStateThatCannotBeCompared(OpenIddictApplicationDescriptor clientDescriptor)
    {
        object?[] uncomparableValues =
        [
            clientDescriptor.ClientSecret,
            clientDescriptor.ConsentType,
            clientDescriptor.ApplicationType,
            clientDescriptor.JsonWebKeySet,
        ];

        int[] uncomparableCounts =
        [
            clientDescriptor.Requirements.Count,
            clientDescriptor.DisplayNames.Count,
            clientDescriptor.Properties.Count,
        ];

        return uncomparableValues.Any(value => value is not null)
               || uncomparableCounts.Any(count => count > 0);
    }

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
