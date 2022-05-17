using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an audited event.
/// </summary>
/// <remarks>
///     <para>
///         The free-form details properties can be used to capture relevant infos (for example,
///         a user email and identifier) at the time of the audited event, even though they may change
///         later on - but we want to keep a track of their value at that time.
///     </para>
///     <para>
///         Depending on audit loggers, these properties can be purely free-form text, or
///         contain json serialized objects.
///     </para>
/// </remarks>
public interface IAuditEntry : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the identifier of the user triggering the audited event.
    /// </summary>
    int PerformingUserId { get; set; }

    /// <summary>
    ///     Gets or sets free-form details about the user triggering the audited event.
    /// </summary>
    string? PerformingDetails { get; set; }

    /// <summary>
    ///     Gets or sets the IP address or the request triggering the audited event.
    /// </summary>
    string? PerformingIp { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the audited event.
    /// </summary>
    DateTime EventDateUtc { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user affected by the audited event.
    /// </summary>
    /// <remarks>Not used when no single user is affected by the event.</remarks>
    int AffectedUserId { get; set; }

    /// <summary>
    ///     Gets or sets free-form details about the entity affected by the audited event.
    /// </summary>
    /// <remarks>The entity affected by the event can be another user, a member...</remarks>
    string? AffectedDetails { get; set; }

    /// <summary>
    ///     Gets or sets the type of the audited event.
    /// </summary>
    string? EventType { get; set; }

    /// <summary>
    ///     Gets or sets free-form details about the audited event.
    /// </summary>
    string? EventDetails { get; set; }
}
