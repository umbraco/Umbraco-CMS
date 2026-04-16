namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides access to the <see cref="IBackOfficeSecurity" /> instance for the current request
///     or ambient async context.
/// </summary>
public interface IBackOfficeSecurityAccessor
{
    /// <summary>
    ///     Gets the <see cref="IBackOfficeSecurity" /> instance for the current request or async context.
    /// </summary>
    IBackOfficeSecurity? BackOfficeSecurity { get; }

    /// <summary>
    ///     Sets an ambient <see cref="IBackOfficeSecurity" /> override for the current async flow.
    ///     The override takes precedence over the HTTP-context-based resolution for the lifetime
    ///     of the returned scope. Disposing the scope clears the override.
    /// </summary>
    /// <remarks>
    ///     This is intended for background processing scenarios  where no HTTP context is available
    ///     but CMS code still needs a backoffice identity.
    /// </remarks>
    /// <param name="backOfficeSecurity">The identity to use for the duration of the scope.</param>
    /// <returns>An <see cref="IDisposable" /> that clears the override when disposed.</returns>
    IDisposable Override(IBackOfficeSecurity backOfficeSecurity)
        => throw new NotImplementedException(
            "This implementation does not support ambient identity overrides. "
            + "Ensure you are using an implementation that supports background processing.");
}
