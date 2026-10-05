using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="ITemplate" /> entities.
/// </summary>
/// <remarks>
///     The repository persists template data only. Reading and writing the template view files is the responsibility of
///     the caller.
/// </remarks>
public interface ITemplateRepository : IAsyncReadWriteRepository<Guid, ITemplate>
{
    /// <summary>
    ///     Gets a template by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The template if found; otherwise, <c>null</c>.</returns>
    Task<ITemplate?> GetByAliasAsync(string alias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all descendant templates of a layout template, ordered by level.
    /// </summary>
    /// <param name="layoutTemplateKey">
    ///     The key of the layout template, or <c>null</c> to get all templates, starting with those that have no layout
    ///     template.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The descendant templates, or an empty collection if the layout template does not exist.</returns>
    Task<IEnumerable<ITemplate>> GetDescendantsAsync(Guid? layoutTemplateKey, CancellationToken cancellationToken);
}
