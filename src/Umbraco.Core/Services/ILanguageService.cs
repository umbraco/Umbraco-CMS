using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface ILanguageService
{
    /// <summary>
    ///     Gets a <see cref="ILanguage" /> by its iso code
    /// </summary>
    /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
    /// <returns>
    ///     <see cref="ILanguage" />
    /// </returns>
    Task<ILanguage?> GetAsync(string isoCode);

    /// <summary>
    ///     Gets the default <see cref="ILanguage" />
    /// </summary>
    /// <returns>
    ///     <see cref="ILanguage" />
    /// </returns>
    Task<ILanguage?> GetDefaultLanguageAsync();

    /// <summary>
    ///     Gets the default language ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    /// <returns>The default language ISO code</returns>
    Task<string> GetDefaultIsoCodeAsync();

    /// <summary>
    ///     Gets all available languages
    /// </summary>
    /// <returns>An enumerable list of <see cref="ILanguage" /> objects</returns>
    Task<IEnumerable<ILanguage>> GetAllAsync();

    /// <summary>
    ///     Gets all languages with the given iso codes
    /// </summary>
    /// <returns>An enumerable list of <see cref="ILanguage" /> objects</returns>
    Task<IEnumerable<ILanguage>> GetMultipleAsync(IEnumerable<string> isoCodes);

    /// <summary>
    ///     Updates an existing <see cref="ILanguage" /> object
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to update</param>
    /// <param name="userKey">Key of the user saving the language</param>
    Task<Attempt<ILanguage, LanguageOperationStatus>> UpdateAsync(ILanguage language, Guid userKey);

    /// <summary>
    ///     Creates a new <see cref="ILanguage" /> object
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to create</param>
    /// <param name="userKey">Key of the user creating the language</param>
    Task<Attempt<ILanguage, LanguageOperationStatus>> CreateAsync(ILanguage language, Guid userKey);

    /// <summary>
    ///     Deletes a <see cref="ILanguage" /> by removing it and its usages from the db
    /// </summary>
    /// <param name="isoCode">The ISO code of the <see cref="ILanguage" /> to delete</param>
    /// <param name="userKey">Key of the user deleting the language</param>
    Task<Attempt<ILanguage?, LanguageOperationStatus>> DeleteAsync(string isoCode, Guid userKey);


    /// <summary>
    /// Retrieves the isoCodes of configured languages by their Ids
    /// </summary>
    /// <param name="ids">The ids of the configured <see cref="ILanguage" />s</param>
    /// <returns>The ISO codes of the <see cref="ILanguage" />s</returns>
    Task<string[]> GetIsoCodesByIdsAsync(ICollection<int> ids);
}
