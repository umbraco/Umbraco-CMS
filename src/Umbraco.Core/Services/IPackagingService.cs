using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;

namespace Umbraco.Core.Services
{
    public interface IPackagingService : IService
    {
        #region Created Packages

        IEnumerable<PackageDefinition> GetAllCreatedPackages();
        PackageDefinition GetCreatedPackageById(int id);
        void DeleteCreatedPackage(int id);

        /// <summary>
        /// Persists a package definition to storage
        /// </summary>
        /// <returns></returns>
        bool SaveCreatedPackage(PackageDefinition definition);

        /// <summary>
        /// Creates the package file and returns it's physical path
        /// </summary>
        /// <param name="definition"></param>
        string ExportCreatedPackage(PackageDefinition definition);

        #endregion

        #region Importing
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
        /// Imports and saves package xml as <see cref="IDataType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated DataTypeDefinitions</returns>
        IEnumerable<IDataType> ImportDataTypeDefinitions(XElement element, int userId = 0, bool raiseEvents = true);

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
        #endregion

        /// <summary>
        /// This will fetch an Umbraco package file from the package repository and return the relative file path to the downloaded package file
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="umbracoVersion"></param>
        /// <param name="userId">The current user id performing the operation</param>
        /// <returns></returns>
        string FetchPackageFile(Guid packageId, Version umbracoVersion, int userId);
    }
}
