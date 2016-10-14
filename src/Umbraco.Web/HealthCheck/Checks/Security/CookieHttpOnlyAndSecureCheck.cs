using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "ED0D7E40-3108-4BE8-AB6D-8CC5D0A6A5B0",
        "HttpOnly and Secure cookies",
        Description = "Checks whether your site uses the httponly-flag, and if running with HTTPS uses the secure-attribute.",
        Group = "Security")]
    public class CookieHttpOnlyAndSecureCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public CookieHttpOnlyAndSecureCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckForHttpOnlyFlag(), CheckForSecureFlag() };
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("No magical action can be applied to solve this");
        }

        private HealthCheckStatus CheckForSecureFlag()
        {
            var message = string.Empty;
            bool success = true;
            var url = HealthCheckContext.HttpContext.Request.Url;
            var invalidcookies = String.Empty;
            // Access the site home page and check for the click-jack protection header or meta tag
            var useSsl = GlobalSettings.UseSSL || HealthCheckContext.HttpContext.Request.ServerVariables["SERVER_PORT"] == "443";

            // No SSL => No need to check for secure cookies
            if (!useSsl)
            {
                return new HealthCheckStatus(_textService.Localize("healthcheck/cookiesSecureNoHttps"))
                {
                    ResultType = StatusResultType.Info
                };
            }
            var address = string.Format("http{0}://{1}:{2}", useSsl ? "s" : "", url.Host.ToLower(), url.Port);
            var request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = "GET";
            if (request.CookieContainer == null)
                request.CookieContainer = new CookieContainer();
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                foreach (System.Net.Cookie cookie in response.Cookies)
                {
                    if (!cookie.Secure)
                    {
                        success = false;
                        invalidcookies += string.IsNullOrEmpty(invalidcookies) ? cookie.Name : invalidcookies+ ", " + cookie.Name;
                    }
                }
                message = success
                    ? _textService.Localize("healthcheck/cookiesAllSecure")
                    : _textService.Localize("healthcheck/cookiesNotAllSecure", new[] { invalidcookies });
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck/httpsCheckInvalidUrl", new[] { address, ex.Message });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Warning
                };
        }


        private HealthCheckStatus CheckForHttpOnlyFlag()
        {
            var message = string.Empty;
            bool success = true;
            var url = HealthCheckContext.HttpContext.Request.Url;
            var invalidcookies = String.Empty;
            // Access the site home page and check for the click-jack protection header or meta tag
            var useSsl = GlobalSettings.UseSSL || HealthCheckContext.HttpContext.Request.ServerVariables["SERVER_PORT"] == "443";
            var address = string.Format("http{0}://{1}:{2}", useSsl ? "s" : "", url.Host.ToLower(), url.Port);
            var request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = "GET";
            if (request.CookieContainer == null)
                request.CookieContainer = new CookieContainer();
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                foreach (System.Net.Cookie cookie in response.Cookies)
                {
                    if (!cookie.HttpOnly)
                    {
                        success = false;
                        invalidcookies += string.IsNullOrEmpty(invalidcookies) ? cookie.Name : invalidcookies + ", " + cookie.Name;
                    }
                }
                message = success
                    ? _textService.Localize("healthcheck/cookiesAllHttpOnly")
                    : _textService.Localize("healthcheck/cookiesNotAllHttpOnly", new[] { invalidcookies });
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck/httpsCheckInvalidUrl", new[] { address, ex.Message });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Warning
                };
        }
    }
}
