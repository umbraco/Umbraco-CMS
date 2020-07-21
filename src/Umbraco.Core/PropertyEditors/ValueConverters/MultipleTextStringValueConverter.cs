using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MultipleTextStringValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => Constants.PropertyEditors.Aliases.MultipleTextstring.Equals(propertyType.EditorAlias);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (IEnumerable<string>);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        private static readonly string[] NewLineDelimiters = { "\r\n", "\r", "\n" };

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            // data is (both in database and xml):
            // <keyFeatureList>
            //    <values>
            //        <value>Strong</value>
            //        <value>Flexible</value>
            //        <value>Efficient</value>
            //    </values>
            // </keyFeatureList>

            var sourceString = source?.ToString();
            if (string.IsNullOrWhiteSpace(sourceString)) return Enumerable.Empty<string>();

            //SD: I have no idea why this logic is here, I'm pretty sure we've never saved the multiple txt string
            // as xml in the database, it's always been new line delimited. Will ask Stephen about this.
            // In the meantime, we'll do this xml check, see if it parses and if not just continue with
            // splitting by newline
            //
            //   RS: SD/Stephan Please consider post before deciding to remove
            //// https://our.umbraco.com/forum/contributing-to-umbraco-cms/76989-keep-the-xml-values-in-the-multipletextstringvalueconverter
            var values = new List<string>();
            var pos = sourceString.IndexOf("<value>", StringComparison.Ordinal);
            while (pos >= 0)
            {
                pos += "<value>".Length;
                var npos = sourceString.IndexOf("<", pos, StringComparison.Ordinal);
                var value = sourceString.Substring(pos, npos - pos);
                values.Add(value);
                pos = sourceString.IndexOf("<value>", pos, StringComparison.Ordinal);
            }

            // WB: MultipleTextStringPropertyEditor stores this as JSON now - will keep XML above stuff for legacy in case?!
            if (sourceString.DetectIsJson())
            {
                var json = sourceString.ConvertToJsonIfPossible();
                if(json is JArray)
                {
                    var jsonArray = (JArray)json;
                    return jsonArray.Where(x => x["value"] != null).Select(x => x["value"].Value<string>());
                }                
            }

            // fall back on normal behaviour
            return values.Any() == false
                ? sourceString.Split(NewLineDelimiters, StringSplitOptions.None)
                : values.ToArray();
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            var d = new XmlDocument();
            var e = d.CreateElement("values");
            d.AppendChild(e);

            var values = (IEnumerable<string>) inter;
            foreach (var value in values)
            {
                var ee = d.CreateElement("value");
                ee.InnerText = value;
                e.AppendChild(ee);
            }

            return d.CreateNavigator();
        }
    }
}
