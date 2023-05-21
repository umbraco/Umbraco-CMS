namespace Umbraco.Cms.Core.Xml.XPath;

/// <summary>
///     Represents a source of content that can be navigated via XPath.
/// </summary>
public interface INavigableSource
{
    /// <summary>
    ///     Gets the index of the last attribute in the fields collections.
    /// </summary>
    int LastAttributeIndex { get; }

    /// <summary>
    ///     Gets the content at the root of the source.
    /// </summary>
    /// <remarks>
    ///     That content should have unique identifier <c>-1</c> and should not be gettable,
    ///     ie Get(-1) should return null. Its <c>ParentId</c> should be <c>-1</c>. It should provide
    ///     values for the attribute fields.
    /// </remarks>
    INavigableContent Root { get; }

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>The content identified by the unique identifier, or null.</returns>
    /// <remarks>When <c>id</c> is <c>-1</c> (root content) implementations should return <c>null</c>.</remarks>
    INavigableContent? Get(int id);
}
