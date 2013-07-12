using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for dealing with legacy content
    /// </summary>    
    [PluginController("UmbracoApi")]
    [ValidationFilter]
    public class LegacyController : UmbracoAuthorizedJsonController
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public LegacyController()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        internal LegacyController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        ///// <summary>
        ///// This will perform the delete operation for legacy items which include any item that
        ///// has functionality included in the ui.xml structure.
        ///// </summary>
        ///// <returns></returns>
        //public HttpResponseMessage DeleteLegacyItem(string nodeId, string nodeType)
        //{
            
        //}

    }
}