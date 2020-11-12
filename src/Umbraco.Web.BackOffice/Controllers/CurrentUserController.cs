using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Mapping;
using Umbraco.Core.Media;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// Controller to back the User.Resource service, used for fetching user data when already authenticated. user.service is currently used for handling authentication
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    public class CurrentUserController : UmbracoAuthorizedJsonController
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly ContentSettings _contentSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IUserService _userService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IBackOfficeUserManager _backOfficeUserManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly AppCaches _appCaches;
        private readonly IShortStringHelper _shortStringHelper;

        public CurrentUserController(
            IMediaFileSystem mediaFileSystem,
            IOptions<ContentSettings> contentSettings,
            IHostingEnvironment hostingEnvironment,
            IImageUrlGenerator imageUrlGenerator,
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            IUserService userService,
            UmbracoMapper umbracoMapper,
            IBackOfficeUserManager backOfficeUserManager,
            ILoggerFactory loggerFactory,
            ILocalizedTextService localizedTextService,
            AppCaches appCaches,
            IShortStringHelper shortStringHelper)
        {
            _mediaFileSystem = mediaFileSystem;
            _contentSettings = contentSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _imageUrlGenerator = imageUrlGenerator;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _userService = userService;
            _umbracoMapper = umbracoMapper;
            _backOfficeUserManager = backOfficeUserManager;
            _loggerFactory = loggerFactory;
            _localizedTextService = localizedTextService;
            _appCaches = appCaches;
            _shortStringHelper = shortStringHelper;
        }


        /// <summary>
        /// Returns permissions for all nodes passed in for the current user
        /// </summary>
        /// <param name="nodeIds"></param>
        /// <returns></returns>
        [HttpPost]
        public Dictionary<int, string[]> GetPermissions(int[] nodeIds)
        {
            var permissions = _userService
                .GetPermissions(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser, nodeIds);

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
            var p = _userService.GetPermissions(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser, nodeId).GetAllPermissions();
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
            if (_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.TourData.IsNullOrWhiteSpace())
            {
                userTours = new List<UserTourStatus> { status };
                _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.TourData = JsonConvert.SerializeObject(userTours);
                _userService.Save(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser);
                return userTours;
            }

            userTours = JsonConvert.DeserializeObject<IEnumerable<UserTourStatus>>(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.TourData).ToList();
            var found = userTours.FirstOrDefault(x => x.Alias == status.Alias);
            if (found != null)
            {
                //remove it and we'll replace it next
                userTours.Remove(found);
            }
            userTours.Add(status);
            _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.TourData = JsonConvert.SerializeObject(userTours);
            _userService.Save(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser);
            return userTours;
        }

        /// <summary>
        /// Returns the user's tours
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserTourStatus> GetUserTours()
        {
            if (_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.TourData.IsNullOrWhiteSpace())
                return Enumerable.Empty<UserTourStatus>();

            var userTours = JsonConvert.DeserializeObject<IEnumerable<UserTourStatus>>(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.TourData);
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
        [UmbracoBackOfficeAuthorize(redirectToUmbracoLogin: false, requireApproval : true)]
        public async Task<UserDetail> PostSetInvitedUserPassword([FromBody]string newPassword)
        {
            var user = await _backOfficeUserManager.FindByIdAsync(_backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(0).ToString());
            if (user == null) throw new InvalidOperationException("Could not find user");

            var result = await _backOfficeUserManager.AddPasswordAsync(user, newPassword);

            if (result.Succeeded == false)
            {
                //it wasn't successful, so add the change error to the model state, we've name the property alias _umb_password on the form
                // so that is why it is being used here.
                ModelState.AddModelError("value", result.Errors.ToErrorMessage());

                throw HttpResponseException.CreateValidationErrorResponse(ModelState);
            }

            //They've successfully set their password, we can now update their user account to be approved
            _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.IsApproved = true;
            //They've successfully set their password, and will now get fully logged into the back office, so the lastlogindate is set so the backoffice shows they have logged in
            _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.LastLoginDate = DateTime.UtcNow;
            _userService.Save(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser);

            //now we can return their full object since they are now really logged into the back office
            var userDisplay = _umbracoMapper.Map<UserDetail>(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser);

            userDisplay.SecondsUntilTimeout = HttpContext.User.GetRemainingAuthSeconds();
            return userDisplay;
        }

        [AppendUserModifiedHeader]
        public async Task<IActionResult> PostSetAvatar(IList<IFormFile> files)
        {
            //borrow the logic from the user controller
            return await UsersController.PostSetAvatarInternal(files, _userService, _appCaches.RuntimeCache,  _mediaFileSystem, _shortStringHelper, _contentSettings, _hostingEnvironment, _imageUrlGenerator, _backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(0));
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
            var passwordChanger = new PasswordChanger(_loggerFactory.CreateLogger<PasswordChanger>());
            var passwordChangeResult = await passwordChanger.ChangePasswordWithIdentityAsync(_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser, _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser, data, _backOfficeUserManager);

            if (passwordChangeResult.Success)
            {
                //even if we weren't resetting this, it is the correct value (null), otherwise if we were resetting then it will contain the new pword
                var result = new ModelWithNotifications<string>(passwordChangeResult.Result.ResetPassword);
                result.AddSuccessNotification(_localizedTextService.Localize("user/password"), _localizedTextService.Localize("user/passwordChanged"));
                return result;
            }

            foreach (var memberName in passwordChangeResult.Result.ChangeError.MemberNames)
            {
                ModelState.AddModelError(memberName, passwordChangeResult.Result.ChangeError.ErrorMessage);
            }

            throw HttpResponseException.CreateValidationErrorResponse(ModelState);
        }

        [UmbracoBackOfficeAuthorize]
        [ValidateAngularAntiForgeryToken]
        public async Task<Dictionary<string, string>> GetCurrentUserLinkedLogins()
        {
            var identityUser = await _backOfficeUserManager.FindByIdAsync(_backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(0).ToString());
            return identityUser.Logins.ToDictionary(x => x.LoginProvider, x => x.ProviderKey);
        }
    }
}
