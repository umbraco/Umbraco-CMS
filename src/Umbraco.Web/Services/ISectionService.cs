using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Services
{
    public interface ISectionService
    {
        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        IEnumerable<IBackOfficeSection> GetSections();

        /// <summary>
        /// Get the user group's allowed sections
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<IBackOfficeSection> GetAllowedSections(int userId);

        /// <summary>
        /// Gets the application by its alias.
        /// </summary>
        /// <param name="appAlias">The application alias.</param>
        /// <returns></returns>
        IBackOfficeSection GetByAlias(string appAlias);
        
    }
    
}
