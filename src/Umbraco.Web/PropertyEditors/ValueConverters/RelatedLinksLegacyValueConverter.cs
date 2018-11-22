using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))] //this shadows the JsonValueConverter
    public class RelatedLinksLegacyValueConverter : PropertyValueConverterBase
    {
        private static readonly string[] MatchingEditors = {
            Constants.PropertyEditors.Aliases.RelatedLinks
        };

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger _logger;
        private readonly ServiceContext _services;
        private readonly CacheHelper _appCache;

        public RelatedLinksLegacyValueConverter(IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, CacheHelper appCache, ILogger logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _services = services;
            _appCache = appCache;
            _logger = logger;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
            => MatchingEditors.Contains(propertyType.EditorAlias);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (JArray);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<JArray>(sourceString);
                    //update the internal links if we have a context
                    if (UmbracoContext.Current != null)
                    {
                        var helper = new UmbracoHelper(_umbracoContextAccessor.UmbracoContext, _services, _appCache);
                        foreach (var a in obj)
                        {
                            var type = a.Value<string>("type");
                            if (type.IsNullOrWhiteSpace() == false)
                            {
                                if (type == "internal")
                                {
                                    switch (propertyType.EditorAlias)
                                    {
                                        case Constants.PropertyEditors.Aliases.RelatedLinks:
                                            var strLinkId = a.Value<string>("link");
                                            var udiAttempt = strLinkId.TryConvertTo<Udi>();
                                            if (udiAttempt)
                                            {
                                                var content = helper.PublishedContent(udiAttempt.Result);
                                                if (content == null) break;
                                                a["link"] = helper.Url(content.Id);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    return obj;
                }
                catch (Exception ex)
                {
                    _logger.Error<RelatedLinksLegacyValueConverter>(ex, "Could not parse the string '{Json}' to a json object", sourceString);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

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
                    _logger.Error<RelatedLinksLegacyValueConverter>(ex, "Could not parse the string '{Json}' to a json object", sourceString);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }
    }
}
