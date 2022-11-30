using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions(Constants.Configuration.ConfigDataTypes)]
public class DataTypesSettings
{
    internal const DataTypeChangeMode StaticDataTypeChangeMode = DataTypeChangeMode.True;

    /// <summary>
    /// Gets or sets a value indicating if data types can be changed after they've been used.
    /// </summary>
    [DefaultValue(StaticDataTypeChangeMode)]
    public DataTypeChangeMode CanBeChanged { get; set; } = StaticDataTypeChangeMode;
}
