using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     Controller to back the User.Resource service, used for fetching user data when already authenticated. user.service
///     is currently used for handling authentication
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class CurrentUserController : UmbracoAuthorizedJsonController
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly ContentSettings _contentSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IPasswordChanger<BackOfficeIdentityUser> _passwordChanger;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IUserDataService _userDataService;
    private readonly IUserService _userService;

    [ActivatorUtilitiesConstructor]
    public CurrentUserController(
        MediaFileManager mediaFileManager,
        IOptionsSnapshot<ContentSettings> contentSettings,
        IHostingEnvironment hostingEnvironment,
        IImageUrlGenerator imageUrlGenerator,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeUserManager backOfficeUserManager,
        ILocalizedTextService localizedTextService,
        AppCaches appCaches,
        IShortStringHelper shortStringHelper,
        IPasswordChanger<BackOfficeIdentityUser> passwordChanger,
        IUserDataService userDataService)
    {
        _mediaFileManager = mediaFileManager;
        _contentSettings = contentSettings.Value;
        _hostingEnvironment = hostingEnvironment;
        _imageUrlGenerator = imageUrlGenerator;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _userService = userService;
        _umbracoMapper = umbracoMapper;
        _backOfficeUserManager = backOfficeUserManager;
        _localizedTextService = localizedTextService;
        _appCaches = appCaches;
        _shortStringHelper = shortStringHelper;
        _passwordChanger = passwordChanger;
        _userDataService = userDataService;
    }

    [Obsolete("This constructor is obsolete and will be removed in v11, use constructor with all values")]
    public CurrentUserController(
        MediaFileManager mediaFileManager,
        IOptions<ContentSettings> contentSettings,
        IHostingEnvironment hostingEnvironment,
        IImageUrlGenerator imageUrlGenerator,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeUserManager backOfficeUserManager,
        ILoggerFactory loggerFactory,
        ILocalizedTextService localizedTextService,
        AppCaches appCaches,
        IShortStringHelper shortStringHelper,
        IPasswordChanger<BackOfficeIdentityUser> passwordChanger) : this(
        mediaFileManager,
        StaticServiceProvider.Instance.GetRequiredService<IOptionsSnapshot<ContentSettings>>(),
        hostingEnvironment,
        imageUrlGenerator,
        backofficeSecurityAccessor,
        userService,
        umbracoMapper,
        backOfficeUserManager,
        localizedTextService,
        appCaches,
        shortStringHelper,
        passwordChanger,
        StaticServiceProvider.Instance.GetRequiredService<IUserDataService>())
    {
    }


    /// <summary>
    ///     Returns permissions for all nodes passed in for the current user
    /// </summary>
    /// <param name="nodeIds"></param>
    /// <returns></returns>
    [HttpPost]
    public Dictionary<int, string[]> GetPermissions(int[] nodeIds)
    {
        EntityPermissionCollection permissions = _userService
            .GetPermissions(_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser, nodeIds);

        var permissionsDictionary = new Dictionary<int, string[]>();
        foreach (var nodeId in nodeIds)
        {
            var aggregatePerms = permissions.GetAllPermissions(nodeId).ToArray();
            permissionsDictionary.Add(nodeId, aggregatePerms);
        }

        return permissionsDictionary;
    }

    /// <summary>
    ///     Checks a nodes permission for the current user
    /// </summary>
    /// <param name="permissionToCheck"></param>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    [HttpGet]
    public bool HasPermission(string permissionToCheck, int nodeId)
    {
        IEnumerable<string> p = _userService
            .GetPermissions(_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser, nodeId).GetAllPermissions();
        if (p.Contains(permissionToCheck.ToString(CultureInfo.InvariantCulture)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Saves a tour status for the current user
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public IEnumerable<UserTourStatus> PostSetUserTour(UserTourStatus? status)
    {
        if (status == null)
        {
            throw new ArgumentNullException(nameof(status));
        }

        List<UserTourStatus>? userTours = null;
        if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.TourData.IsNullOrWhiteSpace() ?? true)
        {
            userTours = new List<UserTourStatus> { status };
            if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser is not null)
            {
                _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.TourData =
                    JsonConvert.SerializeObject(userTours);
                _userService.Save(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser);
            }

            return userTours;
        }

        if (_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.TourData is not null)
        {
            userTours = JsonConvert
                .DeserializeObject<IEnumerable<UserTourStatus>>(_backofficeSecurityAccessor.BackOfficeSecurity
                    .CurrentUser.TourData)?.ToList();
            UserTourStatus? found = userTours?.FirstOrDefault(x => x.Alias == status.Alias);
            if (found != null)
            {
                //remove it and we'll replace it next
                userTours?.Remove(found);
            }

            userTours?.Add(status);
        }

        _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.TourData = JsonConvert.SerializeObject(userTours);
        _userService.Save(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser);
        return userTours ?? Enumerable.Empty<UserTourStatus>();
    }

    /// <summary>
    ///     Returns the user's tours
    /// </summary>
    /// <returns></returns>
    public IEnumerable<UserTourStatus>? GetUserTours()
    {
        if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.TourData.IsNullOrWhiteSpace() ?? true)
        {
            return Enumerable.Empty<UserTourStatus>();
        }

        IEnumerable<UserTourStatus>? userTours =
            JsonConvert.DeserializeObject<IEnumerable<UserTourStatus>>(_backofficeSecurityAccessor.BackOfficeSecurity
                .CurrentUser.TourData!);
        return userTours ?? Enumerable.Empty<UserTourStatus>();
    }

    public IEnumerable<UserData> GetUserData() => _userDataService.GetUserData();

    /// <summary>
    ///     When a user is invited and they click on the invitation link, they will be partially logged in
    ///     where they can set their username/password
    /// </summary>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This only works when the user is logged in (partially)
    /// </remarks>
    [AllowAnonymous]
    public async Task<ActionResult<UserDetail?>> PostSetInvitedUserPassword([FromBody] string newPassword)
    {
        BackOfficeIdentityUser? user = await _backOfficeUserManager.FindByIdAsync(_backofficeSecurityAccessor
            .BackOfficeSecurity?.GetUserId().ResultOr(0).ToString());
        if (user == null)
        {
            throw new InvalidOperationException("Could not find user");
        }

        IdentityResult result = await _backOfficeUserManager.AddPasswordAsync(user, newPassword);

        if (result.Succeeded == false)
        {
            //it wasn't successful, so add the change error to the model state, we've name the property alias _umb_password on the form
            // so that is why it is being used here.
            ModelState.AddModelError("value", result.Errors.ToErrorMessage());

            return ValidationProblem(ModelState);
        }

        if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser is not null)
        {
            //They've successfully set their password, we can now update their user account to be approved
            _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsApproved = true;
            //They've successfully set their password, and will now get fully logged into the back office, so the lastlogindate is set so the backoffice shows they have logged in
            _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.LastLoginDate = DateTime.UtcNow;
            _userService.Save(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser);
        }


        //now we can return their full object since they are now really logged into the back office
        UserDetail? userDisplay =
            _umbracoMapper.Map<UserDetail>(_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser);

        if (userDisplay is not null)
        {
            userDisplay.SecondsUntilTimeout = HttpContext.User.GetRemainingAuthSeconds();
        }

        return userDisplay;
    }

    [AppendUserModifiedHeader]
    public IActionResult PostSetAvatar(IList<IFormFile> file)
    {
        Attempt<int>? userId = _backofficeSecurityAccessor.BackOfficeSecurity?.GetUserId();
        var result = userId?.ResultOr(0);
        //borrow the logic from the user controller
        return UsersController.PostSetAvatarInternal(
            file,
            _userService,
            _appCaches.RuntimeCache,
            _mediaFileManager,
            _shortStringHelper,
            _contentSettings,
            _hostingEnvironment,
            _imageUrlGenerator,
            _backofficeSecurityAccessor.BackOfficeSecurity?.GetUserId().Result ?? 0);
    }

    /// <summary>
    ///     Changes the users password
    /// </summary>
    /// <param name="changingPasswordModel">The changing password model</param>
    /// <returns>
    ///     If the password is being reset it will return the newly reset password, otherwise will return an empty value
    /// </returns>
    public async Task<ActionResult<ModelWithNotifications<string?>>?> PostChangePassword(
        ChangingPasswordModel changingPasswordModel)
    {
        IUser? currentUser = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (currentUser is null)
        {
            return null;
        }

        changingPasswordModel.Id = currentUser.Id;

        // all current users have access to reset/manually change their password

        Attempt<PasswordChangedModel?> passwordChangeResult =
            await _passwordChanger.ChangePasswordWithIdentityAsync(changingPasswordModel, _backOfficeUserManager);

        if (passwordChangeResult.Success)
        {
            // even if we weren't resetting this, it is the correct value (null), otherwise if we were resetting then it will contain the new pword
            var result = new ModelWithNotifications<string?>(passwordChangeResult.Result?.ResetPassword);
            result.AddSuccessNotification(_localizedTextService.Localize("user", "password"), _localizedTextService.Localize("user", "passwordChanged"));
            return result;
        }

        if (passwordChangeResult.Result?.ChangeError?.MemberNames is not null)
        {
            foreach (var memberName in passwordChangeResult.Result.ChangeError.MemberNames)
            {
                ModelState.AddModelError(memberName, passwordChangeResult.Result.ChangeError.ErrorMessage ?? string.Empty);
            }
        }

        return ValidationProblem(ModelState);
    }

    // TODO: Why is this necessary? This inherits from UmbracoAuthorizedApiController
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [ValidateAngularAntiForgeryToken]
    public async Task<Dictionary<string, string>> GetCurrentUserLinkedLogins()
    {
        BackOfficeIdentityUser identityUser = await _backOfficeUserManager.FindByIdAsync(_backofficeSecurityAccessor
            .BackOfficeSecurity?.GetUserId().ResultOr(0).ToString(CultureInfo.InvariantCulture));

        // deduplicate in case there are duplicates (there shouldn't be now since we have a unique constraint on the external logins
        // but there didn't used to be)
        var result = new Dictionary<string, string>();
        foreach (IIdentityUserLogin l in identityUser.Logins)
        {
            result[l.LoginProvider] = l.ProviderKey;
        }

        return result;
    }
}
