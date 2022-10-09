using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a consent state.
/// </summary>
/// <remarks>
///     <para>
///         A consent is fully identified by a source (whoever is consenting), a context (for
///         example, an application), and an action (whatever is consented).
///     </para>
///     <para>A consent state registers the state of the consent (granted, revoked...).</para>
/// </remarks>
public interface IConsent : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Determines whether the consent entity represents the current state.
    /// </summary>
    bool Current { get; }

    /// <summary>
    ///     Gets the unique identifier of whoever is consenting.
    /// </summary>
    string? Source { get; }

    /// <summary>
    ///     Gets the unique identifier of the context of the consent.
    /// </summary>
    /// <remarks>
    ///     <para>Represents the domain, application, scope... of the action.</para>
    ///     <para>When the action is a Udi, this should be the Udi type.</para>
    /// </remarks>
    string? Context { get; }

    /// <summary>
    ///     Gets the unique identifier of the consented action.
    /// </summary>
    string? Action { get; }

    /// <summary>
    ///     Gets the state of the consent.
    /// </summary>
    ConsentState State { get; }

    /// <summary>
    ///     Gets some additional free text.
    /// </summary>
    string? Comment { get; }

    /// <summary>
    ///     Gets the previous states of this consent.
    /// </summary>
    IEnumerable<IConsent>? History { get; }
}
