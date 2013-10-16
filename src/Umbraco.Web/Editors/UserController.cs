using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using legacyUser = umbraco.BusinessLogic.User;
using System.Net.Http;
using System.Collections.Specialized;
using Constants = Umbraco.Core.Constants;


namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Controller to back the User.Resource service, used for fetching user data when already authenticated. user.service is currently used for handling authentication
    /// </summary>
    [PluginController("UmbracoApi")]
    public class UserController : UmbracoAuthorizedJsonController
    {

        /// <summary>
        /// Returns the configuration for the backoffice user membership provider - used to configure the change password dialog
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> GetMembershipProviderConfig()
        {
            var provider = Membership.Providers[UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider];
            if (provider == null)
            {
                throw new InvalidOperationException("No back office membership provider found with the name " + UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider);
            }
            return provider.GetConfiguration();
        } 

        /// <summary>
        /// Returns a user by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UserDetail GetById(int id)
        {
            var user = Services.UserService.GetUserById(id);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<UserDetail>(user);
        }

        /// <summary>
        /// Changes the users password
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public HttpResponseMessage PostChangePassword(ChangePasswordModel data)
        {   
         
            var u = UmbracoContext.Security.CurrentUser;
            if (!UmbracoContext.Security.ValidateBackOfficeCredentials(u.Username, data.OldPassword))
                return new HttpResponseMessage(HttpStatusCode.Forbidden);

            if(!UmbracoContext.Security.ChangePassword(data.OldPassword, data.NewPassword))
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Returns all active users
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserBasic> GetAll()
        {
            return Services.UserService.GetAllUsers().Where(x => x.IsLockedOut == false).Select(Mapper.Map<UserBasic>);
        }
    }
}
