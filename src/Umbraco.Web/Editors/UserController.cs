using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Users)]
    public class UserController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UserController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public UserController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
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

        //TODO: This will probably not be UserDisplay objects since there's probably too much data in the display object for a grid
        public PagedResult<UserDisplay> GetPagedUsers(
            int id,
            int pageNumber = 1,
            int pageSize = 0,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "")
        {

            //TODO: Make this real, for now this is mock data

            var startId = 100 + ((pageNumber -1) * pageSize);
            var numUsers = pageSize;
            var users = new List<UserDisplay>();
            var userTypes = Services.UserService.GetAllUserGroups().ToDictionary(x => x.Alias, x => x.Name);
            var cultures = Services.TextService.GetSupportedCultures().ToDictionary(x => x.Name, x => x.DisplayName);
            for (int i = 0; i < numUsers; i++)
            {
                var display = new UserDisplay
                {
                    Id = startId,
                    //UserType = "writer",
                    AllowedSections = new[] {"content", "media"},
                    AvailableUserGroups = userTypes,
                    Email = "test" + startId + "@test.com",
                    Name = "User " + startId,
                    Culture = "en-US",
                    AvailableCultures = cultures,
                    Path = "-1," + startId,
                    ParentId = -1,
                    StartContentId = -1,
                    StartMediaId = -1
                };
                users.Add(display);
                startId++;
            }

            return new PagedResult<UserDisplay>(100, pageNumber, pageSize)
            {
                Items = users
            };
        }

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

            //TODO: More validation, password changing logic, persisting
            return Mapper.Map<IUser, UserDisplay>(found);
        }

        /// <summary>
        /// Disables the user with the given user id
        /// </summary>
        /// <param name="userId"></param>
        public bool PostDisableUser([FromUri]int userId)
        {
            var user = Services.UserService.GetUserById(userId);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            //without the permanent flag, this will just disable
            Services.UserService.Delete(user);
            return true;
        }
    }
}