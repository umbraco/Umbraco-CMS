using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a data editor.
/// </summary>
/// <remarks>This is the base interface for parameter and property editors.</remarks>
public interface IDataEditor : IDiscoverable
{
    /// <summary>
    ///     Gets the alias of the editor.
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets a value indicating whether the editor supports read-only mode.
    /// </summary>
    bool SupportsReadOnly => false;

    /// <summary>
    ///     Gets a value indicating whether the editor supports configurable elements.
    /// </summary>
    bool SupportsConfigurableElements => false;

    /// <summary>
    ///     Gets a value indicating whether the editor is deprecated.
    /// </summary>
    /// <remarks>Deprecated editors are supported but not proposed in the UI.</remarks>
    bool IsDeprecated { get; }

    /// <summary>
    ///     Gets the configuration for the value editor.
    /// </summary>
    IDictionary<string, object>? DefaultConfiguration { get; }

    /// <summary>
    ///     Gets the index value factory for the editor.
    /// </summary>
    IPropertyIndexValueFactory PropertyIndexValueFactory { get; }

    /// <summary>
    ///     Gets a value editor.
    /// </summary>
    IDataValueEditor GetValueEditor(); // TODO: should be configured?!

    /// <summary>
    ///     Gets a configured value editor.
    /// </summary>
    IDataValueEditor GetValueEditor(object? configurationObject);

    /// <summary>
    ///     Gets an editor to edit the value editor configuration.
    /// </summary>
    /// <remarks>
    ///     <para>Is expected to throw if the editor does not support being configured, e.g. for most parameter editors.</para>
    /// </remarks>
    IConfigurationEditor GetConfigurationEditor();

    /// <summary>
    ///     Determines if the value editor needs to perform <see cref="MergePartialPropertyValueForCulture"/> for a given property type.
    /// </summary>
    bool CanMergePartialPropertyValues(IPropertyType propertyType) => false;

    /// <summary>
    ///     Partially merges a source property value into a target property value for a given culture.
    /// </summary>
    /// <param name="sourceValue">The source property value.</param>
    /// <param name="targetValue">The target property value.</param>
    /// <param name="culture">The culture (or null for invariant).</param>
    /// <returns>The result of the merge operation.</returns>
    object? MergePartialPropertyValueForCulture(object? sourceValue, object? targetValue, string? culture) => sourceValue;

    /// <summary>
    ///     Merges variant and invariant property values.
    /// </summary>
    /// <param name="sourceValue">The source property value.</param>
    /// <param name="targetValue">The target property value.</param>
    /// <param name="canUpdateInvariantData">A value indicating whether invariant data can be updated.</param>
    /// <param name="allowedCultures">The set of allowed cultures.</param>
    /// <returns>The result of the merge operation.</returns>
    object? MergeVariantInvariantPropertyValue(
        object? sourceValue,
        object? targetValue,
        bool canUpdateInvariantData,
        HashSet<string> allowedCultures) => sourceValue;

    /// <summary>
    ///     Determines the specific cultures that contain an actual content change within an otherwise
    ///     culture-invariant property's value, for editors that support partial per-culture publishing
    ///     (see <see cref="CanMergePartialPropertyValues"/>).
    /// </summary>
    /// <param name="sourceValue">The source (edited) property value.</param>
    /// <param name="targetValue">The target (published) property value.</param>
    /// <param name="defaultCulture">
    ///     The default culture to attribute a change to when it cannot be tied to one specific culture
    ///     (e.g. a genuinely invariant nested value, or a purely structural change).
    /// </param>
    /// <returns>
    ///     The set of cultures containing an actual edit. Empty if the editor is unable to determine this,
    ///     in which case the caller should fall back to flagging <paramref name="defaultCulture"/> as edited.
    /// </returns>
    IEnumerable<string> GetChangedCulturesForPartialPropertyValues(object? sourceValue, object? targetValue, string defaultCulture) => [];

    /// <summary>
    ///     Determines whether an edited and a published property value represent the same content, so that
    ///     publishing the edited value would not change what is published.
    /// </summary>
    /// <param name="editedValue">The edited property value.</param>
    /// <param name="publishedValue">The published property value.</param>
    /// <returns><c>true</c> if the values represent the same content; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     The values are equivalent when they are equal. Editors whose stored format can represent the same content
    ///     in more than one way may compare the values by their content instead, for example regardless of the order
    ///     of nested values.
    /// </remarks>
    // TODO (V18): Remove the default implementation.
    bool ArePropertyValuesEquivalent(object? editedValue, object? publishedValue) => object.Equals(editedValue, publishedValue);
}
