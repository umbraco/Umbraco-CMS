using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Core;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    public class RelatedLinksTypeConverter : TypeConverter
    {
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

                // Update the internal links if we have a context
                var umbracoContext = UmbracoContext.Current;
                if (umbracoContext != null)
                {
                    foreach (var a in obj)
                    {
                        var type = a.Value<string>("type");
                        if (type.IsNullOrWhiteSpace() == false && type == "internal")
                        {
                            var linkId = a.Value<int>("link");
                            var link = umbracoContext.UrlProvider.GetUrl(linkId);
                            a["link"] = link;
                        }
                    }
                }

                return obj;

            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
