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
    // TODO KJA: constructor breakage
    public ContentPropertyData(object? value, Guid dataTypeKey)
    {
        Value = value;
        DataTypeKey = dataTypeKey;
    }

    /// <summary>
    ///     The value submitted for the property
    /// </summary>
    public object? Value { get; }

    /// <summary>
    ///     The unique identifier of the data type defining the property type.
    /// </summary>
    public Guid DataTypeKey { get; }

    /// <summary>
    ///     Gets or sets the unique identifier of the content owning the property.
    /// </summary>
    public Guid ContentKey { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the property type.
    /// </summary>
    public Guid PropertyTypeKey { get; set; }
}
