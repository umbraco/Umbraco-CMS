namespace Umbraco.Cms.Core.Xml.XPath;

/// <summary>
///     Represents the type of a content that can be navigated via XPath.
/// </summary>
public interface INavigableContentType
{
    /// <summary>
    ///     Gets the name of the content type.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets the field types of the content type.
    /// </summary>
    /// <remarks>This includes the attributes and the properties.</remarks>
    INavigableFieldType[] FieldTypes { get; }
}
