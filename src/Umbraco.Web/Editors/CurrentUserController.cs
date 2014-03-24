using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core.Configuration;
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
        /// Returns the configuration for the backoffice user membership provider - used to configure the change password dialog
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> GetMembershipProviderConfig()
        {
            var provider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();            
            return provider.GetConfiguration();
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
                result.AddSuccessNotification(ui.Text("user", "password"), ui.Text("user", "passwordChanged"));
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
