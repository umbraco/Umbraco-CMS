using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents a service for handling <see cref="IConsent"/> entities.
    /// </summary>
    public interface IConsentService : IService
    {
        // notes
        //
        // at the moment this is just channelling CRUD to the repository but
        // the consent service should implement as much of the consenting
        // logic as possible
        //
        // we might need some Get methods by State

        /// <summary>
        /// Gets the consent entity with the specified identifier.
        /// </summary>
        IConsent Get(int id);

        /// <summary>
        /// Gets the consents of a source.
        /// </summary>
        IEnumerable<IConsent> GetBySource(string source, string actionType = null);

        /// <summary>
        /// Gets the consents for an action.
        /// </summary>
        IEnumerable<IConsent> GetByAction(string action);

        /// <summary>
        /// Gets the consents for an action type.
        /// </summary>
        IEnumerable<IConsent> GetByActionType(string actionType);

        /// <summary>
        /// Saves a consent.
        /// </summary>
        void Save(IConsent consent); // fixme should it be an attempt?

        /// <summary>
        /// Deletes a consent.
        /// </summary>
        void Delete(IConsent consent);
    }
}
