using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="ITemplate" /> entities.
/// </summary>
public interface ITemplateRepository : IReadWriteQueryRepository<int, ITemplate>, IFileRepository
{
    /// <summary>
    ///     Gets a template by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>The template if found; otherwise, <c>null</c>.</returns>
    ITemplate? Get(string? alias);

    /// <summary>
    ///     Gets templates by their aliases.
    /// </summary>
    /// <param name="aliases">The aliases of the templates.</param>
    /// <returns>A collection of templates.</returns>
    IEnumerable<ITemplate> GetAll(params string[] aliases);

    /// <summary>
    ///     Gets all child templates of a master template.
    /// </summary>
    /// <param name="masterTemplateId">The identifier of the master template.</param>
    /// <returns>A collection of child templates.</returns>
    IEnumerable<ITemplate> GetChildren(int masterTemplateId);

    /// <summary>
    ///     Gets all descendant templates of a master template.
    /// </summary>
    /// <param name="masterTemplateId">The identifier of the master template.</param>
    /// <returns>A collection of descendant templates.</returns>
    IEnumerable<ITemplate> GetDescendants(int masterTemplateId);
}
