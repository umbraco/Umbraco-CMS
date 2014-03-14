﻿using System.Collections.Generic;
using System.Xml;
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
        /// <returns>An enumrable list of generated content</returns>
        IEnumerable<IContent> ImportContent(XElement element, int parentId = -1, int userId = 0);

        /// <summary>
        /// Exports an <see cref="IContentType"/> to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="contentType">ContentType to export</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ContentType item.</returns>
        XElement Export(IContentType contentType);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <returns>An enumrable list of generated ContentTypes</returns>
        IEnumerable<IContentType> ImportContentTypes(XElement element, int userId = 0);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="importStructure">Boolean indicating whether or not to import the </param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <returns>An enumrable list of generated ContentTypes</returns>
        IEnumerable<IContentType> ImportContentTypes(XElement element, bool importStructure, int userId = 0);

        /// <summary>
        /// Imports and saves package xml as <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId"></param>
        /// <returns>An enumrable list of generated DataTypeDefinitions</returns>
        IEnumerable<IDataTypeDefinition> ImportDataTypeDefinitions(XElement element, int userId = 0);

        /// <summary>
        /// Imports and saves package xml as <see cref="ITemplate"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional user id</param>
        /// <returns>An enumrable list of generated Templates</returns>
        IEnumerable<ITemplate> ImportTemplates(XElement element, int userId = 0);

        /// <summary>
        /// Imports and saves package xml as <see cref="ITemplate"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional user id</param>
        /// <returns>An enumrable list of generated Laguages</returns>
        IEnumerable<ILanguage> ImportLanguage(XElement element, int userId = 0);
        
        IEnumerable<IFile> ImportStylesheets(XElement element, int userId = 0);
        IEnumerable<IMacro> ImportMacros(XElement xElement, int userId = 0);
        IEnumerable<IDictionaryItem> ImportDictionaryItems(XElement xElement, int userId = 0);
    }
}