using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IConsent" /> entities.
/// </summary>
public interface IConsentRepository : IReadWriteQueryRepository<int, IConsent>
{
    /// <summary>
    ///     Clears the current flag.
    /// </summary>
    void ClearCurrent(string source, string context, string action);
}
