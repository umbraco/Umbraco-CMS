using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using Umbraco.Web.WebApi;
using umbraco;
using legacyUser = umbraco.BusinessLogic.User;
using System.Net.Http;
using System.Collections.Specialized;
using Constants = Umbraco.Core.Constants;


namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Controller to back the User.Resource service, used for fetching user data when already authenticated. user.service is currently used for handling authentication
    /// </summary>
    [PluginController("UmbracoApi")]
    public class CurrentUserController : UmbracoAuthorizedJsonController
    {
        
        /// <summary>
        /// Changes the users password
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// If the password is being reset it will return the newly reset password, otherwise will return an empty value
        /// </returns>
        public ModelWithNotifications<string> PostChangePassword(ChangingPasswordModel data)
        {
            var passwordChangeResult = PasswordChangeControllerHelper.ChangePassword(Security.CurrentUser, data, ModelState, Members);
            
            if (passwordChangeResult.Success)
            {
                //even if we weren't resetting this, it is the correct value (null), otherwise if we were resetting then it will contain the new pword
                var result = new ModelWithNotifications<string>(passwordChangeResult.Result.ResetPassword);
                result.AddSuccessNotification(Services.TextService.Localize("user/password"), Services.TextService.Localize("user/passwordChanged"));
                return result;
            }

            throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
        }

    }
}
