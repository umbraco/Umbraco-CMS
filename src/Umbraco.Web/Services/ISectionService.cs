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

        ///// <summary>
        ///// Creates a new applcation if no application with the specified alias is found.
        ///// </summary>
        ///// <param name="name">The application name.</param>
        ///// <param name="alias">The application alias.</param>
        ///// <param name="icon">The application icon, which has to be located in umbraco/images/tray folder.</param>
        //void MakeNew(string name, string alias, string icon);

        ///// <summary>
        ///// Makes the new.
        ///// </summary>
        ///// <param name="name">The name.</param>
        ///// <param name="alias">The alias.</param>
        ///// <param name="icon">The icon.</param>
        ///// <param name="sortOrder">The sort order.</param>
        //void MakeNew(string name, string alias, string icon, int sortOrder);

        ///// <summary>
        ///// Deletes the section
        ///// </summary>
        //void DeleteSection(Section section);
    }

    /// <summary>
    /// Purely used to allow a service context to create the default services
    /// </summary>
    internal class EmptySectionService : ISectionService
    {
        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        public IEnumerable<IBackOfficeSection> GetSections()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get the user's allowed sections
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<IBackOfficeSection> GetAllowedSections(int userId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the application by its alias.
        /// </summary>
        /// <param name="appAlias">The application alias.</param>
        /// <returns></returns>
        public IBackOfficeSection GetByAlias(string appAlias)
        {
            throw new System.NotImplementedException();
        }

        ///// <summary>
        ///// Creates a new applcation if no application with the specified alias is found.
        ///// </summary>
        ///// <param name="name">The application name.</param>
        ///// <param name="alias">The application alias.</param>
        ///// <param name="icon">The application icon, which has to be located in umbraco/images/tray folder.</param>
        //public void MakeNew(string name, string alias, string icon)
        //{
        //    throw new System.NotImplementedException();
        //}

        ///// <summary>
        ///// Makes the new.
        ///// </summary>
        ///// <param name="name">The name.</param>
        ///// <param name="alias">The alias.</param>
        ///// <param name="icon">The icon.</param>
        ///// <param name="sortOrder">The sort order.</param>
        //public void MakeNew(string name, string alias, string icon, int sortOrder)
        //{
        //    throw new System.NotImplementedException();
        //}

        ///// <summary>
        ///// Deletes the section
        ///// </summary>
        //public void DeleteSection(IBackOfficeSection section)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}
