namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides access to the <see cref="IBackOfficeSecurity" /> instance for the current request.
/// </summary>
public interface IBackOfficeSecurityAccessor
{
    /// <summary>
    ///     Gets the <see cref="IBackOfficeSecurity" /> instance for the current request.
    /// </summary>
    IBackOfficeSecurity? BackOfficeSecurity { get; }
}
