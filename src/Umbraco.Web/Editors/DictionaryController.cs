using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Constants = Umbraco.Core.Constants;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    /// <summary>
    /// The API controller used for editing dictionary items
    /// </summary>
    /// <remarks>
    /// The security for this controller is defined to allow full CRUD access to dictionary if the user has access to either:
    /// Dictionar
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.Dictionary)]
    [EnableOverrideAuthorization]
    public class DictionaryController : BackOfficeNotificationsController
    {
        /// <summary>
        /// Deletes a data type wth a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundDictionary = Services.LocalizationService.GetDictionaryItemById(id);
            if (foundDictionary == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.LocalizationService.Delete(foundDictionary, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
