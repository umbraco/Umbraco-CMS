using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))] //this shadows the JsonValueConverter
    public class RelatedLinksEditorValueConvertor : PropertyValueConverterBase
    {
        private readonly UmbracoContext _umbracoContext;

        public RelatedLinksEditorValueConvertor(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return Constants.PropertyEditors.RelatedLinksAlias.Equals(propertyType.PropertyEditorAlias);
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

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<JArray>(sourceString);
                    //update the internal links if we have a context
                    var helper = new UmbracoHelper(_umbracoContext);
                    foreach (var a in obj)
                    {
                        var type = a.Value<string>("type");
                        if (type.IsNullOrWhiteSpace() == false)
                        {
                            if (type == "internal")
                            {
                                var linkId = a.Value<int>("link");
                                var link = helper.NiceUrl(linkId);
                                a["link"] = link;
                            }
                        }
                    }
                    return obj;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<RelatedLinksEditorValueConvertor>("Could not parse the string " + sourceString + " to a json object", ex);
                }
            }

            //it's not json, just return the string
            return sourceString;
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

                    var values = (IEnumerable<string>)inter;
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
                    LogHelper.Error<RelatedLinksEditorValueConvertor>("Could not parse the string " + sourceString + " to a json object", ex);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }
    }
}
