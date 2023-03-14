using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Constants;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public class EncryptionHelper
{
    public static string Decrypt(string encryptedString, IDataProtectionProvider dataProtectionProvider)
        => CreateDataProtector(dataProtectionProvider).Unprotect(encryptedString);

    // TODO: Decide if these belong here... I don't think so since this all has to do with surface controller routes
    // could also just be injected too....
    private static IDataProtector CreateDataProtector(IDataProtectionProvider dataProtectionProvider)
        => dataProtectionProvider.CreateProtector(nameof(EncryptionHelper));

    public static string Encrypt(string plainString, IDataProtectionProvider dataProtectionProvider)
        => CreateDataProtector(dataProtectionProvider).Protect(plainString);

    /// <summary>
    ///     This is used in methods like BeginUmbracoForm and SurfaceAction to generate an encrypted string which gets
    ///     submitted in a request for which
    ///     Umbraco can decrypt during the routing process in order to delegate the request to a specific MVC Controller.
    /// </summary>
    public static string CreateEncryptedRouteString(
        IDataProtectionProvider dataProtectionProvider,
        string controllerName,
        string controllerAction,
        string area,
        object? additionalRouteVals = null)
    {
        if (dataProtectionProvider is null)
        {
            throw new ArgumentNullException(nameof(dataProtectionProvider));
        }

        if (string.IsNullOrEmpty(controllerName))
        {
            throw new ArgumentException($"'{nameof(controllerName)}' cannot be null or empty.", nameof(controllerName));
        }

        if (string.IsNullOrEmpty(controllerAction))
        {
            throw new ArgumentException(
                $"'{nameof(controllerAction)}' cannot be null or empty.",
                nameof(controllerAction));
        }

        if (area is null)
        {
            throw new ArgumentNullException(nameof(area));
        }

        // need to create a params string as Base64 to put into our hidden field to use during the routes
        var surfaceRouteParams =
            $"{ViewConstants.ReservedAdditionalKeys.Controller}={WebUtility.UrlEncode(controllerName)}&{ViewConstants.ReservedAdditionalKeys.Action}={WebUtility.UrlEncode(controllerAction)}&{ViewConstants.ReservedAdditionalKeys.Area}={area}";

        // checking if the additional route values is already a dictionary and convert to querystring
        string? additionalRouteValsAsQuery;
        if (additionalRouteVals != null)
        {
            if (additionalRouteVals is Dictionary<string, object> additionalRouteValsAsDictionary)
            {
                additionalRouteValsAsQuery = additionalRouteValsAsDictionary.ToQueryString();
            }
            else
            {
                additionalRouteValsAsQuery = additionalRouteVals.ToDictionary<object>().ToQueryString();
            }
        }
        else
        {
            additionalRouteValsAsQuery = null;
        }

        if (additionalRouteValsAsQuery.IsNullOrWhiteSpace() == false)
        {
            surfaceRouteParams += "&" + additionalRouteValsAsQuery;
        }

        return Encrypt(surfaceRouteParams, dataProtectionProvider);
    }

    public static bool DecryptAndValidateEncryptedRouteString(
        IDataProtectionProvider dataProtectionProvider,
        string encryptedString,
        [MaybeNullWhen(false)] out IDictionary<string, string?> parts)
    {
        if (dataProtectionProvider == null)
        {
            throw new ArgumentNullException(nameof(dataProtectionProvider));
        }

        string decryptedString;
        try
        {
            decryptedString = Decrypt(encryptedString, dataProtectionProvider);
        }
        catch (Exception)
        {
            StaticApplicationLogging.Logger.LogWarning(
                "A value was detected in the ufprt parameter but Umbraco could not decrypt the string");
            parts = null;
            return false;
        }

        NameValueCollection parsedQueryString = HttpUtility.ParseQueryString(decryptedString);
        parts = new Dictionary<string, string?>();
        foreach (var key in parsedQueryString.AllKeys)
        {
            if (key is not null)
            {
                parts[key] = parsedQueryString[key];
            }
        }

        // validate all required keys exist
        // the controller
        if (parts.All(x => x.Key != ViewConstants.ReservedAdditionalKeys.Controller))
        {
            return false;
        }

        // the action
        if (parts.All(x => x.Key != ViewConstants.ReservedAdditionalKeys.Action))
        {
            return false;
        }

        // the area
        if (parts.All(x => x.Key != ViewConstants.ReservedAdditionalKeys.Area))
        {
            return false;
        }

        return true;
    }
}
