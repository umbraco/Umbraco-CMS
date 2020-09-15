using System.Collections.Generic;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Used to propagate hardcoded internal Field lists
    /// </summary>
    public interface IUmbracoTreeSearcherFields
    {
        /// <summary>
        /// Propagate list of searchable fields for all node types
        /// </summary>
        IEnumerable<string> GetBackOfficeFields();
        /// <summary>
        /// Propagate list of searchable fields for Members
        /// </summary>
        IEnumerable<string> GetBackOfficeMembersFields();
        /// <summary>
        /// Propagate list of searchable fields for Media
        /// </summary>
        IEnumerable<string> GetBackOfficeMediaFields();
        /// <summary>
        /// Propagate list of searchable fields for Documents
        /// </summary>
        IEnumerable<string> GetBackOfficeDocumentFields();

        /// <summary>
        /// Set of fields for all node types to be loaded
        /// </summary>
        ISet<string> GetBackOfficeFieldsToLoad();
        /// <summary>
        /// Set list of fields for Members to be loaded
        /// </summary>
        ISet<string> GetBackOfficeMembersFieldsToLoad();
        /// <summary>
        /// Set of fields for Media to be loaded
        /// </summary>
        ISet<string> GetBackOfficeMediaFieldsToLoad();

        /// <summary>
        /// Set of fields for Documents to be loaded
        /// </summary>
        ISet<string> GetBackOfficeDocumentFieldsToLoad();
    }
}
