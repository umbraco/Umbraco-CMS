using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MultipleTextStringValueConverter : PropertyValueConverterBase
    {
        private class JsonEntry
        {
            public string Value { get; set; }
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => Constants.PropertyEditors.Aliases.MultipleTextstring.Equals(propertyType.EditorAlias);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (IEnumerable<string>);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        private static readonly string[] NewLineDelimiters = { "\r\n", "\r", "\n" };

        private readonly IJsonSerializer _jsonSerializer;

        public MultipleTextStringValueConverter(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            var sourceString = source?.ToString();

            if (string.IsNullOrWhiteSpace(sourceString))
            {
                return Enumerable.Empty<string>();
            }

            // Support for the JSON format
            if (sourceString.StartsWith("[", StringComparison.Ordinal) &&
                sourceString.EndsWith("]", StringComparison.Ordinal))
            {
                try
                {
                    var sourceArray = _jsonSerializer.Deserialize<List<JsonEntry>>(source.ToString());

                    return sourceArray
                        .Select(x => x.Value)
                        .ToArray();
                }
                catch
                {
                    // Ignore, try one of the other formats instead.
                }
            }

            // Support for the XML format
            var pos = sourceString.IndexOf("<value>", StringComparison.Ordinal);

            if (pos >= 0)
            {
                // data is (both in database and xml):
                // <keyFeatureList>
                //    <values>
                //        <value>Strong</value>
                //        <value>Flexible</value>
                //        <value>Efficient</value>
                //    </values>
                // </keyFeatureList>

                //SD: I have no idea why this logic is here, I'm pretty sure we've never saved the multiple txt string
                // as xml in the database, it's always been new line delimited. Will ask Stephen about this.
                // In the meantime, we'll do this xml check, see if it parses and if not just continue with
                // splitting by newline
                //
                //   RS: SD/Stephan Please consider post before deciding to remove
                //// https://our.umbraco.com/forum/contributing-to-umbraco-cms/76989-keep-the-xml-values-in-the-multipletextstringvalueconverter

                var values = new List<string>();

                while (pos >= 0)
                {
                    pos += "<value>".Length;
                    var npos = sourceString.IndexOf("<", pos, StringComparison.Ordinal);
                    var value = sourceString.Substring(pos, npos - pos);
                    values.Add(value);
                    pos = sourceString.IndexOf("<value>", pos, StringComparison.Ordinal);
                }

                if (values.Any())
                {
                    return values.ToArray();
                }
            }

            // Fallback to the original behaviour for backwards compatibility
            return sourceString.Split(NewLineDelimiters, StringSplitOptions.None);
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
