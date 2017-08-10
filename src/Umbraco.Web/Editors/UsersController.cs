using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;
using AutoMapper;
using ClientDependency.Core;
using Microsoft.AspNet.Identity;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using IUser = Umbraco.Core.Models.Membership.IUser;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Users)]
    [PrefixlessBodyModelValidator]
    [IsCurrentUserModelFilter]
    public class UsersController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UsersController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public UsersController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        public UsersController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper, BackOfficeUserManager<BackOfficeIdentityUser> backOfficeUserManager) 
            : base(umbracoContext, umbracoHelper, backOfficeUserManager)
        {
        }


        /// <summary>
        /// Returns a list of the sizes of gravatar urls for the user or null if the gravatar server cannot be reached
        /// </summary>
        /// <returns></returns>
        public string[] GetCurrentUserAvatarUrls()
        {
            var urls = UmbracoContext.Security.CurrentUser.GetCurrentUserAvatarUrls(Services.UserService, ApplicationContext.ApplicationCache.StaticCache);
            if (urls == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not access Gravatar endpoint"));

            return urls;
        }

        [AppendUserModifiedHeader("id")]
        [FileUploadCleanupFilter(false)]
        public async Task<HttpResponseMessage> PostSetAvatar(int id)
        {
            return await PostSetAvatarInternal(Request, Services.UserService, ApplicationContext.ApplicationCache.StaticCache, id);
        }

        internal static async Task<HttpResponseMessage> PostSetAvatarInternal(HttpRequestMessage request, IUserService userService, ICacheProvider staticCache, int id)
        {
            if (request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = IOHelper.MapPath("~/App_Data/TEMP/FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var result = await request.Content.ReadAsMultipartAsync(provider);

            //must have a file
            if (result.FileData.Count == 0)
            {
                return request.CreateResponse(HttpStatusCode.NotFound);
            }

            var user = userService.GetUserById(id);
            if (user == null)
                return request.CreateResponse(HttpStatusCode.NotFound);

            var tempFiles = new PostedFiles();

            if (result.FileData.Count > 1)
                return request.CreateValidationErrorResponse("The request was not formatted correctly, only one file can be attached to the request");

            //get the file info
            var file = result.FileData[0];
            var fileName = file.Headers.ContentDisposition.FileName.Trim(new[] { '\"' }).TrimEnd();
            var safeFileName = fileName.ToSafeFileName();
            var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

            if (UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Contains(ext) == false)
            {
                //generate a path of known data, we don't want this path to be guessable
                user.Avatar = "UserAvatars/" + (user.Id + safeFileName).ToSHA1() + "." + ext;

                using (var fs = System.IO.File.OpenRead(file.LocalFileName))
                {
                    FileSystemProviderManager.Current.MediaFileSystem.AddFile(user.Avatar, fs, true);
                }

                userService.Save(user);

                //track the temp file so the cleanup filter removes it
                tempFiles.UploadedFiles.Add(new ContentItemFile
                {
                    TempFilePath = file.LocalFileName
                });
            }

            return request.CreateResponse(HttpStatusCode.OK, user.GetCurrentUserAvatarUrls(userService, staticCache));
        }

        [AppendUserModifiedHeader("id")]
        public HttpResponseMessage PostClearAvatar(int id)
        {
            var found = Services.UserService.GetUserById(id);
            if (found == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

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

            Services.UserService.Save(found);

            if (filePath.IsNullOrWhiteSpace() == false)
            {
                if (FileSystemProviderManager.Current.MediaFileSystem.FileExists(filePath))
                    FileSystemProviderManager.Current.MediaFileSystem.DeleteFile(filePath);
            }

            return Request.CreateResponse(HttpStatusCode.OK, found.GetCurrentUserAvatarUrls(Services.UserService, ApplicationContext.ApplicationCache.StaticCache));
        }

        /// <summary>
        /// Gets a user by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UserDisplay GetById(int id)
        {
            var user = Services.UserService.GetUserById(id);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IUser, UserDisplay>(user);
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
            [FromUri]string[] userGroups = null,
            [FromUri]UserState[] userStates = null,
            string filter = "")
        {
            long pageIndex = pageNumber - 1;
            long total;
            var result = Services.UserService.GetAll(pageIndex, pageSize, out total, orderBy, orderDirection, userStates, userGroups, filter);
            
            var paged = new PagedUserResult(total, pageNumber, pageSize)
            {
                Items = Mapper.Map<IEnumerable<UserBasic>>(result),
                UserStates = Services.UserService.GetUserStates()
            };

            return paged;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="userSave"></param>
        /// <returns></returns>
        public async Task<UserDisplay> PostCreateUser(UserInvite userSave)
        {
            if (userSave == null) throw new ArgumentNullException("userSave");

            if (ModelState.IsValid == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var existing = Services.UserService.GetByEmail(userSave.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "A user with the email already exists");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            //we want to create the user with the UserManager, this ensures the 'empty' (special) password
            //format is applied without us having to duplicate that logic
            var identityUser = BackOfficeIdentityUser.CreateNew(userSave.Email, userSave.Email, GlobalSettings.DefaultUILanguage);
            identityUser.Name = userSave.Name;

            var created = await UserManager.CreateAsync(identityUser);
            if (created.Succeeded == false)
            {
                throw new HttpResponseException(
                    Request.CreateNotificationValidationErrorResponse(string.Join(", ", created.Errors)));
            }

            //we need to generate a password, however we can only do that if the user manager has a password validator that
            //we can read values from
            var passwordValidator = UserManager.PasswordValidator as PasswordValidator;
            var resetPassword = string.Empty;
            if (passwordValidator != null)
            {
                var password = UserManager.GeneratePassword();

                var result = await UserManager.AddPasswordAsync(identityUser.Id, password);
                if (result.Succeeded == false)
                {
                    throw new HttpResponseException(
                        Request.CreateNotificationValidationErrorResponse(string.Join(", ", created.Errors)));
                }
                resetPassword = password;
            }

            //now re-look the user back up which will now exist
            var user = Services.UserService.GetByEmail(userSave.Email);

            //map the save info over onto the user
            user = Mapper.Map(userSave, user);

            //since the back office user is creating this user, they will be set to approved
            user.IsApproved = true;

            Services.UserService.Save(user);

            var display = Mapper.Map<UserDisplay>(user);
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
        public async Task<UserDisplay> PostInviteUser(UserInvite userSave)
        {
            if (userSave == null) throw new ArgumentNullException("userSave");

            if (userSave.Message.IsNullOrWhiteSpace())
                ModelState.AddModelError("Message", "Message cannot be empty");

            if (ModelState.IsValid == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var hasSmtp = GlobalSettings.HasSmtpServerConfigured(RequestContext.VirtualPathRoot);
            if (hasSmtp == false)
            {
                throw new HttpResponseException(
                    Request.CreateNotificationValidationErrorResponse("No Email server is configured"));
            }

            var user = Services.UserService.GetByEmail(userSave.Email);
            if (user != null && (user.LastLoginDate != default(DateTime) || user.EmailConfirmedDate.HasValue))
            {
                ModelState.AddModelError("Email", "A user with the email already exists");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            if (user == null)
            {
                //we want to create the user with the UserManager, this ensures the 'empty' (special) password
                //format is applied without us having to duplicate that logic
                var identityUser = BackOfficeIdentityUser.CreateNew(userSave.Email, userSave.Email, GlobalSettings.DefaultUILanguage);
                identityUser.Name = userSave.Name;

                var created = await UserManager.CreateAsync(identityUser);
                if (created.Succeeded == false)
                {
                    throw new HttpResponseException(
                        Request.CreateNotificationValidationErrorResponse(string.Join(", ", created.Errors)));
                }

                //now re-look the user back up
                user = Services.UserService.GetByEmail(userSave.Email);
            }

            //map the save info over onto the user
            user = Mapper.Map(userSave, user);

            //ensure the invited date is set
            user.InvitedDate = DateTime.Now;

            //Save the updated user
            Services.UserService.Save(user);
            var display = Mapper.Map<UserDisplay>(user);

            //send the email

            await SendUserInviteEmailAsync(display, Security.CurrentUser.Name, user, userSave.Message);

            return display;
        }

        

        private HttpContextBase EnsureHttpContext()
        {
            var attempt = this.TryGetHttpContext();
            if (attempt.Success == false)
                throw new InvalidOperationException("This method requires that an HttpContext be active");
            return attempt.Result;
        }

        private async Task SendUserInviteEmailAsync(UserBasic userDisplay, string from, IUser to, string message)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync((int)userDisplay.Id);

            var inviteToken = string.Format("{0}{1}{2}",
                (int)userDisplay.Id,
                WebUtility.UrlEncode("|"),
                token.ToUrlBase64());

            // Get an mvc helper to get the url
            var http = EnsureHttpContext();
            var urlHelper = new UrlHelper(http.Request.RequestContext);
            var action = urlHelper.Action("VerifyInvite", "BackOffice",
                new
                {
                    area = GlobalSettings.UmbracoMvcArea,
                    invite = inviteToken
                });

            // Construct full URL using configured application URL (which will fall back to request)
            var applicationUri = new Uri(ApplicationContext.UmbracoApplicationUrl);
            var inviteUri = new Uri(applicationUri, action);

            var emailSubject = Services.TextService.Localize("user/inviteEmailCopySubject",
                //Ensure the culture of the found user is used for the email!
                UserExtensions.GetUserCulture(to.Language, Services.TextService));
            var emailBody = Services.TextService.Localize("user/inviteEmailCopyFormat",
                //Ensure the culture of the found user is used for the email!
                UserExtensions.GetUserCulture(to.Language, Services.TextService),
                new[] { userDisplay.Name, from, message, inviteUri.ToString() });

            await UserManager.EmailService.SendAsync(new IdentityMessage
            {
                Body = emailBody,
                Destination = userDisplay.Email,
                Subject = emailSubject
            });

        }

        /// <summary>
        /// Saves a user
        /// </summary>
        /// <param name="userSave"></param>
        /// <returns></returns>
        public async Task<UserDisplay> PostSaveUser(UserSave userSave)
        {
            if (userSave == null) throw new ArgumentNullException("userSave");

            if (ModelState.IsValid == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var intId = userSave.Id.TryConvertTo<int>();
            if (intId.Success == false)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var found = Services.UserService.GetUserById(intId.Result);
            if (found == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var hasErrors = false;

            var existing = Services.UserService.GetByEmail(userSave.Email);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Email", "A user with the email already exists");
                hasErrors = true;
            }
            existing = Services.UserService.GetByUsername(userSave.Username);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Username", "A user with the username already exists");
                hasErrors = true;
            }
            // going forward we prefer to align usernames with email, so we should cross-check to make sure
            // the email or username isn't somehow being used by anyone.
            existing = Services.UserService.GetByEmail(userSave.Username);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Username", "A user using this as their email already exists");
                hasErrors = true;
            }
            existing = Services.UserService.GetByUsername(userSave.Email);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Email", "A user using this as their username already exists");
                hasErrors = true;
            }

            // if the found user has his email for username, we want to keep this synced when changing the email.
            // we have already cross-checked above that the email isn't colliding with anything, so we can safely assign it here.
            if (found.Username == found.Email && userSave.Username != userSave.Email)
            {
                userSave.Username = userSave.Email;
            }

            var resetPasswordValue = string.Empty;
            if (userSave.ChangePassword != null)
            {
                var passwordChanger = new PasswordChanger(Logger, Services.UserService);

                var passwordChangeResult = await passwordChanger.ChangePasswordWithIdentityAsync(found, userSave.ChangePassword, ModelState, UserManager);
                if (passwordChangeResult.Success)
                {
                    //depending on how the provider is configured, the password may be reset so let's store that for later
                    resetPasswordValue = passwordChangeResult.Result.ResetPassword;

                    //need to re-get the user 
                    found = Services.UserService.GetUserById(intId.Result);
                }
                else
                {
                    hasErrors = true;
                }
            }

            if (hasErrors)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            //merge the save data onto the user
            var user = Mapper.Map(userSave, found);

            Services.UserService.Save(user);

            var display = Mapper.Map<UserDisplay>(user);

            //re-map the password reset value (if any)
            if (resetPasswordValue.IsNullOrWhiteSpace() == false)
                display.ResetPasswordValue = resetPasswordValue;

            display.AddSuccessNotification(Services.TextService.Localize("speechBubbles/operationSavedHeader"), Services.TextService.Localize("speechBubbles/editUserSaved"));
            return display;
        }        

        /// <summary>
        /// Disables the users with the given user ids
        /// </summary>
        /// <param name="userIds"></param>
        public HttpResponseMessage PostDisableUsers([FromUri]int[] userIds)
        {
            if (userIds.Contains(Security.GetUserId()))
            {
                throw new HttpResponseException(
                    Request.CreateNotificationValidationErrorResponse("The current user cannot disable itself"));
            }

            var users = Services.UserService.GetUsersById(userIds).ToArray();
            foreach (var u in users)
            {
                u.IsApproved = false;
                u.InvitedDate = null;
            }
            Services.UserService.Save(users);

            if (users.Length > 1)
            {
                return Request.CreateNotificationSuccessResponse(
                    Services.TextService.Localize("speechBubbles/disableUsersSuccess", new[] {userIds.Length.ToString()}));
            }

            return Request.CreateNotificationSuccessResponse(
                Services.TextService.Localize("speechBubbles/disableUserSuccess", new[] { users[0].Name }));
        }

        /// <summary>
        /// Enables the users with the given user ids
        /// </summary>
        /// <param name="userIds"></param>
        public HttpResponseMessage PostEnableUsers([FromUri]int[] userIds)
        {
            var users = Services.UserService.GetUsersById(userIds).ToArray();
            foreach (var u in users)
            {
                u.IsApproved = true;
            }
            Services.UserService.Save(users);

            if (users.Length > 1)
            {
                return Request.CreateNotificationSuccessResponse(
                    Services.TextService.Localize("speechBubbles/enableUsersSuccess", new[] { userIds.Length.ToString() }));
            }

            return Request.CreateNotificationSuccessResponse(
                Services.TextService.Localize("speechBubbles/enableUserSuccess", new[] { users[0].Name }));            
        }

        /// <summary>
        /// Unlocks the users with the given user ids
        /// </summary>
        /// <param name="userIds"></param>
        public HttpResponseMessage PostUnlockUsers([FromUri]int[] userIds)
        {
            var users = Services.UserService.GetUsersById(userIds).ToArray();
            foreach (var u in users)
            {
                u.IsLockedOut = false;
            }
            Services.UserService.Save(users);

            if (users.Length > 1)
            {
                return Request.CreateNotificationSuccessResponse(
                    Services.TextService.Localize("speechBubbles/unlockUsersSuccess", new[] { userIds.Length.ToString() }));
            }

            return Request.CreateNotificationSuccessResponse(
                Services.TextService.Localize("speechBubbles/unlockUserSuccess", new[] { users[0].Name }));
        }

        public HttpResponseMessage PostSetUserGroupsOnUsers([FromUri]string[] userGroupAliases, [FromUri]int[] userIds)
        {
            var users = Services.UserService.GetUsersById(userIds).ToArray();
            var userGroups = Services.UserService.GetUserGroupsByAlias(userGroupAliases).Select(x => x.ToReadOnlyGroup()).ToArray();
            foreach (var u in users)
            {
                u.ClearGroups();
                foreach (var userGroup in userGroups)
                {
                    u.AddGroup(userGroup);
                }
            }
            Services.UserService.Save(users);
            return Request.CreateNotificationSuccessResponse(
                Services.TextService.Localize("speechBubbles/setUserGroupOnUsersSuccess"));
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