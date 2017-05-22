using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The related links property value converter.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(RelatedLinksLegacyValueConverter), typeof(JsonValueConverter))]
    public class RelatedLinksValueConverter : PropertyValueConverterBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ServiceContext _services;
        private readonly CacheHelper _appCache;
        private readonly ILogger _logger;

        public RelatedLinksValueConverter(IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, CacheHelper appCache, ILogger logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
            _services = services;
            _appCache = appCache;
        }

        /// <summary>
        /// Checks if this converter can convert the property editor and registers if it can.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.RelatedLinksAlias)
                || propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.RelatedLinks2Alias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof (JArray);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Content;
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            var relatedLinksData = JsonConvert.DeserializeObject<IEnumerable<RelatedLink>>(sourceString);
            var relatedLinks = new List<RelatedLink>();
            var helper = new UmbracoHelper(_umbracoContextAccessor.UmbracoContext, _services, _appCache);

            foreach (var linkData in relatedLinksData)
            {
                var relatedLink = new RelatedLink
                {
                    Caption = linkData.Caption,
                    NewWindow = linkData.NewWindow,
                    IsInternal = linkData.IsInternal,
                    Type = linkData.Type,
                    Link = linkData.Link
                };

                int contentId;
                if (int.TryParse(relatedLink.Link, out contentId))
                {
                    relatedLink.Id = contentId;
                    relatedLink = CreateLink(relatedLink);
                }
                else
                {
                    var strLinkId = linkData.Link;
                    var udiAttempt = strLinkId.TryConvertTo<Udi>();
                    if (udiAttempt.Success)
                    {
                        var content = helper.PublishedContent(udiAttempt.Result);
                        if (content != null)
                        {
                            relatedLink.Id = content.Id;
                            relatedLink = CreateLink(relatedLink);
                            relatedLink.Content = content;
                        }
                    }
                }

                if (relatedLink.IsDeleted == false)
                {
                    relatedLinks.Add(relatedLink);
                }
                else
                {
                    _logger.Warn<RelatedLinksValueConverter>($"Related Links value converter skipped a link as the node has been unpublished/deleted (Internal Link NodeId: {relatedLink.Link}, Link Caption: \"{relatedLink.Caption}\")");
                }
            }

            return new RelatedLinks(relatedLinks, sourceString);
        }

        private RelatedLink CreateLink(RelatedLink link)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;

            if (link.IsInternal && link.Id != null)
            {
                if (umbracoContext == null)
                    return null;

                var urlProvider = umbracoContext.UrlProvider;

                link.Link = urlProvider.GetUrl((int)link.Id);
                if (link.Link.Equals("#"))
                {
                    link.IsDeleted = true;
                    link.Link = link.Id.ToString();
                }
                else
                {
                    link.IsDeleted = false;
                }
            }

            return link;
        }

        public override object ConvertInterToXPath(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (inter == null) return null;
            var sourceString = inter.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<Array>(sourceString);

                    var d = new XmlDocument();
                    var e = d.CreateElement("links");
                    d.AppendChild(e);

                    foreach (dynamic link in obj)
                    {
                        var ee = d.CreateElement("link");
                        ee.SetAttribute("title", link.title);
                        ee.SetAttribute("link", link.link);
                        ee.SetAttribute("type", link.type);
                        ee.SetAttribute("newwindow", link.newWindow);

                        e.AppendChild(ee);
                    }

                    return d.CreateNavigator();
                }
                catch (Exception ex)
                {
                    _logger.Error<RelatedLinksValueConverter>("Could not parse the string " + sourceString + " to a json object", ex);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }
    }
}
