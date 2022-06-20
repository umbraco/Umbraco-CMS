using System.Xml;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class MultipleTextStringValueConverter : PropertyValueConverterBase
{
    private static readonly string[] NewLineDelimiters = { "\r\n", "\r", "\n" };

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.MultipleTextstring.Equals(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<string>);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
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
        if (string.IsNullOrWhiteSpace(sourceString))
        {
            return Enumerable.Empty<string>();
        }

        // SD: I have no idea why this logic is here, I'm pretty sure we've never saved the multiple txt string
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

        // fall back on normal behaviour
        return values.Any() == false
            ? sourceString.Split(NewLineDelimiters, StringSplitOptions.None)
            : values.ToArray();
    }

    public override object? ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var d = new XmlDocument();
        XmlElement e = d.CreateElement("values");
        d.AppendChild(e);

        var values = (IEnumerable<string>?)inter;
        if (values is not null)
        {
            foreach (var value in values)
            {
                XmlElement ee = d.CreateElement("value");
                ee.InnerText = value;
                e.AppendChild(ee);
            }
        }

        return d.CreateNavigator();
    }
}
