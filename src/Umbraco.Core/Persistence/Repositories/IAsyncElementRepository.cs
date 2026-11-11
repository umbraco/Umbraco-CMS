using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the repository for <see cref="IElement" /> entities. Elements carry no members of their own beyond the
///     publishable content contract.
/// </summary>
public interface IAsyncElementRepository : IAsyncPublishableContentRepository<IElement>
{
}
