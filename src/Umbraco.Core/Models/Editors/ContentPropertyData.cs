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

    /// <summary>
    ///     Gets or sets the uploaded files.
    /// </summary>
    public ContentPropertyFile[]? Files { get; set; }
}
