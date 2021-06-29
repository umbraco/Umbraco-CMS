using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;


namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Controller to back the User.Resource service, used for fetching user data when already authenticated. user.service is currently used for handling authentication
    /// </summary>
    [PluginController("UmbracoApi")]
    public class CurrentUserController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Returns permissions for all nodes passed in for the current user
        /// </summary>
        /// <param name="nodeIds"></param>
        /// <returns></returns>
        [HttpPost]
        public Dictionary<int, string[]> GetPermissions(int[] nodeIds)
        {
            var permissions = Services.UserService
                .GetPermissions(Security.CurrentUser, nodeIds);

            var permissionsDictionary = new Dictionary<int, string[]>();
            foreach (var nodeId in nodeIds)
            {
                var aggregatePerms = permissions.GetAllPermissions(nodeId).ToArray();
                permissionsDictionary.Add(nodeId, aggregatePerms);
            }

            return permissionsDictionary;
        }

        /// <summary>
        /// Checks a nodes permission for the current user
        /// </summary>
        /// <param name="permissionToCheck"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        [HttpGet]
        public bool HasPermission(string permissionToCheck, int nodeId)
        {
            var p = Services.UserService.GetPermissions(Security.CurrentUser, nodeId).GetAllPermissions();
            if (p.Contains(permissionToCheck.ToString(CultureInfo.InvariantCulture)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Saves a tour status for the current user
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public IEnumerable<UserTourStatus> PostSetUserTour(UserTourStatus status)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            List<UserTourStatus> userTours;
            if (Security.CurrentUser.TourData.IsNullOrWhiteSpace())
            {
                userTours = new List<UserTourStatus> { status };
                Security.CurrentUser.TourData = JsonConvert.SerializeObject(userTours);
                Services.UserService.Save(Security.CurrentUser);
                return userTours;
            }

            userTours = JsonConvert.DeserializeObject<IEnumerable<UserTourStatus>>(Security.CurrentUser.TourData).ToList();
            var found = userTours.FirstOrDefault(x => x.Alias == status.Alias);
            if (found != null)
            {
                //remove it and we'll replace it next
                userTours.Remove(found);
            }
            userTours.Add(status);
            Security.CurrentUser.TourData = JsonConvert.SerializeObject(userTours);
            Services.UserService.Save(Security.CurrentUser);
            return userTours;
        }

        /// <summary>
        /// Returns the user's tours
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserTourStatus> GetUserTours()
        {
            if (Security.CurrentUser.TourData.IsNullOrWhiteSpace())
                return Enumerable.Empty<UserTourStatus>();

            var userTours = JsonConvert.DeserializeObject<IEnumerable<UserTourStatus>>(Security.CurrentUser.TourData);
            return userTours;
        }

        /// <summary>
        /// When a user is invited and they click on the invitation link, they will be partially logged in
        /// where they can set their username/password
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        /// <remarks>
        /// This only works when the user is logged in (partially)
        /// </remarks>
        [WebApi.UmbracoAuthorize(requireApproval: false)]
        [OverrideAuthorization]
        public async Task<UserDetail> PostSetInvitedUserPassword([FromBody]string newPassword)
        {
            var result = await UserManager.AddPasswordAsync(Security.GetUserId().ResultOr(0), newPassword);

            if (result.Succeeded == false)
            {
                //it wasn't successful, so add the change error to the model state, we've name the property alias _umb_password on the form
                // so that is why it is being used here.
                ModelState.AddModelError(
                    "value",
                    string.Join(", ", result.Errors));

                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            //They've successfully set their password, we can now update their user account to be approved
            Security.CurrentUser.IsApproved = true;
            //They've successfully set their password, and will now get fully logged into the back office, so the lastlogindate is set so the backoffice shows they have logged in
            Security.CurrentUser.LastLoginDate = DateTime.UtcNow;
            Services.UserService.Save(Security.CurrentUser);

            //now we can return their full object since they are now really logged into the back office
            var userDisplay = Mapper.Map<UserDetail>(Security.CurrentUser);
            var httpContextAttempt = TryGetHttpContext();
            if (httpContextAttempt.Success)
            {
                //set their remaining seconds
                userDisplay.SecondsUntilTimeout = httpContextAttempt.Result.GetRemainingAuthSeconds();
            }
            return userDisplay;
        }

        [AppendUserModifiedHeader]
        [FileUploadCleanupFilter(false)]
        public async Task<HttpResponseMessage> PostSetAvatar()
        {
            //borrow the logic from the user controller
            return await UsersController.PostSetAvatarInternal(Request, Services.UserService, AppCaches.RuntimeCache, Security.GetUserId().ResultOr(0));
        }

        /// <summary>
        /// Changes the users password
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// If the password is being reset it will return the newly reset password, otherwise will return an empty value
        /// </returns>
        public async Task<ModelWithNotifications<string>> PostChangePassword(ChangingPasswordModel data)
        {
            var passwordChanger = new PasswordChanger(Logger, Services.UserService, UmbracoContext.HttpContext);
            var passwordChangeResult = await passwordChanger.ChangePasswordWithIdentityAsync(Security.CurrentUser, Security.CurrentUser, data, UserManager);

            if (passwordChangeResult.Success)
            {
                var userMgr = this.TryGetOwinContext().Result.GetBackOfficeUserManager();

                //raise the reset event
                // TODO: I don't think this is required anymore since from 7.7 we no longer display the reset password checkbox since that didn't make sense.
                if (data.Reset.HasValue && data.Reset.Value)
                {
                    userMgr.RaisePasswordResetEvent(Security.CurrentUser.Id);
                }

                //even if we weren't resetting this, it is the correct value (null), otherwise if we were resetting then it will contain the new pword
                var result = new ModelWithNotifications<string>(passwordChangeResult.Result.ResetPassword);
                result.AddSuccessNotification(Services.TextService.Localize("user", "password"), Services.TextService.Localize("user", "passwordChanged"));
                return result;
            }

            foreach (var memberName in passwordChangeResult.Result.ChangeError.MemberNames)
            {
                ModelState.AddModelError(memberName, passwordChangeResult.Result.ChangeError.ErrorMessage);
            }

            throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
        }

    }
}
