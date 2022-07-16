namespace Umbraco.Cms.Core.Xml.XPath;

/// <summary>
///     Represents the type of a field of a content that can be navigated via XPath.
/// </summary>
/// <remarks>A field can be an attribute or a property.</remarks>
public interface INavigableFieldType
{
    /// <summary>
    ///     Gets the name of the field type.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets a method to convert the field value to a string.
    /// </summary>
    /// <remarks>
    ///     This is for built-in properties, ie attributes. User-defined properties have their
    ///     own way to convert their value for XPath.
    /// </remarks>
    Func<object, string>? XmlStringConverter { get; }
}
