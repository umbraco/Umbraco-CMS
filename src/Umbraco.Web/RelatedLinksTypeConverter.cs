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
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            var convertableTypes = new[]
            {
                typeof(JArray)
            };

            return convertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType))
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
                //update the internal links if we have a context
                if (UmbracoContext.Current != null)
                {
                    var helper = new UmbracoHelper(UmbracoContext.Current);
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
                }
                return obj;

            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
