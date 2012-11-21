using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Media;
using Umbraco.Core.ObjectResolution;
using umbraco;

namespace Umbraco.Web.Media
{
    public class ImageUrl
    {

        public static string GetImageUrl(string specifiedSrc, string field, string provider, string parameters,
                                         int? nodeId = null)
        {
            string url;
            IImageUrlProvider p = GetProvider(provider);

            NameValueCollection parsedParameters = string.IsNullOrEmpty(parameters)
                                                       ? new NameValueCollection()
                                                       : HttpUtility.ParseQueryString(parameters);

            if (!string.IsNullOrEmpty(field))
            {
                string fieldValue = string.Empty;
                if (nodeId.HasValue)
                {
                    var contentFromCache = GetContentFromCache(nodeId.GetValueOrDefault(), field);
                    if (contentFromCache != null)
                    {
                        fieldValue = contentFromCache.ToString();
                    }
                    else
                    {
                        var itemPage = new page(content.Instance.XmlContent.GetElementById(nodeId.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
                        var value = itemPage.Elements[field];
                        fieldValue = value != null ? value.ToString() : string.Empty;
                    }
                }
                else
                {
                    var context = HttpContext.Current;
                    if (context != null)
                    {
                        var elements = context.Items["pageElements"] as Hashtable;
                        if (elements != null)
                        {
                            var value = elements[field];
                            fieldValue = value != null ? value.ToString() : string.Empty;
                        }
                    }
                }
                int mediaId;
                if (int.TryParse(fieldValue, out mediaId))
                {
                    //Fetch media
                    url = p.GetImageUrlFromMedia(mediaId, parsedParameters);
                }
                else
                {
                    //assume file path
                    url = p.GetImageUrlFromFileName(fieldValue, parsedParameters);
                }

            }
            else
            {
                url = p.GetImageUrlFromFileName(specifiedSrc, parsedParameters);
            }
            return url;
        }

        private static IImageUrlProvider GetProvider(string provider)
        {
            return ImageUrlProviderResolver.Current.Provider(provider);
        }

        private static object GetContentFromCache(int nodeIdInt, string field)
        {
            object content =
                ContextFactory.Context.Cache[
                    String.Format("contentItem{0}_{1}", nodeIdInt.ToString(CultureInfo.InvariantCulture), field)];
            return content;
        }
    }
}