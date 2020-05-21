//using Microsoft.AspNetCore.Authentication.Cookies;
//using System;
//using Umbraco.Core;
//using Umbraco.Core.Cache;
//using Umbraco.Core.Configuration;
//using Umbraco.Core.Configuration.UmbracoSettings;
//using Umbraco.Core.Hosting;
//using Umbraco.Core.IO;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.DataProtection;

//namespace Umbraco.Web.BackOffice.Security
//{
//    /// <summary>
//    /// Umbraco auth cookie options
//    /// </summary>
//    public sealed class UmbracoBackOfficeCookieAuthOptions : CookieAuthenticationOptions
//    {
//        public int LoginTimeoutMinutes { get; }

//        public UmbracoBackOfficeCookieAuthOptions(
//            string[] explicitPaths,
//            IUmbracoContextAccessor umbracoContextAccessor,
//            ISecuritySettings securitySettings,
//            IGlobalSettings globalSettings,
//            IHostingEnvironment hostingEnvironment,
//            IRuntimeState runtimeState,
//            IDataProtectionProvider dataProtection,
//            IRequestCache requestCache)
//        {
//            LoginTimeoutMinutes = globalSettings.TimeOutInMinutes;
//            //AuthenticationType = Constants.Security.BackOfficeAuthenticationType;

//            SlidingExpiration = true;
//            ExpireTimeSpan = TimeSpan.FromMinutes(LoginTimeoutMinutes);
//            Cookie.Domain= securitySettings.AuthCookieDomain;
//            Cookie.Name = securitySettings.AuthCookieName;
//            Cookie.HttpOnly = true;
//            Cookie.SecurePolicy = globalSettings.UseHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
//            Cookie.Path = "/";

//            DataProtectionProvider = dataProtection;
//            // Note: the purpose for the data protector must remain fixed for interop to work.
//            var dataProtector = options.DataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", name, "v2");
//            options.TicketDataFormat = new TicketDataFormat(dataProtector);

//            TicketDataFormat = new UmbracoSecureDataFormat(LoginTimeoutMinutes, secureDataFormat1);


//            //Custom cookie manager so we can filter requests
//            CookieManager = new BackOfficeCookieManager(umbracoContextAccessor, runtimeState, hostingEnvironment, globalSettings, requestCache, explicitPaths);
//        }

//        /// <summary>
//        /// Creates the cookie options for saving the auth cookie
//        /// </summary>
//        /// <param name="ctx"></param>
//        /// <param name="ticket"></param>
//        /// <returns></returns>
//        public CookieOptions CreateRequestCookieOptions(HttpContext ctx, AuthenticationTicket ticket)
//        {
//            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
//            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

//            var issuedUtc = ticket.Properties.IssuedUtc ?? DateTime.UtcNow;
//            var expiresUtc = ticket.Properties.ExpiresUtc ?? issuedUtc.Add(ExpireTimeSpan);

//            var cookieOptions = new CookieOptions
//            {
//                Path = "/",
//                Domain = Cookie.Domain ?? null,
//                HttpOnly = true,
//                Secure = Cookie.SecurePolicy == CookieSecurePolicy.Always
//                                         || Cookie.SecurePolicy == CookieSecurePolicy.SameAsRequest && ctx.Request.IsHttps,
//            };

//            if (ticket.Properties.IsPersistent)
//                cookieOptions.Expires = expiresUtc.UtcDateTime;

//            return cookieOptions;
//        }

//    }
//}
