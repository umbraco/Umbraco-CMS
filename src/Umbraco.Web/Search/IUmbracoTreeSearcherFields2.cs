using System.Collections.Generic;

namespace Umbraco.Web.Search
{
    // TODO: Merge this interface to IUmbracoTreeSearcherFields for v9.
    // We should probably make these method make a little more sense when they are combined so have
    // a single method for getting fields to search and fields to load for each category.
    public interface IUmbracoTreeSearcherFields2 : IUmbracoTreeSearcherFields
    {
        /// <summary>
        /// Set of fields for all node types to be loaded
        /// </summary>
        ISet<string> GetBackOfficeFieldsToLoad();

        /// <summary>
        /// Additional set list of fields for Members to be loaded
        /// </summary>
        ISet<string> GetBackOfficeMembersFieldsToLoad();

        /// <summary>
        /// Additional set of fields for Media to be loaded
        /// </summary>
        ISet<string> GetBackOfficeMediaFieldsToLoad();

        /// <summary>
        /// Additional set of fields for Documents to be loaded
        /// </summary>
        ISet<string> GetBackOfficeDocumentFieldsToLoad();
    }
}
