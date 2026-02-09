using System.Xml.Linq;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Serializes entities to XML
/// </summary>
public interface IEntityXmlSerializer
{
    /// <summary>
    ///     The XML element name used for document type serialization.
    /// </summary>
    internal const string DocumentTypeElementName = "DocumentType";

    /// <summary>
    ///     The XML element name used for media type serialization.
    /// </summary>
    internal const string MediaTypeElementName = "MediaType";

    /// <summary>
    ///     The XML element name used for member type serialization.
    /// </summary>
    internal const string MemberTypeElementName = "MemberType";

    /// <summary>
    ///     Exports an <see cref="IContent"/> item as an <see cref="XElement"/>.
    /// </summary>
    /// <param name="content">The content item to serialize.</param>
    /// <param name="published">If <c>true</c>, serializes the published version; otherwise serializes the draft version.</param>
    /// <param name="withDescendants">If <c>true</c>, includes all descendant content items in the serialization.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the content item.</returns>
    // TODO: take care of usage! only used for the packager
    XElement Serialize(
        IContent content,
        bool published,
        bool withDescendants = false);

    /// <summary>
    ///     Exports an <see cref="IMedia"/> item as an <see cref="XElement"/>.
    /// </summary>
    /// <param name="media">The media item to serialize.</param>
    /// <param name="withDescendants">If <c>true</c>, includes all descendant media items in the serialization.</param>
    /// <param name="onMediaItemSerialized">Optional callback action invoked after each media item is serialized.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the media item.</returns>
    XElement Serialize(
        IMedia media,
        bool withDescendants = false,
        Action<IMedia, XElement>? onMediaItemSerialized = null);

    /// <summary>
    ///     Exports an <see cref="IMember"/> item as an <see cref="XElement"/>.
    /// </summary>
    /// <param name="member">The member to serialize.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the member.</returns>
    XElement Serialize(IMember member);

    /// <summary>
    ///     Exports a list of Data Types
    /// </summary>
    /// <param name="dataTypeDefinitions">List of data types to export</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IDataTypeDefinition objects</returns>
    XElement Serialize(IEnumerable<IDataType> dataTypeDefinitions);

    /// <summary>
    ///     Exports a single <see cref="IDataType"/> to an <see cref="XElement"/>.
    /// </summary>
    /// <param name="dataType">The data type to serialize.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the data type.</returns>
    XElement Serialize(IDataType dataType);

    /// <summary>
    ///     Exports a list of <see cref="IDictionaryItem" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="dictionaryItem">List of dictionary items to export</param>
    /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IDictionaryItem objects</returns>
    XElement Serialize(IEnumerable<IDictionaryItem> dictionaryItem, bool includeChildren = true);

    /// <summary>
    ///     Exports a single <see cref="IDictionaryItem" /> item to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="dictionaryItem">Dictionary Item to export</param>
    /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IDictionaryItem object</returns>
    XElement Serialize(IDictionaryItem dictionaryItem, bool includeChildren);

    /// <summary>
    ///     Exports an <see cref="IStylesheet"/> to an <see cref="XElement"/>.
    /// </summary>
    /// <param name="stylesheet">The stylesheet to serialize.</param>
    /// <param name="includeProperties">If <c>true</c>, includes the stylesheet's properties in the serialization.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the stylesheet.</returns>
    XElement Serialize(IStylesheet stylesheet, bool includeProperties);

    /// <summary>
    ///     Exports a list of <see cref="ILanguage" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="languages">List of Languages to export</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the ILanguage objects</returns>
    XElement Serialize(IEnumerable<ILanguage> languages);

    /// <summary>
    ///     Exports a single <see cref="ILanguage"/> to an <see cref="XElement"/>.
    /// </summary>
    /// <param name="language">The language to serialize.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the language.</returns>
    XElement Serialize(ILanguage language);

    /// <summary>
    ///     Exports a single <see cref="ITemplate"/> to an <see cref="XElement"/>.
    /// </summary>
    /// <param name="template">The template to serialize.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the template.</returns>
    XElement Serialize(ITemplate template);

    /// <summary>
    ///     Exports a list of <see cref="ITemplate"/> items to XML as an <see cref="XElement"/>.
    /// </summary>
    /// <param name="templates">The collection of templates to serialize.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of all templates.</returns>
    XElement Serialize(IEnumerable<ITemplate> templates);

    /// <summary>
    ///     Exports an <see cref="IMediaType"/> to an <see cref="XElement"/>.
    /// </summary>
    /// <param name="mediaType">The media type to serialize.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the media type.</returns>
    XElement Serialize(IMediaType mediaType);

    /// <summary>
    ///     Exports an <see cref="IMemberType"/> to an <see cref="XElement"/>.
    /// </summary>
    /// <param name="memberType">The member type to serialize.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the member type.</returns>
    XElement Serialize(IMemberType memberType) => throw new NotImplementedException();

    /// <summary>
    ///     Exports an <see cref="IContentType"/> to an <see cref="XElement"/>.
    /// </summary>
    /// <param name="contentType">The content type to serialize.</param>
    /// <returns>An <see cref="XElement"/> containing the XML representation of the content type.</returns>
    XElement Serialize(IContentType contentType);
}
