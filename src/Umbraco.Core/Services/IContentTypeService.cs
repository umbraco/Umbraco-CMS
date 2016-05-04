using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Manages <see cref="IContentType"/> objects.
    /// </summary>
    public interface IContentTypeService : IContentTypeServiceBase<IContentType>
    {
        /// <summary>
        /// Gets all property type aliases.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllPropertyTypeAliases();

        /// <summary>
        /// Gets all content type aliases
        /// </summary>
        /// <param name="objectTypes">
        /// If this list is empty, it will return all content type aliases for media, members and content, otherwise
        /// it will only return content type aliases for the object types specified
        /// </param>
        /// <returns></returns>
        IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes);

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
    }
}