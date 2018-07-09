using System;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace umbraco
{
    /// <summary>
    /// Function library for umbraco. Includes various helper-methods and methods to
    /// save and load data from umbraco.
    ///
    /// Especially usefull in XSLT where any of these methods can be accesed using the umbraco.library name-space. Example:
    /// &lt;xsl:value-of select="umbraco.library:NiceUrl(@id)"/&gt;
    /// </summary>
    [Obsolete("v8.kill.kill")]
    public class library
    {
        /// <summary>
        /// Get a media object as an xml object
        /// </summary>
        /// <param name="MediaId">The identifier of the media object to be returned</param>
        /// <param name="deep">If true, children of the media object is returned</param>
        /// <returns>An umbraco xml node of the media (same format as a document node)</returns>
        public static XPathNodeIterator GetMedia(int MediaId, bool deep)
        {
            try
            {
                if (UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration > 0)
                {
                    var xml = Current.ApplicationCache.RuntimeCache.GetCacheItem<XElement>(
                        $"{CacheKeys.MediaCacheKey}_{MediaId}_{deep}",
                        timeout:        TimeSpan.FromSeconds(UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration),
                        getCacheItem:   () => GetMediaDo(MediaId, deep).Item1);

                    if (xml != null)
                    {
                        //returning the root element of the Media item fixes the problem
                        return xml.CreateNavigator().Select("/");
                    }

                }
                else
                {
                    var xml = GetMediaDo(MediaId, deep).Item1;

                    //returning the root element of the Media item fixes the problem
                    return xml.CreateNavigator().Select("/");
                }
            }
            catch(Exception ex)
            {
                Current.Logger.Error<library>("An error occurred looking up media", ex);
            }

            Current.Logger.Debug<library>(() => $"No media result for id {MediaId}");

            var errorXml = new XElement("error", string.Format("No media is maching '{0}'", MediaId));
            return errorXml.CreateNavigator().Select("/");
        }


        private static Tuple<XElement, string> GetMediaDo(int mediaId, bool deep)
        {
            var media = Current.Services.MediaService.GetById(mediaId);
            if (media == null) return null;

            var serialized = EntityXmlSerializer.Serialize(
                Current.Services.MediaService,
                Current.Services.DataTypeService,
                Current.Services.UserService,
                Current.Services.LocalizationService,
                Current.UrlSegmentProviders,
                media,
                deep);
            return Tuple.Create(serialized, media.Path);
        }
    }
}
