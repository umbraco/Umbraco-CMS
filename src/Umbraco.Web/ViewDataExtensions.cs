using System.Collections.Generic;
using System.Web.Mvc;

namespace Umbraco.Web
{
    public static class ViewDataExtensions
    {
        public const string TokenUmbracoPath = "UmbracoPath";
        public const string TokenInstallApiBaseUrl = "InstallApiBaseUrl";
        public const string TokenUmbracoBaseFolder = "UmbracoBaseFolder";
        public const string TokenExternalSignInError = "ExternalSignInError";
        public const string TokenPasswordResetCode = "PasswordResetCode";

        public static bool FromTempData(this ViewDataDictionary viewData, TempDataDictionary tempData, string token)
        {
            if (tempData[token] == null) return false;
            viewData[token] = tempData[token];
            return true;
        }

        public static string GetUmbracoPath(this ViewDataDictionary viewData)
        {
            return (string)viewData[TokenUmbracoPath];
        }

        public static void SetUmbracoPath(this ViewDataDictionary viewData, string value)
        {
            viewData[TokenUmbracoPath] = value;
        }

        public static string GetInstallApiBaseUrl(this ViewDataDictionary viewData)
        {
            return (string)viewData[TokenInstallApiBaseUrl];
        }

        public static void SetInstallApiBaseUrl(this ViewDataDictionary viewData, string value)
        {
            viewData[TokenInstallApiBaseUrl] = value;
        }

        public static string GetUmbracoBaseFolder(this ViewDataDictionary viewData)
        {
            return (string)viewData[TokenUmbracoBaseFolder];
        }

        public static void SetUmbracoBaseFolder(this ViewDataDictionary viewData, string value)
        {
            viewData[TokenUmbracoBaseFolder] = value;
        }

        public static IEnumerable<string> GetExternalSignInError(this ViewDataDictionary viewData)
        {
            return (IEnumerable<string>)viewData[TokenExternalSignInError];
        }

        public static void SetExternalSignInError(this ViewDataDictionary viewData, IEnumerable<string> value)
        {
            viewData[TokenExternalSignInError] = value;
        }

        public static string GetPasswordResetCode(this ViewDataDictionary viewData)
        {
            return (string)viewData[TokenPasswordResetCode];
        }

        public static void SetPasswordResetCode(this ViewDataDictionary viewData, string value)
        {
            viewData[TokenPasswordResetCode] = value;
        }
    }
}
