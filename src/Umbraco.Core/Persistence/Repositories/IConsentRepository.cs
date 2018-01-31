using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for <see cref="IConsent"/> entities.
    /// </summary>
    public interface IConsentRepository : IRepositoryQueryable<int, IConsent>
    {
        /// <summary>
        /// Clears the current flag.
        /// </summary>
        void ClearCurrent(string source, string context, string action);
    }
}
