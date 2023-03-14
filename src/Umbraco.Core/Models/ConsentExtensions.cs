using Umbraco.Cms.Core.Models;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="IConsent" /> interface.
/// </summary>
public static class ConsentExtensions
{
    /// <summary>
    ///     Determines whether the consent is granted.
    /// </summary>
    public static bool IsGranted(this IConsent consent) => (consent.State & ConsentState.Granted) > 0;

    /// <summary>
    ///     Determines whether the consent is revoked.
    /// </summary>
    public static bool IsRevoked(this IConsent consent) => (consent.State & ConsentState.Revoked) > 0;
}
