using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Resolves the culture ISO code for a property data row during migrations,
/// handling the case where the property type varies by culture but the referenced
/// language no longer exists.
/// </summary>
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
        // NOTE: old property data rows may still have languageId populated even if the property type no longer varies
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

    /// <summary>
    /// Creates a <see cref="Property"/> suitable for migration value roundtripping.
    /// </summary>
    /// <remarks>
    /// When a property type varies by culture (e.g. inherited from a composition) but the data
    /// is invariant (null culture), <see cref="Property.SetValue"/> rejects the null culture.
    /// This method handles that case by deep-cloning the property type and setting its
    /// <see cref="IPropertyType.Variations"/> to <see cref="ContentVariation.Nothing"/>,
    /// which is safe because the data genuinely is invariant.
    /// </remarks>
    internal static Property CreateMigrationProperty(
        IPropertyType propertyType,
        object? value,
        string? culture,
        string? segment)
    {
        IPropertyType effectivePropertyType = propertyType;

        if (culture is null && propertyType.VariesByCulture())
        {
            // Invariant data on a culture-varying property type (composition scenario).
            // Clone to avoid mutating shared state — important for parallel migration execution.
            effectivePropertyType = (IPropertyType)propertyType.DeepClone();
            effectivePropertyType.Variations = ContentVariation.Nothing;
        }

        var property = new Property(effectivePropertyType);
        property.SetValue(value, culture, segment);
        return property;
    }
}
