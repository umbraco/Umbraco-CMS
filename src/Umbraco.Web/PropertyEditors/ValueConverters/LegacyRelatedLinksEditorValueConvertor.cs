using System;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.Extensions;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))] //this shadows the JsonValueConverter
    [PropertyValueType(typeof(JArray))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]    
    public class LegacyRelatedLinksEditorValueConvertor : PropertyValueConverterBase
    {

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters == false)
            {
                return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.RelatedLinksAlias);
            }
            return false;
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<JArray>(sourceString);
                    
                    //update the internal links if we have a context
                    if (UmbracoContext.Current == null) return obj;

                    var helper = new UmbracoHelper(UmbracoContext.Current);
                    foreach (var a in obj)
                    {
                        var type = a.Value<string>("type");
                        if (type.IsNullOrWhiteSpace() == false)
                        {
                            if (type == "internal")
                            {
                                switch (propertyType.PropertyEditorAlias)
                                {
                                    case Constants.PropertyEditors.RelatedLinksAlias:
                                        var intLinkId = a.Value<int>("link");
                                        var intLink = helper.NiceUrl(intLinkId);
                                        a["link"] = intLink;
                                        break;
                                    case Constants.PropertyEditors.RelatedLinks2Alias:
                                        var strLinkId = a.Value<string>("link");
                                        var udiAttempt = strLinkId.TryConvertTo<Udi>();
                                        if (udiAttempt)
                                        {
                                            var content = helper.TypedContent(udiAttempt.Result);
                                            if (content == null) break;
                                            a["link"] = helper.NiceUrl(content.Id);
                                        }
                                        break;
                                }                                    
                            }
                        }
                    }
                    return obj;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<LegacyRelatedLinksEditorValueConvertor>("Could not parse the string " + sourceString + " to a json object", ex);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
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
                    LogHelper.Error<LegacyRelatedLinksEditorValueConvertor>("Could not parse the string " + sourceString + " to a json object", ex);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }
    }
}
