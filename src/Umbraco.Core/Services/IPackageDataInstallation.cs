using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the PackageDataInstallation service, which provides operations for installing package data such as
///     content types, data types, dictionary items, languages, and templates.
/// </summary>
public interface IPackageDataInstallation
{
    /// <summary>
    ///     Installs the data from a compiled package.
    /// </summary>
    /// <param name="compiledPackage">The compiled package containing the data to install.</param>
    /// <param name="userId">The id of the user performing the installation.</param>
    /// <returns>An <see cref="InstallationSummary"/> containing the results of the installation.</returns>
    InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IMediaType"/>.
    /// </summary>
    /// <param name="docTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <returns>An enumerable list of generated <see cref="IMediaType"/>s.</returns>
    IReadOnlyList<IMediaType> ImportMediaTypes(IEnumerable<XElement> docTypeElements, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IMemberType"/>.
    /// </summary>
    /// <param name="docTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <returns>An enumerable list of generated <see cref="IMemberType"/>s.</returns>
    IReadOnlyList<IMemberType> ImportMemberTypes(IEnumerable<XElement> docTypeElements, int userId) => throw new NotImplementedException();

    /// <summary>
    ///     Imports and saves content base items from a compiled package.
    /// </summary>
    /// <typeparam name="TContentBase">The type of content base being imported (e.g., <see cref="IContent"/> or <see cref="IMedia"/>).</typeparam>
    /// <typeparam name="TContentTypeComposition">The type of content type composition.</typeparam>
    /// <param name="docs">The compiled package content base items to import.</param>
    /// <param name="importedDocumentTypes">A dictionary of imported document types keyed by their alias.</param>
    /// <param name="userId">The id of the user performing the import.</param>
    /// <param name="typeService">The service for managing content types.</param>
    /// <param name="service">The service for managing content.</param>
    /// <returns>A read-only list of imported content base items.</returns>
    IReadOnlyList<TContentBase> ImportContentBase<TContentBase, TContentTypeComposition>(
        IEnumerable<CompiledPackageContentBase> docs,
        IDictionary<string, TContentTypeComposition> importedDocumentTypes,
        int userId,
        IContentTypeBaseService<TContentTypeComposition> typeService,
        IContentServiceBase<TContentBase> service)
        where TContentBase : class, IContentBase
        where TContentTypeComposition : IContentTypeComposition;

    /// <summary>
    ///     Imports and saves a single document type from an <see cref="XElement"/>.
    /// </summary>
    /// <param name="docTypeElement">The XML element representing the document type.</param>
    /// <param name="userId">The id of the user performing the import.</param>
    /// <returns>A read-only list of imported <see cref="IContentType"/> objects.</returns>
    IReadOnlyList<IContentType> ImportDocumentType(XElement docTypeElement, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IContentType"/>
    /// </summary>
    /// <param name="docTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <returns>An enumerable list of generated ContentTypes</returns>
    IReadOnlyList<IContentType> ImportDocumentTypes(IEnumerable<XElement> docTypeElements, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IDataType"/>
    /// </summary>
    /// <param name="dataTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the user</param>
    /// <returns>An enumerable list of generated DataTypeDefinitions</returns>
    IReadOnlyList<IDataType> ImportDataTypes(IReadOnlyCollection<XElement> dataTypeElements, int userId);

    /// <summary>
    /// Imports and saves the 'DictionaryItems' part of the package xml as a list of <see cref="IDictionaryItem"/>
    /// </summary>
    /// <param name="dictionaryItemElementList">Xml to import</param>
    /// <param name="userId">The id of the user performing the import.</param>
    /// <returns>An enumerable list of dictionary items</returns>
    IReadOnlyList<IDictionaryItem> ImportDictionaryItems(
        IEnumerable<XElement> dictionaryItemElementList,
        int userId);

    /// <summary>
    ///     Imports a single dictionary item and its children from an <see cref="XElement"/>.
    /// </summary>
    /// <param name="dictionaryItemElement">The XML element representing the dictionary item.</param>
    /// <param name="userId">The id of the user performing the import.</param>
    /// <param name="parentId">The optional parent id for the dictionary item.</param>
    /// <returns>An enumerable collection of imported <see cref="IDictionaryItem"/> objects.</returns>
    IEnumerable<IDictionaryItem> ImportDictionaryItem(XElement dictionaryItemElement, int userId, Guid? parentId);

    /// <summary>
    /// Imports and saves the 'Languages' part of a package xml as a list of <see cref="ILanguage"/>
    /// </summary>
    /// <param name="languageElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation</param>
    /// <returns>An enumerable list of generated languages</returns>
    IReadOnlyList<ILanguage> ImportLanguages(IEnumerable<XElement> languageElements, int userId);

    /// <summary>
    ///     Imports a single template from an <see cref="XElement"/> asynchronously.
    /// </summary>
    /// <param name="templateElement">The XML element representing the template.</param>
    /// <param name="userId">The id of the user performing the import.</param>
    /// <returns>An enumerable collection of imported <see cref="ITemplate"/> objects.</returns>
    Task<IEnumerable<ITemplate>> ImportTemplateAsync(XElement templateElement, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="ITemplate"/>
    /// </summary>
    /// <param name="templateElements">Xml to import</param>
    /// <param name="userId">Optional user id</param>
    /// <returns>An enumerable list of generated Templates</returns>
    Task<IReadOnlyList<ITemplate>> ImportTemplatesAsync(IReadOnlyCollection<XElement> templateElements, int userId);

    /// <summary>
    ///     Gets the content type key from an XML element representing a content type.
    /// </summary>
    /// <param name="contentType">The XML element representing the content type.</param>
    /// <returns>The <see cref="Guid"/> key of the content type.</returns>
    Guid GetContentTypeKey(XElement contentType);

    /// <summary>
    ///     Gets the entity type alias from an XML element.
    /// </summary>
    /// <param name="entityType">The XML element representing the entity type.</param>
    /// <returns>The alias of the entity type, or <c>null</c> if not found.</returns>
    string? GetEntityTypeAlias(XElement entityType);
}
