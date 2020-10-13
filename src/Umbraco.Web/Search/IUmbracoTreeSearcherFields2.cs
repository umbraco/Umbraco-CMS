using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Search
{
    public interface IUmbracoTreeSearcherFields2 : IUmbracoTreeSearcherFields
    {
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
