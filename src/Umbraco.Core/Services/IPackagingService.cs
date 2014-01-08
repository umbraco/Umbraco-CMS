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
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated ContentTypes</returns>
        IEnumerable<IContentType> ImportContentTypes(XElement element, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves package xml as <see cref="IContentType"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="importStructure">Boolean indicating whether or not to import the </param>
        /// <param name="userId">Optional id of the User performing the operation. Default is zero (admin).</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated ContentTypes</returns>
        IEnumerable<IContentType> ImportContentTypes(XElement element, bool importStructure, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves package xml as <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId"></param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated DataTypeDefinitions</returns>
        IEnumerable<IDataTypeDefinition> ImportDataTypeDefinitions(XElement element, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Imports and saves package xml as <see cref="ITemplate"/>
        /// </summary>
        /// <param name="element">Xml to import</param>
        /// <param name="userId">Optional user id</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns>An enumrable list of generated Templates</returns>
        IEnumerable<ITemplate> ImportTemplates(XElement element, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Exports an <see cref="IContentType"/> to xml as an <see cref="XElement"/>
        /// </summary>
        /// <param name="contentType">ContentType to export</param>
        /// <param name="raiseEvents">Optional parameter indicating whether or not to raise events</param>
        /// <returns><see cref="XElement"/> containing the xml representation of the ContentType item.</returns>
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
    }
}