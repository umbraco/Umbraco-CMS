using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents a service for handling audit entries.
/// </summary>
public interface IAuditEntryService : IService
{
    /// <summary>
    ///     Writes an audit entry for an audited event.
    /// </summary>
    /// <param name="performingUserKey">The key of the user triggering the audited event.</param>
    /// <param name="performingDetails">Free-form details about the user triggering the audited event.</param>
    /// <param name="performingIp">The IP address or the request triggering the audited event.</param>
    /// <param name="eventDateUtc">The date and time of the audited event.</param>
    /// <param name="affectedUserKey">The identifier of the user affected by the audited event.</param>
    /// <param name="affectedDetails">Free-form details about the entity affected by the audited event.</param>
    /// <param name="eventType">
    ///     The type of the audited event - must contain only alphanumeric chars and hyphens with forward slashes separating
    ///     categories.
    ///     <example>
    ///         The eventType will generally be formatted like: {application}/{entity-type}/{category}/{sub-category}
    ///         Example: umbraco/user/sign-in/failed
    ///     </example>
    /// </param>
    /// <param name="eventDetails">Free-form details about the audited event.</param>
    /// <returns>The created audit entry.</returns>
    public Task<IAuditEntry> WriteAsync(
        Guid? performingUserKey,
        string performingDetails,
        string performingIp,
        DateTime eventDateUtc,
        Guid? affectedUserKey,
        string? affectedDetails,
        string eventType,
        string eventDetails);
}
