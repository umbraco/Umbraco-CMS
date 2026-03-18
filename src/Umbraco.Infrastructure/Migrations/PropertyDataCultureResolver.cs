using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Resolves the culture ISO code for a property data row during migrations,
/// handling the case where the property type varies by culture but the referenced
/// language no longer exists.
/// </summary>
[Obsolete("Only used by obsolete V15/V18 migrations. Scheduled for removal in Umbraco 19.")]
internal static class PropertyDataCultureResolver
{
    /// <summary>
    /// Log message template used when a property data row references a language that no longer exists.
    /// </summary>
    internal const string OrphanedLanguageWarningTemplate =
        "    - property data with id: {propertyDataId} references a language that does not exist - language id: {languageId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})";

    /// <summary>
    /// Represents the result of resolving a culture for a property data row.
    /// </summary>
    internal readonly record struct CultureResolutionResult
    {
        /// <summary>
        /// The resolved culture ISO code, or null if invariant.
        /// </summary>
        public string? Culture { get; init; }

        /// <summary>
        /// True if the row should be skipped because it references a deleted language.
        /// </summary>
        public bool ShouldSkip { get; init; }

        /// <summary>
        /// The language ID that was not found (only set when <see cref="ShouldSkip"/> is true).
        /// </summary>
        public int? OrphanedLanguageId { get; init; }
    }

    /// <summary>
    /// Resolves the culture for a property data row, detecting orphaned language references.
    /// </summary>
    /// <param name="propertyType">The property type (may vary by culture via composition).</param>
    /// <param name="languageId">The language ID from the property data row (null for invariant data).</param>
    /// <param name="languagesById">Lookup of all known languages by ID.</param>
    /// <returns>
    /// A result indicating the resolved culture and whether the row should be skipped.
    /// When <paramref name="languageId"/> is null and the property type varies by culture,
    /// this is legitimate invariant data (e.g. from a content type using a culture-varying
    /// composition) and should NOT be skipped.
    /// </returns>
    internal static CultureResolutionResult ResolveCulture(
        IPropertyType propertyType,
        int? languageId,
        IDictionary<int, ILanguage> languagesById)
    {
        // NOTE: some old property data DTOs can have variance defined, even if the property type no longer varies
        string? culture = propertyType.VariesByCulture()
                          && languageId.HasValue
                          && languagesById.TryGetValue(languageId.Value, out ILanguage? language)
            ? language!.IsoCode
            : null;

        // If culture is null, the property type varies by culture, AND the DTO has a non-null
        // language ID, then the language has been deleted. This is an error scenario we can only
        // log; in all likelihood this is an old property version and won't cause runtime issues.
        //
        // If languageId is NULL, this is legitimate invariant data on a content type that uses a
        // culture-varying composition — we must NOT skip it.
        if (culture is null && propertyType.VariesByCulture() && languageId.HasValue)
        {
            return new CultureResolutionResult
            {
                Culture = null,
                ShouldSkip = true,
                OrphanedLanguageId = languageId.Value,
            };
        }

        return new CultureResolutionResult
        {
            Culture = culture,
            ShouldSkip = false,
        };
    }
}
