using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebServices;

namespace Umbraco.Web.Editors
{
    //NOTE: The reason why this controller exists here with this name is because in v7, all of the editor controllers exist here,
    // therefore, these methods will still be exactly the same in v7 and no URLs will need to change!

    [PluginController("UmbracoApi")]
    public class MediaController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Remove the xml formatter... only support JSON!
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(global::System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            controllerContext.Configuration.Formatters.Remove(controllerContext.Configuration.Formatters.XmlFormatter);
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="sorted"></param>
        /// <returns></returns>
        public HttpResponseMessage PostSort(ContentSortOrder sorted)
        {
            if (sorted == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            //if there's nothing to sort just return ok
            if (sorted.IdSortOrder.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            if (!Security.UserHasAppAccess(global::Umbraco.Core.Constants.Applications.Media, UmbracoUser))
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User has no access to this application");
            }

            var mediaService = base.ApplicationContext.Services.MediaService;
            var sortedMedia = new List<IMedia>();
            try
            {
                sortedMedia.AddRange(sorted.IdSortOrder.Select(mediaService.GetById));

                // Save Media with new sort order and update content xml in db accordingly
                if (!mediaService.Sort(sortedMedia))
                {
                    LogHelper.Warn<MediaController>("Media sorting failed, this was probably caused by an event being cancelled");
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Media sorting failed, this was probably caused by an event being cancelled");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MediaController>("Could not update media sort order", ex);
                throw;
            }
        }


    }
}
