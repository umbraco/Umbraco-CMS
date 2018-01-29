using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a consent.
    /// </summary>
    public interface IConsent : IAggregateRoot, IRememberBeingDirty
    {
        /// <summary>
        /// Gets or sets the unique identifier of whoever is consenting.
        /// </summary>
        /// <remarks>Could be a Udi, or anything really.</remarks>
        string Source { get; set; }

        /// <summary>
        /// Gets or sets the Udi of the consented action.
        /// </summary>
        /// <remarks>Could be a Udi, or anything really.</remarks>
        string Action { get; set; }

        /// <summary>
        /// Gets the type of the consented action.
        /// </summary>
        /// <remarks>
        /// <para>Represents the domain, application, scope... of the action.</para>
        /// <para>When the action is a Udi, this should be the Udi type.</para>
        /// </remarks>
        string ActionType { get; }

        /// <summary>
        /// Gets or sets the state of the consent.
        /// </summary>
        ConsentState State { get; set; }

        /// <summary>
        /// Gets or sets some additional free text.
        /// </summary>
        string Comment { get; set; }
    }
}
