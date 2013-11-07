using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    internal interface IMemberTypeService : IService
    {
        /// <summary>
        /// Gets a list of all available <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        IEnumerable<IMemberType> GetAllMemberTypes(params int[] ids);

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        IMemberType GetMemberType(int id);

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        IMemberType GetMemberType(string alias);
    }
}