using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IPackagingService : IService
    {
        /// <summary>
        /// Imports and saves package xml as <see cref="IContent"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="parentId">Optional parent Id for the content being imported</param>
        /// <param name="userId">Optional Id of the user performing the import</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated content</returns>
        IEnumerable<IContent> ImportContent(XElement element, int parentId = -1, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin)</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated ContentTypes</returns>
        IEnumerable<IContentType> ImportContentTypes(XElement element, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="importStructure">Boolean indicating whether or not to import the </param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin)</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated ContentTypes</returns>
        IEnumerable<IContentType> ImportContentTypes(XElement element, bool importStructure, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves package xml as <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated DataTypeDefinitions</returns>
        IEnumerable<IDataTypeDefinition> ImportDataTypeDefinitions(XElement element, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves the 'DictionaryItems' part of the package xml as a list of <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="dictionaryItemElementList">Xml to import</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumerable list of dictionary items</returns>
        IEnumerable<IDictionaryItem> ImportDictionaryItems(XElement dictionaryItemElementList, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves the 'Languages' part of a package xml as a list of <see cref="ILanguage"/>
        /// </summary>
        /// <param name="languageElementList">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin)</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumerable list of generated languages</returns>
        IEnumerable<ILanguage> ImportLanguages(XElement languageElementList, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves the 'Macros' part of a package xml as a list of <see cref="IMacro"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns></returns>
        IEnumerable<IMacro> ImportMacros(XElement element, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves package xml as <see cref="ITemplate"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin)</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated Templates</returns>
        IEnumerable<ITemplate> ImportTemplates(XElement element, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Exports an <see cref="IContentType"/> to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="contentType">ContentType to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ContentType item</returns>
        XElement Export(IContentType contentType, bool raiseEvents = true);

        /// <summary>
        /// Exports an <see cref="IContent"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="content">Content to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Content object</returns>
        XElement Export(IContent content, bool deep = false, bool raiseEvents = true);

        /// <summary>
        /// Exports an <see cref="IMedia"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="media">Media to export</param>
        /// <param name="deep">Optional parameter indicating whether to include descendents</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Media object</returns>
        XElement Export(IMedia media, bool deep = false, bool raiseEvents = true);

        /// <summary>
        /// Exports a list of <see cref="ILanguage"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="languages">List of Languages to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Language object</returns>
        XElement Export(IEnumerable<ILanguage> languages, bool raiseEvents = true);

        /// <summary>
        /// Exports a single <see cref="ILanguage"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="language">Language to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the Language object</returns>
        XElement Export(ILanguage language, bool raiseEvents = true);
        
        /// <summary>
        /// Exports a list of <see cref="IDictionaryItem"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dictionaryItem">List of dictionary items to export</param>
        /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDictionaryItem objects</returns>
        XElement Export(IEnumerable<IDictionaryItem> dictionaryItem, bool includeChildren = true, bool raiseEvents = true);

        /// <summary>
        /// Exports a single <see cref="IDictionaryItem"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="dictionaryItem">Dictionary Item to export</param>
        /// <param name="includeChildren">Optional boolean indicating whether or not to include children</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDictionaryItem object</returns>
        XElement Export(IDictionaryItem dictionaryItem, bool includeChildren, bool raiseEvents = true);

        /// <summary>
        /// Exports a list of Data Types
        /// </summary>
        /// <param name="dataTypeDefinitions">List of data types to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDataTypeDefinition objects</returns>
        XElement Export(IEnumerable<IDataTypeDefinition> dataTypeDefinitions, bool raiseEvents = true);

        /// <summary>
        /// Exports a single Data Type
        /// </summary>
        /// <param name="dataTypeDefinition">Data type to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IDataTypeDefinition object</returns>
        XElement Export(IDataTypeDefinition dataTypeDefinition, bool raiseEvents = true);

        /// <summary>
        /// Exports a list of <see cref="ITemplate"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="templates">List of Templates to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ITemplate objects</returns>
        XElement Export(IEnumerable<ITemplate> templates, bool raiseEvents = true);

        /// <summary>
        /// Exports a single <see cref="ITemplate"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="template">Template to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ITemplate object</returns>
        XElement Export(ITemplate template, bool raiseEvents = true);

        /// <summary>
        /// Exports a list of <see cref="IMacro"/> items to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="macros">Macros to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IMacro objects</returns>
        XElement Export(IEnumerable<IMacro> macros, bool raiseEvents = true);

        /// <summary>
        /// Exports a single <see cref="IMacro"/> item to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="macro">Macro to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the IMacro object</returns>
        XElement Export(IMacro macro, bool raiseEvents = true);
    }
}