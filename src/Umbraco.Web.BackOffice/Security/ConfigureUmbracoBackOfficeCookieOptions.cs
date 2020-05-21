using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Web;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Used to configure <see cref="CookieAuthenticationOptions"/> for the back office authentication type
    /// </summary>
    public class ConfigureUmbracoBackOfficeCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ISecuritySettings _securitySettings;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeState _runtimeState;
        private readonly IDataProtectionProvider _dataProtection;
        private readonly IRequestCache _requestCache;

        public ConfigureUmbracoBackOfficeCookieOptions(
            IUmbracoContextAccessor umbracoContextAccessor,
            ISecuritySettings securitySettings,
            IGlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            IDataProtectionProvider dataProtection,
            IRequestCache requestCache)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _securitySettings = securitySettings;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _runtimeState = runtimeState;
            _dataProtection = dataProtection;
            _requestCache = requestCache;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name != Constants.Security.BackOfficeAuthenticationType) return;

            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(_globalSettings.TimeOutInMinutes);
            options.Cookie.Domain = _securitySettings.AuthCookieDomain;
            options.Cookie.Name = _securitySettings.AuthCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = _globalSettings.UseHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
            options.Cookie.Path = "/";

            options.DataProtectionProvider = _dataProtection;

            // NOTE: This is borrowed directly from aspnetcore source
            // Note: the purpose for the data protector must remain fixed for interop to work.
            var dataProtector = options.DataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", name, "v2");
            var ticketDataFormat = new TicketDataFormat(dataProtector);

            options.TicketDataFormat = new UmbracoSecureDataFormat(_globalSettings.TimeOutInMinutes, ticketDataFormat);

            //Custom cookie manager so we can filter requests
            options.CookieManager = new BackOfficeCookieManager(
                _umbracoContextAccessor,
                _runtimeState,
                _hostingEnvironment,
                _globalSettings,
                _requestCache);
            // _explicitPaths); TODO: Implement this once we do OAuth somehow
        }

        public void Configure(CookieAuthenticationOptions options)
        {
        }
    }
}
