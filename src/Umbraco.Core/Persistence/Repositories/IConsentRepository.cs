using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for <see cref="IConsent"/> entities.
    /// </summary>
    public interface IConsentRepository : IRepositoryQueryable<int, IConsent>
    { }
}
