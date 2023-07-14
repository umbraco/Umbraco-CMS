namespace Umbraco.Cms.Core.Xml.XPath;

/// <summary>
///     Represents the type of a content that can be navigated via XPath.
/// </summary>
[Obsolete("The current implementation of XPath is suboptimal and will be removed entirely in a future version. Scheduled for removal in v14")]
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
