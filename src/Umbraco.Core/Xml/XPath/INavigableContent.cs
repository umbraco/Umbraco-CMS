namespace Umbraco.Cms.Core.Xml.XPath;

/// <summary>
///     Represents a content that can be navigated via XPath.
/// </summary>
public interface INavigableContent
{
    /// <summary>
    ///     Gets the unique identifier of the navigable content.
    /// </summary>
    /// <remarks>The root node identifier should be <c>-1</c>.</remarks>
    int Id { get; }

    /// <summary>
    ///     Gets the unique identifier of parent of the navigable content.
    /// </summary>
    /// <remarks>
    ///     The top-level content parent identifiers should be <c>-1</c> ie the identifier
    ///     of the root node, whose parent identifier should in turn be <c>-1</c>.
    /// </remarks>
    int ParentId { get; }

    /// <summary>
    ///     Gets the type of the navigable content.
    /// </summary>
    INavigableContentType Type { get; }

    /// <summary>
    ///     Gets the unique identifiers of the children of the navigable content.
    /// </summary>
    IList<int>? ChildIds { get; }

    /// <summary>
    ///     Gets the value of a field of the navigable content for XPath navigation use.
    /// </summary>
    /// <param name="index">The field index.</param>
    /// <returns>The value of the field for XPath navigation use.</returns>
    /// <remarks>
    ///     <para>
    ///         Fields are attributes or elements depending on their relative index value compared
    ///         to source.LastAttributeIndex.
    ///     </para>
    ///     <para>For attributes, the value must be a string.</para>
    ///     <para>
    ///         For elements, the value should an <c>XPathNavigator</c> instance if the field is xml
    ///         and has content (is not empty), <c>null</c> to indicate that the element is empty, or a string
    ///         which can be empty, whitespace... depending on what the data type wants to expose.
    ///     </para>
    /// </remarks>
    object? Value(int index);

    // TODO: implement the following one

    ///// <summary>
    ///// Gets the value of a field of the navigable content, for a specified language.
    ///// </summary>
    ///// <param name="index">The field index.</param>
    ///// <param name="languageKey">The language key.</param>
    ///// <returns>The value of the field for the specified language.</returns>
    ///// <remarks>...</remarks>
    // object Value(int index, string languageKey);
}
