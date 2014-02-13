using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IMemberTypeService : IService
    { 
        /// <summary>
        /// Gets a list of all available <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        IEnumerable<IMemberType> GetAll(params int[] ids);

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        IMemberType Get(int id);

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        IMemberType Get(string alias);

        /// <summary>
        /// Saves a single <see cref="IMemberType"/> object
        /// </summary>
        /// <param name="memberType"><see cref="IMemberType"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the ContentType</param>
        void Save(IMemberType memberType, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IMemberType"/> objects
        /// </summary>
        /// <param name="memberTypes">Collection of <see cref="IMemberType"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the ContentTypes</param>
        void Save(IEnumerable<IMemberType> memberTypes, int userId = 0);

        /// <summary>
        /// Deletes a single <see cref="IMemberType"/> object
        /// </summary>
        /// <param name="memberType"><see cref="IMemberType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IMemberType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IMemberType"/></remarks>
        /// <param name="userId">Optional Id of the User deleting the ContentType</param>
        void Delete(IMemberType memberType, int userId = 0);

        /// <summary>
        /// Deletes a collection of <see cref="IMemberType"/> objects
        /// </summary>
        /// <param name="memberTypes">Collection of <see cref="IMemberType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IMemberType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IMemberType"/></remarks>
        /// <param name="userId">Optional Id of the User deleting the ContentTypes</param>
        void Delete(IEnumerable<IMemberType> memberTypes, int userId = 0);
    }
}