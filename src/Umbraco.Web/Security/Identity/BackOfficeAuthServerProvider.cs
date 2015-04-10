using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OAuth;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// A simple OAuth server provider to verify back office users
    /// </summary>
    public class BackOfficeAuthServerProvider : OAuthAuthorizationServerProvider
    {
        private readonly BackOfficeAuthServerProviderOptions _options;

        public BackOfficeAuthServerProvider(BackOfficeAuthServerProviderOptions options = null)
        {            
            if (options == null)
                options = new BackOfficeAuthServerProviderOptions();
            _options = options;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<BackOfficeUserManager>();

            if (_options.AllowCors)
            {
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });    
            }
            

            var user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            var identity = await userManager.ClaimsIdentityFactory.CreateAsync(userManager, user, context.Options.AuthenticationType);

            context.Validated(identity);
        }
    }
}