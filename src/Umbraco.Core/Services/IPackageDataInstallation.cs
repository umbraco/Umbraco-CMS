using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Services;

public interface IPackageDataInstallation
{
    InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IContentType"/>
    /// </summary>
    /// <param name="docTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <returns>An enumerable list of generated ContentTypes</returns>
    IReadOnlyList<IMediaType> ImportMediaTypes(IEnumerable<XElement> docTypeElements, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IContentType"/>
    /// </summary>
    /// <param name="docTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <param name="entityContainersInstalled">Collection of entity containers installed by the package to be populated with those created in installing data types.</param>
    /// <returns>An enumerable list of generated ContentTypes</returns>
    IReadOnlyList<IMediaType> ImportMediaTypes(IEnumerable<XElement> docTypeElements, int userId,
        out IEnumerable<EntityContainer> entityContainersInstalled);

    IReadOnlyList<TContentBase> ImportContentBase<TContentBase, TContentTypeComposition>(
        IEnumerable<CompiledPackageContentBase> docs,
        IDictionary<string, TContentTypeComposition> importedDocumentTypes,
        int userId,
        IContentTypeBaseService<TContentTypeComposition> typeService,
        IContentServiceBase<TContentBase> service)
        where TContentBase : class, IContentBase
        where TContentTypeComposition : IContentTypeComposition;

    /// <summary>
    /// Imports and saves package xml as <see cref="IContent"/>
    /// </summary>
    /// <param name="roots">The root contents to import from</param>
    /// <param name="typeService">The content type base service</param>
    /// <param name="parentId">Optional parent Id for the content being imported</param>
    /// <param name="importedDocumentTypes">A dictionary of already imported document types (basically used as a cache)</param>
    /// <param name="userId">Optional Id of the user performing the import</param>
    /// <param name="service">The content service base</param>
    /// <returns>An enumerable list of generated content</returns>
    IEnumerable<TContentBase> ImportContentBase<TContentBase, TContentTypeComposition>(
        IEnumerable<XElement> roots,
        int parentId,
        IDictionary<string, TContentTypeComposition> importedDocumentTypes,
        int userId,
        IContentTypeBaseService<TContentTypeComposition> typeService,
        IContentServiceBase<TContentBase> service)
        where TContentBase : class, IContentBase
        where TContentTypeComposition : IContentTypeComposition;

    IReadOnlyList<IContentType> ImportDocumentType(XElement docTypeElement, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IContentType"/>
    /// </summary>
    /// <param name="docTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <returns>An enumerable list of generated ContentTypes</returns>
    IReadOnlyList<IContentType> ImportDocumentTypes(IEnumerable<XElement> docTypeElements, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IContentType"/>
    /// </summary>
    /// <param name="docTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <param name="entityContainersInstalled">Collection of entity containers installed by the package to be populated with those created in installing data types.</param>
    /// <returns>An enumerable list of generated ContentTypes</returns>
    IReadOnlyList<IContentType> ImportDocumentTypes(IEnumerable<XElement> docTypeElements, int userId,
        out IEnumerable<EntityContainer> entityContainersInstalled);

    /// <summary>
    /// Imports and saves package xml as <see cref="IContentType"/>
    /// </summary>
    /// <param name="unsortedDocumentTypes">Xml to import</param>
    /// <param name="importStructure">Boolean indicating whether or not to import the </param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <param name="service">The content type service.</param>
    /// <returns>An enumerable list of generated ContentTypes</returns>
    IReadOnlyList<T> ImportDocumentTypes<T>(IReadOnlyCollection<XElement> unsortedDocumentTypes, bool importStructure, int userId, IContentTypeBaseService<T> service)
        where T : class, IContentTypeComposition;

    /// <summary>
    /// Imports and saves package xml as <see cref="IContentType"/>
    /// </summary>
    /// <param name="unsortedDocumentTypes">Xml to import</param>
    /// <param name="importStructure">Boolean indicating whether or not to import the </param>
    /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
    /// <param name="service">The content type service</param>
    /// <param name="entityContainersInstalled">Collection of entity containers installed by the package to be populated with those created in installing data types.</param>
    /// <returns>An enumerable list of generated ContentTypes</returns>
    IReadOnlyList<T> ImportDocumentTypes<T>(
        IReadOnlyCollection<XElement> unsortedDocumentTypes,
        bool importStructure, int userId,
        IContentTypeBaseService<T> service,
        out IEnumerable<EntityContainer> entityContainersInstalled)
        where T : class, IContentTypeComposition;

    /// <summary>
    /// Imports and saves package xml as <see cref="IDataType"/>
    /// </summary>
    /// <param name="dataTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the user</param>
    /// <returns>An enumerable list of generated DataTypeDefinitions</returns>
    IReadOnlyList<IDataType> ImportDataTypes(IReadOnlyCollection<XElement> dataTypeElements, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="IDataType"/>
    /// </summary>
    /// <param name="dataTypeElements">Xml to import</param>
    /// <param name="userId">Optional id of the user</param>
    /// <param name="entityContainersInstalled">Collection of entity containers installed by the package to be populated with those created in installing data types.</param>
    /// <returns>An enumerable list of generated DataTypeDefinitions</returns>
    IReadOnlyList<IDataType> ImportDataTypes(IReadOnlyCollection<XElement> dataTypeElements, int userId,
        out IEnumerable<EntityContainer> entityContainersInstalled);

    /// <summary>
    /// Imports and saves the 'DictionaryItems' part of the package xml as a list of <see cref="IDictionaryItem"/>
    /// </summary>
    /// <param name="dictionaryItemElementList">Xml to import</param>
    /// <param name="userId"></param>
    /// <returns>An enumerable list of dictionary items</returns>
    IReadOnlyList<IDictionaryItem> ImportDictionaryItems(IEnumerable<XElement> dictionaryItemElementList,
        int userId);

    IEnumerable<IDictionaryItem> ImportDictionaryItem(XElement dictionaryItemElement, int userId, Guid? parentId);

    /// <summary>
    /// Imports and saves the 'Languages' part of a package xml as a list of <see cref="ILanguage"/>
    /// </summary>
    /// <param name="languageElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation</param>
    /// <returns>An enumerable list of generated languages</returns>
    IReadOnlyList<ILanguage> ImportLanguages(IEnumerable<XElement> languageElements, int userId);

    IReadOnlyList<IScript> ImportScripts(IEnumerable<XElement> scriptElements, int userId);
    IReadOnlyList<IPartialView> ImportPartialViews(IEnumerable<XElement> partialViewElements, int userId);
    IReadOnlyList<IFile> ImportStylesheets(IEnumerable<XElement> stylesheetElements, int userId);
    IEnumerable<ITemplate> ImportTemplate(XElement templateElement, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="ITemplate"/>
    /// </summary>
    /// <param name="templateElements">Xml to import</param>
    /// <param name="userId">Optional user id</param>
    /// <returns>An enumerable list of generated Templates</returns>
    IReadOnlyList<ITemplate> ImportTemplates(IReadOnlyCollection<XElement> templateElements, int userId);

    Guid GetContentTypeKey(XElement contentType);
}
