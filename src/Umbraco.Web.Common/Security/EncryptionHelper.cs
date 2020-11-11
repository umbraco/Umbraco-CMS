using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Web.Common.Constants;

namespace Umbraco.Web.Common.Security
{
    public class EncryptionHelper
    {
        private static IDataProtector CreateDataProtector(IDataProtectionProvider dataProtectionProvider)
        {
            return dataProtectionProvider.CreateProtector(nameof(EncryptionHelper));
        }

        public static string Decrypt(string encryptedString, IDataProtectionProvider dataProtectionProvider)
        {
            return CreateDataProtector(dataProtectionProvider).Unprotect(encryptedString);
        }

        public static string Encrypt(string plainString, IDataProtectionProvider dataProtectionProvider)
        {
            return CreateDataProtector(dataProtectionProvider).Protect(plainString);
        }

        /// <summary>
        /// This is used in methods like BeginUmbracoForm and SurfaceAction to generate an encrypted string which gets submitted in a request for which
        /// Umbraco can decrypt during the routing process in order to delegate the request to a specific MVC Controller.
        /// </summary>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="controllerName"></param>
        /// <param name="controllerAction"></param>
        /// <param name="area"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static string CreateEncryptedRouteString(IDataProtectionProvider dataProtectionProvider, string controllerName, string controllerAction, string area, object additionalRouteVals = null)
        {
            if (dataProtectionProvider == null) throw new ArgumentNullException(nameof(dataProtectionProvider));
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrEmpty(controllerName)) throw new ArgumentException("Value can't be empty.", nameof(controllerName));
            if (controllerAction == null) throw new ArgumentNullException(nameof(controllerAction));
            if (string.IsNullOrEmpty(controllerAction)) throw new ArgumentException("Value can't be empty.", nameof(controllerAction));
            if (area == null) throw new ArgumentNullException(nameof(area));

            //need to create a params string as Base64 to put into our hidden field to use during the routes
            var surfaceRouteParams = $"{ViewConstants.ReservedAdditionalKeys.Controller}={WebUtility.UrlEncode(controllerName)}&{ViewConstants.ReservedAdditionalKeys.Action}={WebUtility.UrlEncode(controllerAction)}&{ViewConstants.ReservedAdditionalKeys.Area}={area}";

            //checking if the additional route values is already a dictionary and convert to querystring
            string additionalRouteValsAsQuery;
            if (additionalRouteVals != null)
            {
                if (additionalRouteVals is Dictionary<string, object> additionalRouteValsAsDictionary)
                    additionalRouteValsAsQuery = additionalRouteValsAsDictionary.ToQueryString();
                else
                    additionalRouteValsAsQuery = additionalRouteVals.ToDictionary<object>().ToQueryString();
            }
            else
                additionalRouteValsAsQuery = null;

            if (additionalRouteValsAsQuery.IsNullOrWhiteSpace() == false)
                surfaceRouteParams += "&" + additionalRouteValsAsQuery;

            return Encrypt(surfaceRouteParams, dataProtectionProvider);
        }

        public static bool DecryptAndValidateEncryptedRouteString(IDataProtectionProvider dataProtectionProvider, string encryptedString, out IDictionary<string, string> parts)
        {
            if (dataProtectionProvider == null) throw new ArgumentNullException(nameof(dataProtectionProvider));
            string decryptedString;
            try
            {
                decryptedString = Decrypt(encryptedString, dataProtectionProvider);
            }
            catch (Exception)
            {
                StaticApplicationLogging.Logger.LogWarning("A value was detected in the ufprt parameter but Umbraco could not decrypt the string");
                parts = null;
                return false;
            }
            var parsedQueryString = HttpUtility.ParseQueryString(decryptedString);
            parts = new Dictionary<string, string>();
            foreach (var key in parsedQueryString.AllKeys)
            {
                parts[key] = parsedQueryString[key];
            }
            //validate all required keys exist
            //the controller
            if (parts.All(x => x.Key != ViewConstants.ReservedAdditionalKeys.Controller))
                return false;
            //the action
            if (parts.All(x => x.Key != ViewConstants.ReservedAdditionalKeys.Action))
                return false;
            //the area
            if (parts.All(x => x.Key != ViewConstants.ReservedAdditionalKeys.Area))
                return false;

            return true;
        }
    }
}
