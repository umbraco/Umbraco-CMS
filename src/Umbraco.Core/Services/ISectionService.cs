using Umbraco.Cms.Core.Sections;

namespace Umbraco.Cms.Core.Services;

public interface ISectionService
{
    /// <summary>
    ///     The cache storage for all applications
    /// </summary>
    IEnumerable<ISection> GetSections();

    /// <summary>
    ///     Get the user group's allowed sections
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    IEnumerable<ISection> GetAllowedSections(int userId);

    /// <summary>
    ///     Gets the application by its alias.
    /// </summary>
    /// <param name="appAlias">The application alias.</param>
    /// <returns></returns>
    ISection? GetByAlias(string appAlias);
}
