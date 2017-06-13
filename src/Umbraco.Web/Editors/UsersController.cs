using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
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
            //TODO: Allow re-inviting the user!

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

            var identityUser = await UserManager.FindByEmailAsync(userSave.Email);
            if (identityUser != null && identityUser.LastLoginDateUtc.HasValue && identityUser.LastLoginDateUtc.Value == default(DateTime))
            {
                ModelState.AddModelError("Email", "A user with the email already exists");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            if (identityUser == null)
            {
                identityUser = new BackOfficeIdentityUser
                {
                    Email = userSave.Email,
                    Name = userSave.Name,
                    UserName = userSave.Email
                };

                //Save the user first
                var result = await UserManager.CreateAsync(identityUser);
                if (result.Succeeded == false)
                {
                    throw new HttpResponseException(
                        Request.CreateNotificationValidationErrorResponse(string.Join(", ", result.Errors)));
                }
            }
            else
            {
                identityUser.Name = userSave.Name;
                var result = await UserManager.UpdateAsync(identityUser);
                if (result.Succeeded == false)
                {
                    throw new HttpResponseException(
                        Request.CreateNotificationValidationErrorResponse(string.Join(", ", result.Errors)));
                }
            }

            //Add/Update to roles
            var roleResult = await UserManager.AddToRolesAsync(identityUser.Id, userSave.UserGroups.ToArray());
            if (roleResult.Succeeded == false)
            {
                throw new HttpResponseException(
                    Request.CreateNotificationValidationErrorResponse(string.Join(", ", roleResult.Errors)));
            }
            
            //now send the email
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(identityUser.Id);
            var link = string.Format("{0}#/login/false?invite={1}{2}{3}",
                ApplicationContext.UmbracoApplicationUrl,
                identityUser.Id,
                WebUtility.UrlEncode("|"),
                token.ToUrlBase64());

            await UserManager.EmailService.SendAsync(new IdentityMessage
            {
                Body = string.Format("You have been invited to the Umbraco Back Office!\n\n{0}\n\nClick this link to accept the invite\n\n{1}", userSave.Message, link),
                Destination = userSave.Email,
                Subject = "You have been invited to the Umbraco Back Office!"
            });

            return Mapper.Map<UserDisplay>(identityUser);
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