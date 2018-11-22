using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

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

        /// <summary>
        /// This will perform the delete operation for legacy items which include any item that
        /// has functionality included in the ui.xml structure.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage DeleteLegacyItem(string nodeId, string alias, string nodeType)
        {
            //U4-2686 - alias is html encoded, make sure to decode 
            alias = HttpUtility.HtmlDecode(alias);

            //In order to process this request we MUST have an HttpContext available
            var httpContextAttempt = TryGetHttpContext();
            if (httpContextAttempt.Success)
            {
                //this is a hack check based on legacy
                if (nodeType == "memberGroups")
                {
                    LegacyDialogHandler.Delete(httpContextAttempt.Result, UmbracoUser, nodeType, 0, alias);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }

                int id;
                if (int.TryParse(nodeId, out id))
                {
                    LegacyDialogHandler.Delete(httpContextAttempt.Result, UmbracoUser, nodeType, id, alias);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }

                //the way this legacy stuff used to work is that if the node id didn't parse, we would     
                //pass the node id as the alias with an id of zero = sure whatevs.
                LegacyDialogHandler.Delete(httpContextAttempt.Result, UmbracoUser, nodeType, 0, nodeId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            //We must have an HttpContext available for this to work.
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new InvalidOperationException("No HttpContext found in the current request"));
        }

    }
}