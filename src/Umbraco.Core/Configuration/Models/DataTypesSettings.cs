using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for data types settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigDataTypes)]
public class DataTypesSettings
{
    /// <summary>
    ///     The default value for the <see cref="CanBeChanged" /> setting.
    /// </summary>
    internal const DataTypeChangeMode StaticDataTypeChangeMode = DataTypeChangeMode.True;

    /// <summary>
    ///     The default value for the <see cref="ShowDeprecatedPropertyEditors" /> setting.
    /// </summary>
    internal const bool StaticShowDeprecatedPropertyEditors = false;

    /// <summary>
    /// Gets or sets a value indicating if data types can be changed after they've been used.
    /// </summary>
    [DefaultValue(StaticDataTypeChangeMode)]
    public DataTypeChangeMode CanBeChanged { get; set; } = StaticDataTypeChangeMode;

    /// <summary>
    /// Gets or sets a value indicating whether deprecated property editors are offered when picking the editor for a
    /// data type.
    /// </summary>
    /// <remarks>
    /// A deprecated property editor keeps working for the data types already using it either way. This only governs
    /// whether one can be chosen for a data type that is not already using it.
    /// </remarks>
    [DefaultValue(StaticShowDeprecatedPropertyEditors)]
    public bool ShowDeprecatedPropertyEditors { get; set; } = StaticShowDeprecatedPropertyEditors;
}
