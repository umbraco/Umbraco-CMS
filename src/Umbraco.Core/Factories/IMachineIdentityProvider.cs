namespace Umbraco.Cms.Core.Factories;

/// <summary>
///     Provides a machine identifier for use in cache synchronisation tracking.
/// </summary>
/// <remarks>
///     <para>
///         Implementations are resolved in the order they are registered in
///         <see cref="MachineIdentityProviderCollection" />. The first implementation that returns a non-null
///         value wins.
///     </para>
///     <para>
///         Return <c>null</c> to indicate that the provider does not apply in the current environment,
///         allowing the next provider in the collection to be tried.
///     </para>
///     <para>
///         The returned value is the <em>base</em> identifier only — <see cref="Umbraco.Cms.Core.Configuration.Models.HostingSettings.SiteName" />
///         is appended by <see cref="IMachineInfoFactory" /> after provider resolution.
///     </para>
/// </remarks>
public interface IMachineIdentityProvider
{
    /// <summary>
    ///     Gets the machine identifier, or <c>null</c> if this provider does not apply.
    /// </summary>
    string? GetMachineIdentifier();
}
