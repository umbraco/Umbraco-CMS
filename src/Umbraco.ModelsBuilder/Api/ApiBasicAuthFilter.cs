using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;

namespace Umbraco.ModelsBuilder.Api
{

    //TODO: This needs to be changed:
    // * Authentication cannot happen in a filter, only Authorization
    // * The filter must be an AuthorizationFilter, not an ActionFilter
    // * Authorization must be done using the Umbraco logic - it is very specific for claim checking for ASP.Net Identity
    // * Theoretically this shouldn't be required whatsoever because when we authenticate a request that has Basic Auth (i.e. for
    //   VS to work, it will add the correct Claims to the Identity and it will automatically be authorized.
    //
    // we *do* have POC supporting ASP.NET identity, however they require some config on the server
    // we'll keep using this quick-and-dirty method for the time being

    public class ApiBasicAuthFilter : System.Web.Http.Filters.ActionFilterAttribute // use the http one, not mvc, with api controllers!
    {
        private static readonly char[] Separator = ":".ToCharArray();
        private readonly string _section;

        public ApiBasicAuthFilter(string section)
        {
            _section = section;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                var user = Authenticate(actionContext.Request);
                if (user == null || !user.AllowedSections.Contains(_section))
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                //else
                //{
                //    // note - would that be a proper way to pass data to the controller?
                //    // see http://stevescodingblog.co.uk/basic-authentication-with-asp-net-webapi/
                //    actionContext.ControllerContext.RouteData.Values["umbraco-user"] = user;
                //}

                base.OnActionExecuting(actionContext);
            }
            catch
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }

        private static IUser Authenticate(HttpRequestMessage request)
        {
            var ah = request.Headers.Authorization;
            if (ah == null || ah.Scheme != "Basic")
                return null;

            var token = ah.Parameter;
            var credentials = Encoding.ASCII
                .GetString(Convert.FromBase64String(token))
                .Split(Separator);
            if (credentials.Length != 2)
                return null;

            var username = ApiClient.DecodeTokenElement(credentials[0]);
            var password = ApiClient.DecodeTokenElement(credentials[1]);

            var providerKey = UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider;
            var provider = Membership.Providers[providerKey];
            if (provider == null || !provider.ValidateUser(username, password))
                return null;
            var user = Current.Services.UserService.GetByUsername(username);
            if (!user.IsApproved || user.IsLockedOut)
                return null;
            return user;
        }
    }
}