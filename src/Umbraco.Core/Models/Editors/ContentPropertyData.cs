namespace Umbraco.Cms.Core.Models.Editors;

/// <summary>
///     Represents data that has been submitted to be saved for a content property
/// </summary>
/// <remarks>
///     This object exists because we may need to save additional data for each property, more than just
///     the string representation of the value being submitted. An example of this is uploaded files.
/// </remarks>
public class ContentPropertyData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPropertyData" /> class.
    /// </summary>
    /// <param name="value">The value submitted for the property.</param>
    /// <param name="dataTypeConfiguration">The data type configuration for the property.</param>
    public ContentPropertyData(object? value, object? dataTypeConfiguration)
    {
        Value = value;
        DataTypeConfiguration = dataTypeConfiguration;
    }

    /// <summary>
    ///     The value submitted for the property
    /// </summary>
    public object? Value { get; }

    /// <summary>
    ///     The data type configuration for the property.
    /// </summary>
    public object? DataTypeConfiguration { get; }

    /// <summary>
    ///     Gets or sets the unique identifier of the content owning the property.
    /// </summary>
    public Guid ContentKey { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the property type.
    /// </summary>
    public Guid PropertyTypeKey { get; set; }
}
