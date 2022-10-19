using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IPropertyType : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets of sets the name of the property type.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Gets of sets the alias of the property type.
    /// </summary>
    string Alias { get; set; }

    /// <summary>
    ///     Gets of sets the description of the property type.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the datatype for this property type.
    /// </summary>
    int DataTypeId { get; set; }

    Guid DataTypeKey { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the property editor for this property type.
    /// </summary>
    string PropertyEditorAlias { get; set; }

    /// <summary>
    ///     Gets or sets the database type for storing value for this property type.
    /// </summary>
    ValueStorageType ValueStorageType { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the property group this property type belongs to.
    /// </summary>
    /// <remarks>For generic properties, the value is <c>null</c>.</remarks>
    Lazy<int>? PropertyGroupId { get; set; }

    /// <summary>
    ///     Gets of sets a value indicating whether a value for this property type is required.
    /// </summary>
    bool Mandatory { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the label of this property type should be displayed on top.
    /// </summary>
    bool LabelOnTop { get; set; }

    /// <summary>
    ///     Gets of sets the sort order of the property type.
    /// </summary>
    int SortOrder { get; set; }

    /// <summary>
    ///     Gets or sets the regular expression validating the property values.
    /// </summary>
    string? ValidationRegExp { get; set; }

    bool SupportsPublishing { get; set; }

    /// <summary>
    ///     Gets or sets the content variation of the property type.
    /// </summary>
    ContentVariation Variations { get; set; }

    /// <summary>
    ///     Gets or sets the custom validation message used when a value for this PropertyType is required
    /// </summary>
    string? MandatoryMessage { get; set; }

    /// <summary>
    ///     Gets or sets the custom validation message used when a pattern for this PropertyType must be matched
    /// </summary>
    string? ValidationRegExpMessage { get; set; }

    /// <summary>
    ///     Determines whether the property type supports a combination of culture and segment.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="wildcards">A value indicating whether wildcards are valid.</param>
    bool SupportsVariation(string? culture, string? segment, bool wildcards = false);
}
