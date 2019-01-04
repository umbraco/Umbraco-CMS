using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for editing dictionary items
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.Macros)]
    public class MacrosController : BackOfficeNotificationsController
    {
        /// <summary>
        /// Creates a new macro
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Request
                    .CreateNotificationValidationErrorResponse("Name can not be empty;");

            var alias = name.ToSafeAlias();

            var existingMacro = this.Services.MacroService.GetByAlias(alias);

            if (existingMacro != null)
            {
                return Request.CreateNotificationValidationErrorResponse("Macro with this name already exists");
            }

            try
            {
                var macro = new Macro { Alias = alias, Name = name };

                this.Services.MacroService.Save(macro, this.Security.CurrentUser.Id);

                return Request.CreateResponse(HttpStatusCode.OK, macro.Id);
            }
            catch (Exception exception)
            {
                this.Logger.Error<MacrosController>(exception, "Error creating macro");
                return Request.CreateNotificationValidationErrorResponse("Error creating dictionary item");
            }                      
        }
    }
}
