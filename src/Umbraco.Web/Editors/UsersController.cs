using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using AutoMapper;
using ClientDependency.Core;
using Microsoft.AspNet.Identity;
using Umbraco.Core;
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

        public UsersController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper, BackOfficeUserManager<BackOfficeIdentityUser> backOfficeUserManager) : base(umbracoContext, umbracoHelper, backOfficeUserManager)
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

        [FileUploadCleanupFilter(false)]
        public async Task<HttpResponseMessage> PostSetAvatar(int id)
        {
            if (Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = IOHelper.MapPath("~/App_Data/TEMP/FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            //must have a file
            if (result.FileData.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
           
            var user = Services.UserService.GetUserById(id);
            if (user == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var tempFiles = new PostedFiles();

            if (result.FileData.Count > 1)
                return Request.CreateValidationErrorResponse("The request was not formatted correctly, only one file can be attached to the request");

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

                Services.UserService.Save(user);

                //track the temp file so the cleanup filter removes it
                tempFiles.UploadedFiles.Add(new ContentItemFile
                {                             
                    TempFilePath = file.LocalFileName
                });
            }

            return Request.CreateResponse(HttpStatusCode.OK, user.GetCurrentUserAvatarUrls(Services.UserService, ApplicationContext.ApplicationCache.StaticCache));
        }

        public HttpResponseMessage PostClearAvatar(int id)
        {
            var found = Services.UserService.GetUserById(id);
            if (found == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var filePath = found.Avatar;

            found.Avatar = null;

            Services.UserService.Save(found);

            if (FileSystemProviderManager.Current.MediaFileSystem.FileExists(filePath))
                FileSystemProviderManager.Current.MediaFileSystem.DeleteFile(filePath);

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
        /// Returns all user groups
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserGroupBasic> GetUserGroups()
        {
            return Mapper.Map<IEnumerable<IUserGroup>, IEnumerable<UserGroupBasic>>(Services.UserService.GetAllUserGroups());
        }

        /// <summary>
        /// Returns all user groups
        /// </summary>
        /// <returns></returns>
        public UserGroupDisplay GetUserGroup(int id)
        {
            var found = Services.UserService.GetUserGroupById(id);
            if (found == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            return Mapper.Map<UserGroupDisplay>(found);
        }

        /// <summary>
        /// Returns a paged users collection
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="userGroups"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public PagedResult<UserDisplay> GetPagedUsers(
            int pageNumber = 1,
            int pageSize = 10,
            string orderBy = "username",
            Direction orderDirection = Direction.Ascending,
            [FromUri]string[] userGroups = null,
            string filter = "")
        {
            long pageIndex = pageNumber - 1;
            long total;
            var result = Services.UserService.GetAll(pageIndex, pageSize, out total, orderBy, orderDirection, null, userGroups, filter);

            if (total == 0)
            {
                return new PagedResult<UserDisplay>(0, 0, 0);
            }

            return new PagedResult<UserDisplay>(total, pageNumber, pageSize)
            {
                Items = Mapper.Map<IEnumerable<UserDisplay>>(result)
            };
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="userSave"></param>
        /// <returns></returns>
        public UserDisplay PostCreateUser(UserInvite userSave)
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

            var user = Mapper.Map<IUser>(userSave);

            Services.UserService.Save(user);

            return Mapper.Map<UserDisplay>(user);
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
                var created = await UserManager.CreateAsync(new BackOfficeIdentityUser
                {
                    Email = userSave.Email,
                    Name = userSave.Name,
                    UserName = userSave.Email
                });
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
            
            //Save the user first
            Services.UserService.Save(user);            
            var display = Mapper.Map<UserDisplay>(user);

            //send the email

            await SendEmailAsync(display, Security.CurrentUser.Name, userSave.Message);

            return display;
        }

        private HttpContextBase EnsureHttpContext()
        {
            var attempt = this.TryGetHttpContext();
            if (attempt.Success == false)
                throw new InvalidOperationException("This method requires that an HttpContext be active");
            return attempt.Result;
        }

        private async Task SendEmailAsync(UserDisplay userDisplay, string from, string message)
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

            var virtualPath = SystemDirectories.Umbraco.EnsureEndsWith("/") + "Views/UserInvite.cshtml";
            var view = IOHelper.MapPath(virtualPath);

            //This should always exist but just in case, we'll check
            if (System.IO.File.Exists(view) == false)
            {
                await UserManager.EmailService.SendAsync(new IdentityMessage
                {
                    Body = string.Format("<html><body>You have been invited to the Umbraco Back Office!<br/><br/>{0}\n\nClick this link to accept the invite\n\n{1}", message, inviteUri.ToString()),
                    Destination = userDisplay.Email,
                    Subject = "You have been invited to the Umbraco Back Office"
                });
            }
            else
            {     
                //Send an email based on a razor view template
                var requestContext = new RequestContext(http, new RouteData());                
                var userInviteEmail = new UserInviteEmail
                {
                    StartContentIds = userDisplay.StartContentIds,
                    StartMediaIds = userDisplay.StartMediaIds,
                    Email = userDisplay.Email,
                    Name = userDisplay.Name,
                    UserGroups = userDisplay.UserGroups,
                    Message = message,
                    InviteUrl = inviteUri.ToString(),
                    FromName = from
                };
                var viewResult = requestContext.RenderViewToString(new ViewDataDictionary(), new TempDataDictionary(), virtualPath, userInviteEmail, false);
                await UserManager.EmailService.SendAsync(new IdentityMessage
                {
                    Body = viewResult,
                    Destination = userDisplay.Email,
                    Subject = "You have been invited to the Umbraco Back Office"
                });
            }
            
        }

        /// <summary>
        /// Saves a user
        /// </summary>
        /// <param name="userSave"></param>
        /// <returns></returns>
        public UserDisplay PostSaveUser(UserSave userSave)
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
            existing = Services.UserService.GetByUsername(userSave.Name);
            if (existing != null && existing.Id != userSave.Id)
            {
                ModelState.AddModelError("Email", "A user with the email already exists");
                hasErrors = true;
            }            

            if (hasErrors)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            //TODO: More validation, password changing logic, persisting

            //merge the save data onto the user
            var user = Mapper.Map(userSave, found);

            Services.UserService.Save(user);

            var display = Mapper.Map<UserDisplay>(user);

            display.AddSuccessNotification(Services.TextService.Localize("speechBubbles/operationSavedHeader"), Services.TextService.Localize("speechBubbles/editUserSaved"));
            return display;
        }

        /// <summary>
        /// Disables the users with the given user ids
        /// </summary>
        /// <param name="userIds"></param>
        public bool PostDisableUsers([FromUri]int[] userIds)
        {
            var users = Services.UserService.GetUsersById(userIds).ToArray();
            foreach (var u in users)
            {
                u.IsApproved = false;
            }
            Services.UserService.Save(users);

            return true;
        }

        /// <summary>
        /// Enables the users with the given user ids
        /// </summary>
        /// <param name="userIds"></param>
        public bool PostEnableUsers([FromUri]int[] userIds)
        {
            var users = Services.UserService.GetUsersById(userIds).ToArray();
            foreach (var u in users)
            {
                u.IsApproved = true;
            }
            Services.UserService.Save(users);

            return true;
        }
    }
}