using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface ILanguageRepository : IReadWriteQueryRepository<int, ILanguage>
{
    ILanguage? GetByIsoCode(string isoCode);

    /// <summary>
    ///     Gets a language identifier from its ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    int? GetIdByIsoCode(string? isoCode, bool throwOnNotFound = true);

    /// <summary>
    ///     Gets a language ISO code from its identifier.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    string? GetIsoCodeById(int? id, bool throwOnNotFound = true);

    /// <summary>
    ///     Gets the default language ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    string GetDefaultIsoCode();

    /// <summary>
    ///     Gets the default language identifier.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    int? GetDefaultId();
}
