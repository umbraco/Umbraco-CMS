using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="ILanguage" /> entities.
/// </summary>
public interface ILanguageRepository : IAsyncReadWriteRepository<int, ILanguage>
{
    /// <summary>
    ///     Gets a language by its ISO code.
    /// </summary>
    /// <param name="isoCode">The ISO code of the language.</param>
    /// <returns>The language if found; otherwise, <c>null</c>.</returns>
    Task<ILanguage?> GetByIsoCodeAsync(string isoCode);

    /// <summary>
    ///     Gets a language identifier from its ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    Task<int?> GetIdByIsoCodeAsync(string? isoCode, bool throwOnNotFound = true);

    /// <summary>
    ///     Gets a language ISO code from its identifier.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    Task<string?> GetIsoCodeByIdAsync(int? id, bool throwOnNotFound = true);

    /// <summary>
    ///     Gets the default language ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    Task<string> GetDefaultIsoCodeAsync();

    /// <summary>
    ///     Gets the default language identifier.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    Task<int?> GetDefaultIdAsync();

    /// <summary>
    ///     Gets multiple language ISO codes from the provided Ids.
    /// </summary>
    /// <param name="ids">The language Ids.</param>
    /// <param name="throwOnNotFound">Indicates whether to throw an exception if the provided Id is not found as a language.</param>
    /// <returns></returns>
    Task<string[]> GetIsoCodesByIdsAsync(ICollection<int> ids, bool throwOnNotFound = true);
}
