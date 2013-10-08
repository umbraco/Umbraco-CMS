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

        IMemberType GetMemberType(string alias);
        IMemberType GetMemberType(int id);
    }
}