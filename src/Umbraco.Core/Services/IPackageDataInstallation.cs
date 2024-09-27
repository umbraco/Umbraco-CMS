using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
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

    IReadOnlyList<TContentBase> ImportContentBase<TContentBase, TContentTypeComposition>(
        IEnumerable<CompiledPackageContentBase> docs,
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
    /// <param name="userId"></param>
    /// <returns>An enumerable list of dictionary items</returns>
    IReadOnlyList<IDictionaryItem> ImportDictionaryItems(
        IEnumerable<XElement> dictionaryItemElementList,
        int userId);

    IEnumerable<IDictionaryItem> ImportDictionaryItem(XElement dictionaryItemElement, int userId, Guid? parentId);

    /// <summary>
    /// Imports and saves the 'Languages' part of a package xml as a list of <see cref="ILanguage"/>
    /// </summary>
    /// <param name="languageElements">Xml to import</param>
    /// <param name="userId">Optional id of the User performing the operation</param>
    /// <returns>An enumerable list of generated languages</returns>
    IReadOnlyList<ILanguage> ImportLanguages(IEnumerable<XElement> languageElements, int userId);

    [Obsolete("Use Async version instead, Scheduled to be removed in v17")]
    IEnumerable<ITemplate> ImportTemplate(XElement templateElement, int userId);

    Task<IEnumerable<ITemplate>> ImportTemplateAsync(XElement templateElement, int userId) => Task.FromResult(ImportTemplate(templateElement, userId));

    /// <summary>
    /// Imports and saves package xml as <see cref="ITemplate"/>
    /// </summary>
    /// <param name="templateElements">Xml to import</param>
    /// <param name="userId">Optional user id</param>
    /// <returns>An enumerable list of generated Templates</returns>
    [Obsolete("Use Async version instead, Scheduled to be removed in v17")]
    IReadOnlyList<ITemplate> ImportTemplates(IReadOnlyCollection<XElement> templateElements, int userId);

    /// <summary>
    /// Imports and saves package xml as <see cref="ITemplate"/>
    /// </summary>
    /// <param name="templateElements">Xml to import</param>
    /// <param name="userId">Optional user id</param>
    /// <returns>An enumerable list of generated Templates</returns>
    Task<IReadOnlyList<ITemplate>> ImportTemplatesAsync(IReadOnlyCollection<XElement> templateElements, int userId) => Task.FromResult(ImportTemplates(templateElements, userId));

    Guid GetContentTypeKey(XElement contentType);

    string? GetEntityTypeAlias(XElement entityType);
}
