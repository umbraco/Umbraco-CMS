using System;
using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
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
        /// Returns the configuration for the backoffice user membership provider - used to configure the change password dialog
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> GetMembershipProviderConfig()
        {
            var provider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();
            return provider.GetConfiguration(Services.UserService); // fixme inject
        }

        /// <summary>
        /// Changes the users password
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// If the password is being reset it will return the newly reset password, otherwise will return an empty value
        /// </returns>
        public ModelWithNotifications<string> PostChangePassword(ChangingPasswordModel data)
        {
            var userProvider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();

            //TODO: WE need to support this! - requires UI updates, etc...
            if (userProvider.RequiresQuestionAndAnswer)
            {
                throw new NotSupportedException("Currently the user editor does not support providers that have RequiresQuestionAndAnswer specified");
            }

            var passwordChangeResult = Members.ChangePassword(Security.CurrentUser.Username, data, userProvider);
            if (passwordChangeResult.Success)
            {
                //even if we weren't resetting this, it is the correct value (null), otherwise if we were resetting then it will contain the new pword
                var result = new ModelWithNotifications<string>(passwordChangeResult.Result.ResetPassword);
                result.AddSuccessNotification(Services.TextService.Localize("user/password"), Services.TextService.Localize("user/passwordChanged"));
                return result;
            }

            //it wasn't successful, so add the change error to the model state, we've name the property alias _umb_password on the form
            // so that is why it is being used here.
            ModelState.AddPropertyError(
                passwordChangeResult.Result.ChangeError,
                string.Format("{0}password", Constants.PropertyEditors.InternalGenericPropertiesPrefix));

            throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
        }

    }
}
