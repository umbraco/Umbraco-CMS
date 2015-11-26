using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Threading;
using System.Web;
using AutoMapper;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Security
{
    internal static class WebAuthExtensions
    {
        /// <summary>
        /// This will set a an authenticated IPrincipal to the current request given the IUser object
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static IPrincipal SetPrincipalForRequest(this HttpRequestMessage request, IUser user)
        {
            var principal = new ClaimsPrincipal(
                new UmbracoBackOfficeIdentity(
                    new ClaimsIdentity(),
                    Mapper.Map<UserData>(user)));

            //It is actually not good enough to set this on the current app Context and the thread, it also needs
            // to be set explicitly on the HttpContext.Current !! This is a strange web api thing that is actually 
            // an underlying fault of asp.net not propogating the User correctly.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
            var http = request.TryGetHttpContext();
            if (http)
            {
                http.Result.User = principal;
            }
            Thread.CurrentPrincipal = principal;

            //For WebAPI
            request.SetUserPrincipal(principal);

            return principal;
        }

        /// <summary>
        /// This will set a an authenticated IPrincipal to the current request given the IUser object
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        internal static IPrincipal SetPrincipalForRequest(this HttpContextBase httpContext, UserData userData)
        {
            var principal = new ClaimsPrincipal(
                new UmbracoBackOfficeIdentity(
                    new ClaimsIdentity(),
                    userData));

            //It is actually not good enough to set this on the current app Context and the thread, it also needs
            // to be set explicitly on the HttpContext.Current !! This is a strange web api thing that is actually 
            // an underlying fault of asp.net not propogating the User correctly.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
            httpContext.User = principal;
            Thread.CurrentPrincipal = principal;
            return principal;
        }
    }
}