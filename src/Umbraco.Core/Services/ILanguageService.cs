﻿using Umbraco.Cms.Core.Models;
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
    ///     Updates an existing <see cref="ILanguage" /> object
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to update</param>
    /// <param name="userId">Optional id of the user saving the language</param>
    Task<Attempt<ILanguage, LanguageOperationStatus>> UpdateAsync(ILanguage language, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a new <see cref="ILanguage" /> object
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to create</param>
    /// <param name="userId">Optional id of the user creating the language</param>
    Task<Attempt<ILanguage, LanguageOperationStatus>> CreateAsync(ILanguage language, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a <see cref="ILanguage" /> by removing it and its usages from the db
    /// </summary>
    /// <param name="isoCode">The ISO code of the <see cref="ILanguage" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the language</param>
    Task<Attempt<ILanguage?, LanguageOperationStatus>> DeleteAsync(string isoCode, int userId = Constants.Security.SuperUserId);
}
