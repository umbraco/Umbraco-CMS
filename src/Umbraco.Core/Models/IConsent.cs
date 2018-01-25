using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a consent.
    /// </summary>
    public interface IConsent : IAggregateRoot, IRememberBeingDirty
    {
        /// <summary>
        /// Gets or sets the Udi of whoever is consenting.
        /// </summary>
        Udi Source { get; set; }

        /// <summary>
        /// Gets or sets the Udi of the consented action.
        /// </summary>
        Udi Action { get; set; }

        /// <summary>
        /// Gets the type of the consented action.
        /// </summary>
        /// <remarks>This is the Udi type of <see cref="Action"/> and represents
        /// the domain, application, scope... of the action.</remarks>
        string ActionType { get; } // eg "forms-actions" -> "forms-actions/publish-my-details"

        /// <summary>
        /// Gets or sets the state of the consent.
        /// </summary>
        ConsentState State { get; set; }

        /// <summary>
        /// Gets or sets - fixme - comment, or additional json data?
        /// </summary>
        string Comment { get; set; }
    }
}
