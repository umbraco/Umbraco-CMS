using System;
using System.Xml;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.CodeAnnotations;

namespace umbraco.cms.helpers
{
    /// <summary>
    /// Summary description for url.
    /// </summary>
    public class url
    {
        public url()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [UmbracoWillObsolete("Use Umbraco.Core.StringExtensions.ToUrlSegment() instead.")]
        public static string FormatUrl(string url)
        {
            return url.ToUrlSegment();
        }

        /// <summary>
        /// Utility method for checking for valid proxy urls or redirect urls to prevent Open Redirect security issues
        /// </summary>
        /// <param name="url">The url to validate</param>
        /// <param name="callerUrl">The url of the current local domain (to ensure we can validate if the requested url is local without dependency on the request)</param>
        /// <returns>True if it's an allowed url</returns>
        public static bool ValidateProxyUrl(string url, string callerUrl)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                return false;
            }

            Uri requestUri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out requestUri))
            {
                if (!string.IsNullOrEmpty(callerUrl))
                {
                    Uri localUri;
                    if (Uri.TryCreate(callerUrl, UriKind.RelativeOrAbsolute, out localUri))
                    {
                        // check for local urls
                        if (!requestUri.IsAbsoluteUri || requestUri.Host == localUri.Host)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        //TODO: SD: why throw an exception?? shouldn't we just return false ?
                        throw new ArgumentException("CallerUrl is in a wrong format that couldn't be parsed as a valid URI. If you don't want to evaluate for local urls, but just proxy urls then leave callerUrl empty", "callerUrl");
                    }
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
                //TODO: SD: why throw an exception?? shouldn't we just return false ?
                throw new ArgumentException("url is in a wrong format that couldn't be parsed as a valid URI", "url");
                
            }
            return false;
        }
    }
}
