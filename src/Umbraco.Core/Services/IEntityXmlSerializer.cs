using System.Xml.Linq;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Serializes entities to XML
/// </summary>
public interface IEntityXmlSerializer
{
    /// <summary>
    ///     Exports an IContent item as an XElement.
    /// </summary>
    XElement Serialize(
        IContent content,
        bool published,
        bool withDescendants = false) // TODO: take care of usage! only used for the packager
        ;

    /// <summary>
    ///     Exports an IMedia item as an XElement.
    /// </summary>
    XElement Serialize(
        IMedia media,
        bool withDescendants = false,
        Action<IMedia, XElement>? onMediaItemSerialized = null);

    /// <summary>
    ///     Exports an IMember item as an XElement.
    /// </summary>
    XElement Serialize(IMember member);

    /// <summary>
    ///     Exports a list of Data Types
    /// </summary>
    /// <param name="dataTypeDefinitions">List of data types to export</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IDataTypeDefinition objects</returns>
    XElement Serialize(IEnumerable<IDataType> dataTypeDefinitions);

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

    XElement Serialize(IStylesheet stylesheet, bool includeProperties);

    /// <summary>
    ///     Exports a list of <see cref="ILanguage" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="languages">List of Languages to export</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the ILanguage objects</returns>
    XElement Serialize(IEnumerable<ILanguage> languages);

    XElement Serialize(ILanguage language);

    XElement Serialize(ITemplate template);

    /// <summary>
    ///     Exports a list of <see cref="ITemplate" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="templates"></param>
    /// <returns></returns>
    XElement Serialize(IEnumerable<ITemplate> templates);

    XElement Serialize(IMediaType mediaType);

    /// <summary>
    ///     Exports a list of <see cref="IMacro" /> items to xml as an <see cref="XElement" />
    /// </summary>
    /// <param name="macros">Macros to export</param>
    /// <returns><see cref="XElement" /> containing the xml representation of the IMacro objects</returns>
    XElement Serialize(IEnumerable<IMacro> macros);

    XElement Serialize(IMacro macro);

    XElement Serialize(IContentType contentType);
}
