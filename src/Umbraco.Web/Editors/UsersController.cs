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