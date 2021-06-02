using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Owin;
using Newtonsoft.Json;
using Umbraco.Core;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Used to handle errors registered by external login providers
    /// </summary>
    /// <remarks>
    /// When an external login provider registers an error with <see cref="OwinExtensions.SetExternalLoginProviderErrors"/> during the OAuth process,
    /// this middleware will detect that, store the errors into cookie data and redirect to the back office login so we can read the errors back out.
    /// </remarks>
    internal class BackOfficeExternalLoginProviderErrorMiddlware : OwinMiddleware
    {
        public BackOfficeExternalLoginProviderErrorMiddlware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var shortCircuit = false;
            if (!context.Request.Uri.IsClientSideRequest())
            {
                // check if we have any errors registered
                var errors = context.GetExternalLoginProviderErrors();
                if (errors != null)
                {
                    shortCircuit = true;

                    var serialized = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errors)));

                    context.Response.Cookies.Append(ViewDataExtensions.TokenExternalSignInError, serialized, new CookieOptions
                    {
                        Expires = DateTime.Now.AddMinutes(5),
                        HttpOnly = true,
                        Secure = context.Request.IsSecure
                    });

                    context.Response.Redirect(context.Request.Uri.ToString());
                }
            }

            if (Next != null && !shortCircuit)
            {
                await Next.Invoke(context);
            }
        }
    }
}
