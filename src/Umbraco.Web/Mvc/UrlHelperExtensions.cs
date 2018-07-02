﻿using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Xml;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Extension methods for UrlHelper
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Utility method for checking for valid proxy urls or redirect urls to prevent Open Redirect security issues
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="url">The url to validate</param>
        /// <param name="callerUrl">The url of the current local domain (to ensure we can validate if the requested url is local without dependency on the request)</param>
        /// <returns>True if it's an allowed url</returns>
        public static bool ValidateProxyUrl(this UrlHelper urlHelper, string url, string callerUrl)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) == false)
            {
                return false;
            }

            if (url.StartsWith("//"))
                return false;

            Uri requestUri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out requestUri))
            {
                if (string.IsNullOrEmpty(callerUrl) == false)
                {
                    Uri localUri;
                    if (Uri.TryCreate(callerUrl, UriKind.RelativeOrAbsolute, out localUri))
                    {
                        // check for local urls

                        //Cannot start with // since that is not a local url
                        if (requestUri.OriginalString.StartsWith("//") == false
                            //cannot be non-absolute and also contain the char : since that will indicate a protocol
                            && (requestUri.IsAbsoluteUri == false && requestUri.OriginalString.Contains(":") == false)
                            //needs to be non-absolute or the hosts must match the current request
                            && (requestUri.IsAbsoluteUri == false || requestUri.Host == localUri.Host))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                //we cannot continue if the url is not absolute
                if (requestUri.IsAbsoluteUri == false)
                {
                    return false;
                }

                // check for valid proxy urls
                var feedProxyXml = XmlHelper.OpenAsXmlDocument(IOHelper.MapPath(SystemFiles.FeedProxyConfig));
                if (feedProxyXml != null &&
                    feedProxyXml.SelectSingleNode(string.Concat("//allow[@host = '", requestUri.Host, "']")) != null)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Returns the base path (not including the 'action') of the MVC controller "SaveFileController"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetSaveFileServicePath(this UrlHelper url)
        {
            var result = url.Action("SavePartialView", "SaveFile", new {area = UmbracoConfig.For.GlobalSettings().GetUmbracoMvcArea()});
            return result.TrimEnd("SavePartialView").EnsureEndsWith('/');
        }

        /// <summary>
        /// Returns the base path (not including the 'action') of the MVC controller "BulkPublishController"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetBulkPublishServicePath(this UrlHelper url)
        {
            var result = url.Action("PublishDocument", "BulkPublish", new { area = UmbracoConfig.For.GlobalSettings().GetUmbracoMvcArea() });
            return result.TrimEnd("PublishDocument").EnsureEndsWith('/');
        }

        /// <summary>
        /// Returns the base path (not including the 'action') of the MVC controller "CoreStringsController"
        /// </summary>
        /// <param name="url">The url helper.</param>
        /// <returns>The base path of the controller.</returns>
        public static string GetCoreStringsControllerPath(this UrlHelper url)
        {
            var result = url.Action("ToSafeAlias", "CoreStrings", new { area = UmbracoConfig.For.GlobalSettings().GetUmbracoMvcArea() });
            return result.TrimEnd("ToSafeAlias");
        }
    }
}
