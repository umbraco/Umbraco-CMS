using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
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

        /// <summary>
        /// This will perform the delete operation for legacy items which include any item that
        /// has functionality included in the ui.xml structure.
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage DeleteLegacyItem(string nodeId, string nodeType)
        {
            //TODO: Detect recycle bin node ids and delete permanently!

            //In order to process this request we MUST have an HttpContext available
            var httpContextAttempt = TryGetHttpContext();
            if (httpContextAttempt.Success)
            {
                int id;
                if (int.TryParse(nodeId, out id))
                {
                    LegacyDialogHandler.Delete(httpContextAttempt.Result, UmbracoUser, nodeType, id, "");
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                //We must have an integer id for this to work
                throw new HttpResponseException(HttpStatusCode.PreconditionFailed);
            }
            //We must have an HttpContext available for this to work.
            throw new HttpResponseException(HttpStatusCode.PreconditionFailed);

        }

    }
}