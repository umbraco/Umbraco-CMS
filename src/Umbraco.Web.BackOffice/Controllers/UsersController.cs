using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.BackOffice.ActionResults;
using Umbraco.Cms.Web.BackOffice.Extensions;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.BackOffice.ModelBinders;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessUsers)]
    [PrefixlessBodyModelValidator]
    [IsCurrentUserModelFilter]
    public class UsersController : UmbracoAuthorizedJsonController
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly ContentSettings _contentSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISqlContext _sqlContext;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly SecuritySettings _securitySettings;
        private readonly IEmailSender _emailSender;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly AppCaches _appCaches;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IUserService _userService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly GlobalSettings _globalSettings;
        private readonly IBackOfficeUserManager _userManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly LinkGenerator _linkGenerator;
        private readonly IBackOfficeExternalLoginProviders _externalLogins;
        private readonly UserEditorAuthorizationHelper _userEditorAuthorizationHelper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IMediaFileSystem mediaFileSystem,
            IOptions<ContentSettings> contentSettings,
            IHostingEnvironment hostingEnvironment,
            ISqlContext sqlContext,
            IImageUrlGenerator imageUrlGenerator,
            IOptions<SecuritySettings> securitySettings,
            IEmailSender emailSender,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            AppCaches appCaches,
            IShortStringHelper shortStringHelper,
            IUserService userService,
            ILocalizedTextService localizedTextService,
            UmbracoMapper umbracoMapper,
            IOptions<GlobalSettings> globalSettings,
            IBackOfficeUserManager backOfficeUserManager,
            ILoggerFactory loggerFactory,
            LinkGenerator linkGenerator,
            IBackOfficeExternalLoginProviders externalLogins,
            UserEditorAuthorizationHelper userEditorAuthorizationHelper)
        {
            _mediaFileSystem = mediaFileSystem;
            _contentSettings = contentSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _sqlContext = sqlContext;
            _imageUrlGenerator = imageUrlGenerator;
            _securitySettings = securitySettings.Value;
            _emailSender = emailSender;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _appCaches = appCaches;
            _shortStringHelper = shortStringHelper;
            _userService = userService;
            _localizedTextService = localizedTextService;
            _umbracoMapper = umbracoMapper;
            _globalSettings = globalSettings.Value;
            _userManager = backOfficeUserManager;
            _loggerFactory = loggerFactory;
            _linkGenerator = linkGenerator;
            _externalLogins = externalLogins;
            _userEditorAuthorizationHelper = userEditorAuthorizationHelper;
            _logger = _loggerFactory.CreateLogger<UsersController>();
        }

        /// <summary>
        /// Returns a list of the sizes of gravatar URLs for the user or null if the gravatar server cannot be reached
        /// </summary>
        /// <returns></returns>
        public ActionResult<string[]> GetCurrentUserAvatarUrls()
        {
            var urls = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileSystem, _imageUrlGenerator);
            if (urls == null)
                return new ValidationErrorResult("Could not access Gravatar endpoint");

            return urls;
        }

        [AppendUserModifiedHeader("id")]
        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public IActionResult PostSetAvatar(int id, IList<IFormFile> file)
        {
            return PostSetAvatarInternal(file, _userService, _appCaches.RuntimeCache, _mediaFileSystem, _shortStringHelper, _contentSettings, _hostingEnvironment, _imageUrlGenerator, id);
        }

        internal static IActionResult PostSetAvatarInternal(IList<IFormFile> files, IUserService userService, IAppCache cache, IMediaFileSystem mediaFileSystem, IShortStringHelper shortStringHelper, ContentSettings contentSettings, IHostingEnvironment hostingEnvironment, IImageUrlGenerator imageUrlGenerator, int id)
        {
            if (files is null)
            {
                return new UnsupportedMediaTypeResult();
            }

            var root = hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
            //ensure it exists
            Directory.CreateDirectory(root);

            //must have a file
            if (files.Count == 0)
            {
                return new NotFoundResult();
            }

            var user = userService.GetUserById(id);
            if (user == null)
                return new NotFoundResult();

            if (files.Count > 1)
                return new ValidationErrorResult("The request was not formatted correctly, only one file can be attached to the request");

            //get the file info
            var file = files.First();
            var fileName = file.FileName.Trim(new[] { '\"' }).TrimEnd();
            var safeFileName = fileName.ToSafeFileName(shortStringHelper);
            var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

            if (contentSettings.DisallowedUploadFiles.Contains(ext) == false)
            {
                //generate a path of known data, we don't want this path to be guessable
                user.Avatar = "UserAvatars/" + (user.Id + safeFileName).GenerateHash<SHA1>() + "." + ext;

                using (var fs = file.OpenReadStream())
                {
                    mediaFileSystem.AddFile(user.Avatar, fs, true);
                }

                userService.Save(user);
            }

            return new OkObjectResult(user.GetUserAvatarUrls(cache, mediaFileSystem, imageUrlGenerator));
        }

        [AppendUserModifiedHeader("id")]
        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public ActionResult<string[]> PostClearAvatar(int id)
        {
            var found = _userService.GetUserById(id);
            if (found == null)
                return NotFound();

            var filePath = found.Avatar;

            //if the filePath is already null it will mean that the user doesn't have a custom avatar and their gravatar is currently
            //being used (if they have one). This means they want to remove their gravatar too which we can do by setting a special value
            //for the avatar.
            if (filePath.IsNullOrWhiteSpace() == false)
            {
                found.Avatar = null;
            }
            else
            {
                //set a special value to indicate to not have any avatar
                found.Avatar = "none";
            }

            _userService.Save(found);

            if (filePath.IsNullOrWhiteSpace() == false)
            {
                if (_mediaFileSystem.FileExists(filePath))
                    _mediaFileSystem.DeleteFile(filePath);
            }

            return found.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileSystem, _imageUrlGenerator);
        }

        /// <summary>
        /// Gets a user by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public ActionResult<UserDisplay> GetById(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            var result = _umbracoMapper.Map<IUser, UserDisplay>(user);
            return result;
        }

        /// <summary>
        /// Get users by integer ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public ActionResult<IEnumerable<UserDisplay>> GetByIds([FromJsonPath]int[] ids)
        {
            if (ids == null)
            {
                return NotFound();
            }

            if (ids.Length == 0)
                return Enumerable.Empty<UserDisplay>().ToList();

            var users = _userService.GetUsersById(ids);
            if (users == null)
            {
                return NotFound();
            }

            var result = _umbracoMapper.MapEnumerable<IUser, UserDisplay>(users);
            return result;
        }

        /// <summary>
        /// Returns a paged users collection
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="userGroups"></param>
        /// <param name="userStates"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public PagedUserResult GetPagedUsers(
            int pageNumber = 1,
            int pageSize = 10,
            string orderBy = "username",
            Direction orderDirection = Direction.Ascending,
            [FromQuery]string[] userGroups = null,
            [FromQuery]UserState[] userStates = null,
            string filter = "")
        {
            //following the same principle we had in previous versions, we would only show admins to admins, see
            // https://github.com/umbraco/Umbraco-CMS/blob/dev-v7/src/Umbraco.Web/umbraco.presentation/umbraco/Trees/loadUsers.cs#L91
            // so to do that here, we'll need to check if this current user is an admin and if not we should exclude all user who are
            // also admins

            var hideDisabledUsers = _securitySettings.HideDisabledUsersInBackOffice;
            var excludeUserGroups = new string[0];
            var isAdmin = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsAdmin();
            if (isAdmin == false)
            {
                //this user is not an admin so in that case we need to exclude all admin users
                excludeUserGroups = new[] {Constants.Security.AdminGroupAlias};
            }

            var filterQuery = _sqlContext.Query<IUser>();

            if (!_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsSuper())
            {
                // only super can see super - but don't use IsSuper, cannot be mapped to SQL
                //filterQuery.Where(x => !x.IsSuper());
                filterQuery.Where(x => x.Id != Constants.Security.SuperUserId);
            }

            if (filter.IsNullOrWhiteSpace() == false)
            {
                filterQuery.Where(x => x.Name.Contains(filter) || x.Username.Contains(filter));
            }

            if (hideDisabledUsers)
            {
                if (userStates == null || userStates.Any() == false)
                {
                    userStates = new[] { UserState.Active, UserState.Invited, UserState.LockedOut, UserState.Inactive };
                }
            }

            long pageIndex = pageNumber - 1;
            long total;
            var result = _userService.GetAll(pageIndex, pageSize, out total, orderBy, orderDirection, userStates, userGroups, excludeUserGroups, filterQuery);

            var paged = new PagedUserResult(total, pageNumber, pageSize)
            {
                Items = _umbracoMapper.MapEnumerable<IUser, UserBasic>(result),
                UserStates = _userService.GetUserStates()
            };

            return paged;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="userSave"></param>
        /// <returns></returns>
        public async Task<ActionResult<UserDisplay>> PostCreateUser(UserInvite userSave)
        {
            if (userSave == null) throw new ArgumentNullException("userSave");

            if (ModelState.IsValid == false)
            {
                return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));
            }

            if (_securitySettings.UsernameIsEmail)
            {
                //ensure they are the same if we're using it
                userSave.Username = userSave.Email;
            }
            else
            {
                //first validate the username if were showing it
                CheckUniqueUsername(userSave.Username, null);
            }
            CheckUniqueEmail(userSave.Email, null);

            if (ModelState.IsValid == false)
            {
                return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));
            }

            //Perform authorization here to see if the current user can actually save this user with the info being requested
            var canSaveUser = _userEditorAuthorizationHelper.IsAuthorized(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, null, null, null, userSave.UserGroups);
            if (canSaveUser == false)
            {
                return Unauthorized(canSaveUser.Result);
            }

            //we want to create the user with the UserManager, this ensures the 'empty' (special) password
            //format is applied without us having to duplicate that logic
            var identityUser = BackOfficeIdentityUser.CreateNew(_globalSettings, userSave.Username, userSave.Email, _globalSettings.DefaultUILanguage);
            identityUser.Name = userSave.Name;

            var created = await _userManager.CreateAsync(identityUser);
            if (created.Succeeded == false)
            {
                return ValidationErrorResult.CreateNotificationValidationErrorResult(created.Errors.ToErrorMessage());
            }

            string resetPassword;
            var password = _userManager.GeneratePassword();

            var result = await _userManager.AddPasswordAsync(identityUser, password);
            if (result.Succeeded == false)
            {
                return ValidationErrorResult.CreateNotificationValidationErrorResult(created.Errors.ToErrorMessage());
            }

            resetPassword = password;

            //now re-look the user back up which will now exist
            var user = _userService.GetByEmail(userSave.Email);

            //map the save info over onto the user
            user = _umbracoMapper.Map(userSave, user);

            //since the back office user is creating this user, they will be set to approved
            user.IsApproved = true;

            _userService.Save(user);

            var display = _umbracoMapper.Map<UserDisplay>(user);
            display.ResetPasswordValue = resetPassword;
            return display;
        }

        /// <summary>
        /// Invites a user
        /// </summary>
        /// <param name="userSave"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will email the user an invite and generate a token that will be validated in the email
        /// </remarks>
        public async Task<ActionResult<UserDisplay>> PostInviteUser(UserInvite userSave)
        {
            if (userSave == null) throw new ArgumentNullException("userSave");

            if (userSave.Message.IsNullOrWhiteSpace())
                ModelState.AddModelError("Message", "Message cannot be empty");

            IUser user;
            if (_securitySettings.UsernameIsEmail)
            {
                //ensure it's the same
                userSave.Username = userSave.Email;
            }
            else
            {
                //first validate the username if we're showing it
                var userResult = CheckUniqueUsername(userSave.Username, u => u.LastLoginDate != default || u.EmailConfirmedDate.HasValue);
                if (!(userResult.Result is null))
                {
                    return userResult.Result;
                }

                user = userResult.Value;
            }
            user = CheckUniqueEmail(userSave.Email, u => u.LastLoginDate != default || u.EmailConfirmedDate.HasValue);

            if (ModelState.IsValid == false)
            {
                return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));
            }

            if (!EmailSender.CanSendRequiredEmail(_globalSettings) && !_userManager.HasSendingUserInviteEventHandler)
            {
                return new ValidationErrorResult("No Email server is configured");
            }

            //Perform authorization here to see if the current user can actually save this user with the info being requested
            var canSaveUser = _userEditorAuthorizationHelper.IsAuthorized(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, user, null, null, userSave.UserGroups);
            if (canSaveUser == false)
            {
                return new ValidationErrorResult(canSaveUser.Result, StatusCodes.Status401Unauthorized);
            }

            if (user == null)
            {
                //we want to create the user with the UserManager, this ensures the 'empty' (special) password
                //format is applied without us having to duplicate that logic
                var identityUser = BackOfficeIdentityUser.CreateNew(_globalSettings, userSave.Username, userSave.Email, _globalSettings.DefaultUILanguage);
                identityUser.Name = userSave.Name;

                var created = await _userManager.CreateAsync(identityUser);
                if (created.Succeeded == false)
                {
                    return ValidationErrorResult.CreateNotificationValidationErrorResult(created.Errors.ToErrorMessage());
                }

                //now re-look the user back up
                user = _userService.GetByEmail(userSave.Email);
            }

            //map the save info over onto the user
            user = _umbracoMapper.Map(userSave, user);

            //ensure the invited date is set
            user.InvitedDate = DateTime.Now;

            //Save the updated user (which will process the user groups too)
            _userService.Save(user);
            var display = _umbracoMapper.Map<UserDisplay>(user);

            UserInviteEventArgs inviteArgs;

            try
            {
                inviteArgs = _userManager.RaiseSendingUserInvite(User, userSave, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in a custom event handler while inviting the user");
                return ValidationErrorResult.CreateNotificationValidationErrorResult($"An error occurred inviting the user (check logs for more info): {ex.Message}");
            }

            // If the event is handled then no need to send the email
            if (inviteArgs.InviteHandled)
            {
                // if no user result was created then map the minimum args manually for the UI
                if (!inviteArgs.ShowUserResult)
                {
                    display = new UserDisplay
                    {
                        Name = userSave.Name,
                        Email = userSave.Email,
                        Username = userSave.Username
                    };
                }
            }
            else
            {
                //send the email

                await SendUserInviteEmailAsync(display, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Name, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Email, user, userSave.Message);

            }

            display.AddSuccessNotification(_localizedTextService.Localize("speechBubbles/resendInviteHeader"), _localizedTextService.Localize("speechBubbles/resendInviteSuccess", new[] { user.Name }));
            return display;
        }

        private IUser CheckUniqueEmail(string email, Func<IUser, bool> extraCheck)
        {
            var user = _userService.GetByEmail(email);
            if (user != null && (extraCheck == null || extraCheck(user)))
            {
                ModelState.AddModelError("Email", "A user with the email already exists");
            }
            return user;
        }

        private ActionResult<IUser> CheckUniqueUsername(string username, Func<IUser, bool> extraCheck)
        {
            var user = _userService.GetByUsername(username);
            if (user != null && (extraCheck == null || extraCheck(user)))
            {
                ModelState.AddModelError(
                    _securitySettings.UsernameIsEmail ? "Email" : "Username",
                    "A user with the username already exists");
                return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));
            }

            return new ActionResult<IUser>(user);
        }

        private async Task SendUserInviteEmailAsync(UserBasic userDisplay, string from, string fromEmail, IUser to, string message)
        {
            var user = await _userManager.FindByIdAsync(((int) userDisplay.Id).ToString());
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var inviteToken = string.Format("{0}{1}{2}",
                (int)userDisplay.Id,
                WebUtility.UrlEncode("|"),
                token.ToUrlBase64());

            // Get an mvc helper to get the URL
            var action = _linkGenerator.GetPathByAction(
                nameof(BackOfficeController.VerifyInvite),
                ControllerExtensions.GetControllerName<BackOfficeController>(),
                new
                {
                    area = Constants.Web.Mvc.BackOfficeArea,
                    invite = inviteToken
                });

            // Construct full URL using configured application URL (which will fall back to request)
            var applicationUri = _hostingEnvironment.ApplicationMainUrl;
            var inviteUri = new Uri(applicationUri, action);

            var emailSubject = _localizedTextService.Localize("user/inviteEmailCopySubject",
                //Ensure the culture of the found user is used for the email!
                UmbracoUserExtensions.GetUserCulture(to.Language, _localizedTextService, _globalSettings));
            var emailBody = _localizedTextService.Localize("user/inviteEmailCopyFormat",
                //Ensure the culture of the found user is used for the email!
                UmbracoUserExtensions.GetUserCulture(to.Language, _localizedTextService, _globalSettings),
                new[] { userDisplay.Name, from, message, inviteUri.ToString(), fromEmail });

            var mailMessage = new EmailMessage(fromEmail, to.Email, emailSubject, emailBody, true);

            await _emailSender.SendAsync(mailMessage);
        }

        /// <summary>
        /// Saves a user
        /// </summary>
        /// <param name="userSave"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        public ActionResult<UserDisplay> PostSaveUser(UserSave userSave)
        {
            if (userSave == null) throw new ArgumentNullException(nameof(userSave));

            if (ModelState.IsValid == false)
            {
                return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));
            }

            var intId = userSave.Id.TryConvertTo<int>();
            if (intId.Success == false)
                return NotFound();


            var found = _userService.GetUserById(intId.Result);
            if (found == null)
                return NotFound();

            //Perform authorization here to see if the current user can actually save this user with the info being requested
            var canSaveUser = _userEditorAuthorizationHelper.IsAuthorized(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, found, userSave.StartContentIds, userSave.StartMediaIds, userSave.UserGroups);
            if (canSaveUser == false)
            {
                return Unauthorized(canSaveUser.Result);
            }

            var hasErrors = false;

            // we need to check if there's any Deny Local login providers present, if so we need to ensure that the user's email address cannot be changed
            var hasDenyLocalLogin = _externalLogins.HasDenyLocalLogin();
            if (hasDenyLocalLogin)
            {
                userSave.Email = found.Email; // it cannot change, this would only happen if people are mucking around with the request
            }

            var existing = _userService.GetByEmail(userSave.Email);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Email", "A user with the email already exists");
                hasErrors = true;
            }
            existing = _userService.GetByUsername(userSave.Username);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Username", "A user with the username already exists");
                hasErrors = true;
            }
            // going forward we prefer to align usernames with email, so we should cross-check to make sure
            // the email or username isn't somehow being used by anyone.
            existing = _userService.GetByEmail(userSave.Username);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Username", "A user using this as their email already exists");
                hasErrors = true;
            }
            existing = _userService.GetByUsername(userSave.Email);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Email", "A user using this as their username already exists");
                hasErrors = true;
            }

            // if the found user has their email for username, we want to keep this synced when changing the email.
            // we have already cross-checked above that the email isn't colliding with anything, so we can safely assign it here.
            if (_securitySettings.UsernameIsEmail && found.Username == found.Email && userSave.Username != userSave.Email)
            {
                userSave.Username = userSave.Email;
            }

            if (hasErrors)
                return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));

            //merge the save data onto the user
            var user = _umbracoMapper.Map(userSave, found);

            _userService.Save(user);

            var display = _umbracoMapper.Map<UserDisplay>(user);

            // determine if the user has changed their own language;
            var currentUser = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser;
            var userHasChangedOwnLanguage =
                user.Id == currentUser.Id && currentUser.Language != user.Language;

            var textToLocalise = userHasChangedOwnLanguage ? "speechBubbles/operationSavedHeaderReloadUser" : "speechBubbles/operationSavedHeader";
            var culture = userHasChangedOwnLanguage
                ? CultureInfo.GetCultureInfo(user.Language)
                : Thread.CurrentThread.CurrentUICulture;
            display.AddSuccessNotification(_localizedTextService.Localize(textToLocalise, culture), _localizedTextService.Localize("speechBubbles/editUserSaved", culture));
            return display;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="changingPasswordModel"></param>
        /// <returns></returns>
        public async Task<ActionResult<ModelWithNotifications<string>>> PostChangePassword(ChangingPasswordModel changingPasswordModel)
        {
            changingPasswordModel = changingPasswordModel ?? throw new ArgumentNullException(nameof(changingPasswordModel));

            if (ModelState.IsValid == false)
            {
                return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));
            }

            var intId = changingPasswordModel.Id.TryConvertTo<int>();
            if (intId.Success == false)
            {
                return NotFound();
            }

            var found = _userService.GetUserById(intId.Result);
            if (found == null)
            {
                return NotFound();
            }

            // TODO: Why don't we inject this? Then we can just inject a logger
            var passwordChanger = new PasswordChanger(_loggerFactory.CreateLogger<PasswordChanger>());
            var passwordChangeResult = await passwordChanger.ChangePasswordWithIdentityAsync(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, found, changingPasswordModel, _userManager);

            if (passwordChangeResult.Success)
            {
                var result = new ModelWithNotifications<string>(passwordChangeResult.Result.ResetPassword);
                result.AddSuccessNotification(_localizedTextService.Localize("general/success"), _localizedTextService.Localize("user/passwordChangedGeneric"));
                return result;
            }

            foreach (var memberName in passwordChangeResult.Result.ChangeError.MemberNames)
            {
                ModelState.AddModelError(memberName, passwordChangeResult.Result.ChangeError.ErrorMessage);
            }

            return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));
        }


        /// <summary>
        /// Disables the users with the given user ids
        /// </summary>
        /// <param name="userIds"></param>
        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public IActionResult PostDisableUsers([FromQuery]int[] userIds)
        {
            var tryGetCurrentUserId = _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId();
            if (tryGetCurrentUserId && userIds.Contains(tryGetCurrentUserId.Result))
            {
                return ValidationErrorResult.CreateNotificationValidationErrorResult("The current user cannot disable itself");
            }

            var users = _userService.GetUsersById(userIds).ToArray();
            foreach (var u in users)
            {
                u.IsApproved = false;
                u.InvitedDate = null;
            }
            _userService.Save(users);

            if (users.Length > 1)
            {
                return new UmbracoNotificationSuccessResponse(
                    _localizedTextService.Localize("speechBubbles/disableUsersSuccess", new[] {userIds.Length.ToString()}));
            }

            return new UmbracoNotificationSuccessResponse(
                _localizedTextService.Localize("speechBubbles/disableUserSuccess", new[] { users[0].Name }));
        }

        /// <summary>
        /// Enables the users with the given user ids
        /// </summary>
        /// <param name="userIds"></param>
        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public IActionResult PostEnableUsers([FromQuery]int[] userIds)
        {
            var users = _userService.GetUsersById(userIds).ToArray();
            foreach (var u in users)
            {
                u.IsApproved = true;
            }
            _userService.Save(users);

            if (users.Length > 1)
            {
                return new UmbracoNotificationSuccessResponse(
                    _localizedTextService.Localize("speechBubbles/enableUsersSuccess", new[] { userIds.Length.ToString() }));
            }

            return new UmbracoNotificationSuccessResponse(
                _localizedTextService.Localize("speechBubbles/enableUserSuccess", new[] { users[0].Name }));
        }

        /// <summary>
        /// Unlocks the users with the given user ids
        /// </summary>
        /// <param name="userIds"></param>
        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public async Task<IActionResult> PostUnlockUsers([FromQuery]int[] userIds)
        {
            if (userIds.Length <= 0) return Ok();
            var notFound = new List<int>();

            foreach (var u in userIds)
            {
                var user = await _userManager.FindByIdAsync(u.ToString());
                if (user == null)
                {
                    notFound.Add(u);
                    continue;
                }

                var unlockResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now);
                if (unlockResult.Succeeded == false)
                {
                    return new ValidationErrorResult(
                        $"Could not unlock for user {u} - error {unlockResult.Errors.ToErrorMessage()}");
                }

                if (userIds.Length == 1)
                {
                    return new UmbracoNotificationSuccessResponse(
                        _localizedTextService.Localize("speechBubbles/unlockUserSuccess", new[] {user.Name}));
                }
            }

            return new UmbracoNotificationSuccessResponse(
                _localizedTextService.Localize("speechBubbles/unlockUsersSuccess", new[] {(userIds.Length - notFound.Count).ToString()}));
        }

        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public IActionResult PostSetUserGroupsOnUsers([FromQuery]string[] userGroupAliases, [FromQuery]int[] userIds)
        {
            var users = _userService.GetUsersById(userIds).ToArray();
            var userGroups = _userService.GetUserGroupsByAlias(userGroupAliases).Select(x => x.ToReadOnlyGroup()).ToArray();
            foreach (var u in users)
            {
                u.ClearGroups();
                foreach (var userGroup in userGroups)
                {
                    u.AddGroup(userGroup);
                }
            }
            _userService.Save(users);
            return new UmbracoNotificationSuccessResponse(
                _localizedTextService.Localize("speechBubbles/setUserGroupOnUsersSuccess"));
        }

        /// <summary>
        /// Deletes the non-logged in user provided id
        /// </summary>
        /// <param name="id">User Id</param>
        /// <remarks>
        /// Limited to users that haven't logged in to avoid issues with related records constrained
        /// with a foreign key on the user Id
        /// </remarks>
        [Authorize(Policy = AuthorizationPolicies.AdminUserEditsRequireAdmin)]
        public IActionResult PostDeleteNonLoggedInUser(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check user hasn't logged in.  If they have they may have made content changes which will mean
            // the Id is associated with audit trails, versions etc. and can't be removed.
            if (user.LastLoginDate != default(DateTime))
            {
                return BadRequest();
            }

            var userName = user.Name;
            _userService.Delete(user, true);

            return new UmbracoNotificationSuccessResponse(
                _localizedTextService.Localize("speechBubbles/deleteUserSuccess", new[] { userName }));
        }

        public class PagedUserResult : PagedResult<UserBasic>
        {
            public PagedUserResult(long totalItems, long pageNumber, long pageSize) : base(totalItems, pageNumber, pageSize)
            {
                UserStates = new Dictionary<UserState, int>();
            }

            /// <summary>
            /// This is basically facets of UserStates key = state, value = count
            /// </summary>
            [DataMember(Name = "userStates")]
            public IDictionary<UserState, int> UserStates { get; set; }
        }

    }
}
