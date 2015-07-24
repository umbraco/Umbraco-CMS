using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the ContentTypeService, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    public interface IContentTypeService : IService
    {
        /// <summary>
        /// Gets all property type aliases.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllPropertyTypeAliases();

        /// <summary>
        /// Copies a content type as a child under the specified parent if specified (otherwise to the root)
        /// </summary>
        /// <param name="original">
        /// The content type to copy
        /// </param>
        /// <param name="alias">
        /// The new alias of the content type
        /// </param>
        /// <param name="name">
        /// The new name of the content type
        /// </param>
        /// <param name="parentId">
        /// The parent to copy the content type to, default is -1 (root)
        /// </param>
        /// <returns></returns>
        IContentType Copy(IContentType original, string alias, string name, int parentId = -1);

        /// <summary>
        /// Copies a content type as a child under the specified parent if specified (otherwise to the root)
        /// </summary>
        /// <param name="original">
        /// The content type to copy
        /// </param>
        /// <param name="alias">
        /// The new alias of the content type
        /// </param>
        /// <param name="name">
        /// The new name of the content type
        /// </param>
        /// <param name="parent">
        /// The parent to copy the content type to
        /// </param>
        /// <returns></returns>
        IContentType Copy(IContentType original, string alias, string name, IContentType parent);

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        IContentType GetContentType(int id);

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        IContentType GetContentType(string alias);

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Key
        /// </summary>
        /// <param name="id">Alias of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        IContentType GetContentType(Guid id);

        /// <summary>
        /// Gets a list of all available <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        IEnumerable<IContentType> GetAllContentTypes(params int[] ids);

        /// <summary>
        /// Gets a list of all available <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        IEnumerable<IContentType> GetAllContentTypes(IEnumerable<Guid> ids);

        /// <summary>
        /// Gets a list of children for a <see cref="IContentType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        IEnumerable<IContentType> GetContentTypeChildren(int id);

        /// <summary>
        /// Gets a list of children for a <see cref="IContentType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        IEnumerable<IContentType> GetContentTypeChildren(Guid id);

        /// <summary>
        /// Saves a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the ContentType</param>
        void Save(IContentType contentType, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the ContentTypes</param>
        void Save(IEnumerable<IContentType> contentTypes, int userId = 0);

        /// <summary>
        /// Deletes a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/></remarks>
        /// <param name="userId">Optional Id of the User deleting the ContentType</param>
        void Delete(IContentType contentType, int userId = 0);

        /// <summary>
        /// Deletes a collection of <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/></remarks>
        /// <param name="userId">Optional Id of the User deleting the ContentTypes</param>
        void Delete(IEnumerable<IContentType> contentTypes, int userId = 0);
        
        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        IMediaType GetMediaType(int id);

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        IMediaType GetMediaType(string alias);

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        IMediaType GetMediaType(Guid id);

        /// <summary>
        /// Gets a list of all available <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        IEnumerable<IMediaType> GetAllMediaTypes(params int[] ids);

        /// <summary>
        /// Gets a list of all available <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        IEnumerable<IMediaType> GetAllMediaTypes(IEnumerable<Guid> ids);

        /// <summary>
        /// Gets a list of children for a <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        IEnumerable<IMediaType> GetMediaTypeChildren(int id);

        /// <summary>
        /// Gets a list of children for a <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        IEnumerable<IMediaType> GetMediaTypeChildren(Guid id);

        /// <summary>
        /// Saves a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the MediaType</param>
        void Save(IMediaType mediaType, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the MediaTypes</param>
        void Save(IEnumerable<IMediaType> mediaTypes, int userId = 0);

        /// <summary>
        /// Deletes a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        /// <param name="userId">Optional Id of the User deleting the MediaType</param>
        void Delete(IMediaType mediaType, int userId = 0);

        /// <summary>
        /// Deletes a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        /// <param name="userId">Optional Id of the User deleting the MediaTypes</param>
        void Delete(IEnumerable<IMediaType> mediaTypes, int userId = 0);

        /// <summary>
        /// Generates the complete (simplified) XML DTD.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        string GetDtd();

        /// <summary>
        /// Generates the complete XML DTD without the root.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        string GetContentTypesDtd();

        /// <summary>
        /// Checks whether an <see cref="IContentType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>True if the content type has any children otherwise False</returns>
        bool HasChildren(int id);

        /// <summary>
        /// Checks whether an <see cref="IContentType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>True if the content type has any children otherwise False</returns>
        bool HasChildren(Guid id);

        /// <summary>
        /// Checks whether an <see cref="IMediaType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>True if the media type has any children otherwise False</returns>
        bool MediaTypeHasChildren(int id);

        /// <summary>
        /// Checks whether an <see cref="IMediaType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>True if the media type has any children otherwise False</returns>
        bool MediaTypeHasChildren(Guid id);
    }
}