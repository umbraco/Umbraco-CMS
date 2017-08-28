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

    using Umbraco.Web.WebApi;

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

        /// <summary>
        /// Creates a new dictoinairy item
        /// </summary>
        /// <param name="parentId">
        /// The parent id.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Create(int parentId, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return this.Request.CreateNotificationValidationErrorResponse("Key can not be empty;"); // TODO translate
            }

            if (this.Services.LocalizationService.DictionaryItemExists(key))
            {
                return this.Request.CreateNotificationValidationErrorResponse("Key already exists"); // TODO translate
            }

            try
            {
                Guid? parentGuid = null;

                if (parentId > 0)
                {
                    parentGuid = this.Services.LocalizationService.GetDictionaryItemById(parentId).Key;
                }

                var item = this.Services.LocalizationService.CreateDictionaryItemWithIdentity(
                    key,
                    parentGuid,
                    string.Empty);


                return this.Request.CreateResponse(HttpStatusCode.OK, item.Id);
            }
            catch (Exception exception)
            {
                this.Logger.Error(this.GetType(), "Error creating dictionary", exception);
                return this.Request.CreateNotificationValidationErrorResponse("Error creating dictionary item");
            }            
        }
    }
}
