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
    /// Gets or sets a value indicating if data types can be changed after they've been used.
    /// </summary>
    [DefaultValue(StaticDataTypeChangeMode)]
    public DataTypeChangeMode CanBeChanged { get; set; } = StaticDataTypeChangeMode;
}
