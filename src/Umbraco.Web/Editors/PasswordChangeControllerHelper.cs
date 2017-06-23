using System;
using System.Linq;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models;
using Umbraco.Web.Security;

namespace Umbraco.Web.Editors
{
    internal class PasswordChangeControllerHelper
    {
        
        public static Attempt<PasswordChangedModel> ChangePassword(
            IUser currentUser,
            ChangingPasswordModel data, 
            ModelStateDictionary modelState, 
            MembershipHelper membersHelper)
        {
            var userProvider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();
            
            if (userProvider.RequiresQuestionAndAnswer)
            {
                throw new NotSupportedException("Currently the user editor does not support providers that have RequiresQuestionAndAnswer specified");
            }

            var passwordChangeResult = membersHelper.ChangePassword(currentUser.Username, data, userProvider);
            if (passwordChangeResult.Success == false)
            {
                //it wasn't successful, so add the change error to the model state
                var fieldName = passwordChangeResult.Result.ChangeError.MemberNames.FirstOrDefault() ?? "password";
                modelState.AddModelError(fieldName,
                    passwordChangeResult.Result.ChangeError.ErrorMessage);
            }

            return passwordChangeResult;
        }
    }
}