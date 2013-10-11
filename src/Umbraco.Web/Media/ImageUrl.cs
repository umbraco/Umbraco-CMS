using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Media;
using umbraco;

namespace Umbraco.Web.Media
{
    public class ImageUrl
    {
        [Obsolete("Use TryGetImageUrl() instead")]
        public static string GetImageUrl(string specifiedSrc, string field, string provider, string parameters, int? nodeId = null)
        {
            string url;
            var found = TryGetImageUrl(specifiedSrc, field, provider, parameters, nodeId, out url);

            return found ? url : string.Empty;
        }

        public static bool TryGetImageUrl(string specifiedSrc, string field, string provider, string parameters, int? nodeId, out string url)
        {
            var imageUrlProvider = GetProvider(provider);

            var parsedParameters = string.IsNullOrEmpty(parameters) ? new NameValueCollection() : HttpUtility.ParseQueryString(parameters);

            var queryValues = parsedParameters.Keys.Cast<string>().ToDictionary(key => key, key => parsedParameters[key]);

            if (string.IsNullOrEmpty(field))
            {
                url = imageUrlProvider.GetImageUrlFromFileName(specifiedSrc, queryValues);
                return true;
            }
            else
            {
                var fieldValue = string.Empty;
                if (nodeId.HasValue)
                {
                    var contentFromCache = GetContentFromCache(nodeId.GetValueOrDefault(), field);
                    if (contentFromCache != null)
                    {
                        fieldValue = contentFromCache.ToString();
                    }
                    else
                    {
                        var p = UmbracoContext.Current.ContentCache.GetById(nodeId.GetValueOrDefault());
                        var v = p.GetPropertyValue(field);
                        fieldValue = v == null ? string.Empty : v.ToString();
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

                if (!string.IsNullOrWhiteSpace(fieldValue))
                {
                    int mediaId;
                    url = int.TryParse(fieldValue, out mediaId)
                              ? imageUrlProvider.GetImageUrlFromMedia(mediaId, queryValues)
                              : imageUrlProvider.GetImageUrlFromFileName(fieldValue, queryValues);
                    return true;
                }
            }

            url = string.Empty;
            return false;
        }

        private static IImageUrlProvider GetProvider(string provider)
        {
            return ImageUrlProviderResolver.Current.GetProvider(provider);
        }

        private static object GetContentFromCache(int nodeIdInt, string field)
        {
            var content = ApplicationContext.Current.ApplicationCache.GetCacheItem<object>(
                string.Format("{0}{1}_{2}", CacheKeys.ContentItemCacheKey, nodeIdInt.ToString(CultureInfo.InvariantCulture), field));            
            return content;
        }
    }
}