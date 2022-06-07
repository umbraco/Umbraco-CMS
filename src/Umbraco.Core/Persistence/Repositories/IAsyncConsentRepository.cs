using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for <see cref="IConsent"/> entities.
    /// </summary>
    public interface IAsyncConsentRepository : IConsentRepository
    {
        /// <summary>
        /// Clears the current flag.
        /// </summary>
        Task ClearCurrentAsync(string source, string context, string action);
    }
}
