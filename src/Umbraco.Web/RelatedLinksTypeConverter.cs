using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    public class RelatedLinksTypeConverter : TypeConverter
    {
        private readonly UmbracoHelper _umbracoHelper;

        public RelatedLinksTypeConverter(UmbracoHelper umbracoHelper)
        {
            _umbracoHelper = umbracoHelper;
        }

        public RelatedLinksTypeConverter()
        {

        }

        private static readonly Type[] ConvertableTypes = new[]
        {
            typeof(JArray)
        };

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ConvertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType))
                   || base.CanConvertFrom(context, destinationType);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            var relatedLinks = value as RelatedLinks;
            if (relatedLinks == null)
                return null;

            if (TypeHelper.IsTypeAssignableFrom<JArray>(destinationType))
            {
                // Conversion to JArray taken from old value converter

                var obj = JsonConvert.DeserializeObject<JArray>(relatedLinks.PropertyData);

                var umbracoHelper = GetUmbracoHelper();

                //update the internal links if we have a context
                if (umbracoHelper != null)
                {
                    foreach (var a in obj)
                    {
                        var type = a.Value<string>("type");
                        if (type.IsNullOrWhiteSpace() == false)
                        {
                            if (type == "internal")
                            {
                                var linkId = a.Value<int>("link");
                                var link = umbracoHelper.Url(linkId);
                                a["link"] = link;
                            }
                        }
                    }
                }
                return obj;

            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private UmbracoHelper GetUmbracoHelper()
        {
            if (_umbracoHelper != null)
                return _umbracoHelper;

            if (UmbracoContext.Current == null)
            {
                Current.Logger.Warn<RelatedLinksTypeConverter>("Cannot create an UmbracoHelper the UmbracoContext is null");
                return null;
            }

            //DO NOT assign to _umbracoHelper variable, this is a singleton class and we cannot assign this based on an UmbracoHelper which is request based
            return new UmbracoHelper(UmbracoContext.Current, Current.Services, Current.ApplicationCache);
        }
    }
}
