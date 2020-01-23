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
    }
}
